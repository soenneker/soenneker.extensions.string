[![](https://img.shields.io/nuget/v/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.String

Focused string helpers for the transformations that otherwise get reimplemented throughout an application: parsing values without exceptions, cleaning user input, building slugs and link targets, handling Base64, and performing common ordinal comparisons.

## Installation

```bash
dotnet add package Soenneker.Extensions.String
```

## A quick example

```csharp
using Soenneker.Extensions.String;

string? rawPhone = " +1 (888) 773-7326 ";

string sanitized = rawPhone.SanitizePhoneNumber(); // "+18887737326"
string display = sanitized.ToDisplayPhoneNumber(); // "(888) 773-7326"
string link = sanitized.ToTelFormat();              // "tel:+18887737326"
```

The methods are extension methods, so importing `Soenneker.Extensions.String` makes them available directly on `string` and `string?` values.

## Parsing without exception handling

```csharp
double? price = "19.95".ToDouble();       // 19.95
double? invalidPrice = "free".ToDouble(); // null

int count = "42".ToInt();                 // 42
int invalidCount = "many".ToInt();        // 0

DayOfWeek day = "friday".ToEnum<DayOfWeek>();              // DayOfWeek.Friday
DayOfWeek? maybeDay = "eventually".TryToEnum<DayOfWeek>(); // null
```

`ToDouble()` and `ToDecimal()` return `null` when parsing fails. `ToInt()` and `ToLong()` deliberately return `0`, so use those only when zero is an acceptable fallback. Numeric parsing uses US English culture and permits leading or trailing whitespace.

`ToEnum<T>()` is case-insensitive and throws for invalid input. `TryToEnum<T>()` is the nullable, non-throwing version. Both accept enum names and numeric values.

Date helpers use invariant culture:

| Method | Result |
| --- | --- |
| `ToDateTime()` | Parses assuming local time; returns `null` on failure. |
| `ToUtcDateTime()` | Parses and adjusts the result to UTC; returns `null` on failure. |
| `ToDateTimeOffset()` | Preserves an explicit offset; assumes local time when none is present. |
| `ToUtcDateTimeOffset()` | Parses then normalizes to offset `00:00`. |
| `ToIsoDateTimeOffset()` | Accepts only the supported ISO-8601 forms; assumes local time if an offset is omitted. |
| `ToUtcIsoDateTimeOffset()` | Strict ISO parsing followed by UTC normalization. |

## Validation and comparison

```csharp
"abc123".IsAlphaNumeric();       // true
"abc-123".IsAlphaNumeric();      // false
"123".IsNumeric();               // true; ASCII digits only
"".IsNumeric();                  // false

"report.CSV".EndsWithIgnoreCase(".csv"); // true
"ready".EqualsAny(StringComparison.Ordinal, "waiting", "ready"); // true
```

`StartsWithAny()`, `EndsWithAny()`, `ContainsAny()`, and `EqualsAny()` default to ordinal comparison. Pass a `StringComparison` when case-insensitive or culture-aware behavior is wanted. The `*IgnoreCase()` helpers always use `OrdinalIgnoreCase`.

The empty-value helpers differ intentionally:

| Value | `IsNullOrEmpty()` | `IsEmpty()` | `HasContent()` | `IsWhiteSpace()` |
| --- | ---: | ---: | ---: | ---: |
| `null` | `true` | `false` | `false` | `true` |
| `""` | `true` | `true` | `false` | `true` |
| `"  "` | `false` | `false` | `true` | `true` |

`ThrowIfNullOrEmpty()` and `ThrowIfNullOrWhiteSpace()` are guard helpers. They throw `ArgumentNullException` for `null` and `ArgumentException` for the other rejected cases.

GUID validation also distinguishes empty GUIDs and nullable input:

- `IsValidGuid()` accepts `Guid.Empty`; `IsValidPopulatedGuid()` does not.
- The `*NullableGuid()` variants treat `null` as valid, while malformed non-null text remains invalid.
- `ToIntFromGuid()` requires the dashed `D` format and returns a deterministic non-negative integer from the GUID's first four bytes. It throws `FormatException` for other input.

## Cleaning and transforming text

```csharp
"  Alpha  Beta  ".RemoveWhiteSpace(); // "AlphaBeta"
"PO-123-45".RemoveDashes();           // "PO12345"
"abc123xyz".RemoveNonDigits();        // "123"
"one, two, , three".FromCommaSeparatedToList(); // ["one", "two", "three"]

"Hello, ASP.NET World!".Slugify();    // "hello-asp-net-world"
"abcdef".Truncate(3);                 // "abc"
```

`Truncate(length)` never appends an ellipsis. It returns exactly the requested prefix, the original string when it already fits, and `""` for a non-positive length or null/empty input.

`Slugify()` lowercases invariantly, keeps letters and digits, converts whitespace/dashes into a single `-`, preserves underscore runs as a single `_`, removes other punctuation, and does not transliterate accented characters.

Other focused transformations include:

- `RemoveAllChar()`, `RemoveLeadingChar()`, and `RemoveTrailingChar()` remove only the requested character; the leading/trailing variants remove at most one occurrence.
- `SplitTrimmedNonEmpty()` trims pieces and discards empty ones. It returns `null` when no usable pieces remain, whereas `FromCommaSeparatedToList()` returns an empty list.
- `ToDashesFromWhiteSpace()` replaces each whitespace character with `-`; it does not collapse runs. `ToDashesFromPeriods()` replaces `.` with `-`.
- `ToUnixLineBreaks()` changes CRLF (`\r\n`) to LF (`\n`). Lone carriage returns are untouched.
- `ToLowerFirstChar()` and `ToUpperFirstChar()` change only the first character using invariant casing.
- `ToLowerOrdinal()` and `ToUpperOrdinal()` change ASCII letters only. The `*InvariantFast()` methods also handle non-ASCII invariant casing.
- `Mask()` masks the entire value when it has six or fewer characters; longer values expose only their final three characters.

## URL, file, and link helpers

```csharp
"Quarterly.Report.PDF".ToFileExtension(); // "pdf"
"https://example.com/files/report.pdf?download=1".ToFileNameFromUri(); // "report.pdf"

"https://example.com".IsUri();             // true
"C:\\temp\\file.txt".IsUri();            // false
"https://example.com/a b".IsHttpUriLike(); // false
```

`IsUri()` and `ToUri()` require an explicit absolute URI scheme and intentionally reject Windows drive paths. `IsHttpUriLike()` is only a lightweight check: it requires `http://` or `https://` and rejects whitespace/control characters, but it does not fully parse or validate the URI.

`ToEscaped()`/`ToUnescaped()` wrap `Uri.EscapeDataString()` and `Uri.UnescapeDataString()`. `ToFileExtension()` removes the leading dot and lowercases the extension. `ToFileNameFromUri()` accepts absolute URIs and returns `null` for invalid input.

For clickable targets, `ToTelFormat()` and `ToSmsFormat()` remove formatting characters and add a country code when the number does not already begin with `+`; `ToMailToFormat()` simply prefixes `mailto:` and does not validate or escape the address.

## Encoding and Base64

```csharp
string encoded = "hello".ToBase64();           // "aGVsbG8="
string decoded = encoded.ToStringFromBase64(); // "hello"
byte[] utf8 = "hello".ToBytes();
```

`ToStringFromBase64()` accepts standard Base64 and unpadded Base64URL (`-` and `_`). Invalid data throws `FormatException`. `ToBytesFromBase64()` and `ToBytesFromHex()` return an empty array for null/empty input but otherwise use the runtime decoders and propagate formatting errors.

Temporary buffers used by `ToBase64()` and `ToStringFromBase64()` are cleared before pooled storage is returned. The input and returned strings are immutable and remain in managed memory normally.

`GetEncoding()` reads a `charset=` parameter from Content-Type text and falls back to UTF-8 when it is missing, malformed, or unsupported.

## Specialized helpers

- `Shuffle()` uses the library's regular pseudo-random source. `SecureShuffle()` uses `RandomNumberGenerator` and clears temporary buffers; use it when unpredictability matters.
- `ToEscapedForScriban()` removes `{{`/`}}`, changes double quotes to single quotes, normalizes slashes and line breaks, and trims the result. It is targeted sanitization, not a general-purpose HTML or URL encoder.
- `RemoveCodeBlockMarkers()` trims surrounding whitespace and removes outer Markdown triple-backtick fences, including an opening language identifier.
- `ToIds()` splits every colon and preserves empty segments. For a composite `partition:document` identifier, prefer `ToSplitId()` or allocation-free `ToSplitIdRanges()`; both split at the first colon and treat an ID without a colon as both partition and document ID.
- `AddPartitionKey()` and `AddDocumentId()` build composite IDs as `partitionKey:documentId`.
- `ToBool()` returns `true` only for a case-insensitive `"true"` (surrounding whitespace is accepted); every other value returns `false`.
