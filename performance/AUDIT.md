# String and dependency performance audit — 2026-09-12

The later [PooledStringBuilder follow-up](BUILDER-AUDIT.md) compares the updated builder against this audit's resulting string implementations and records an additional retained change.

## Outcome

Changes are implemented in four repositories: `soenneker.extensions.string`, `soenneker.extensions.spans.readonly.chars`, `soenneker.extensions.spans.readonly.bytes`, and `soenneker.utils.random`. Public signatures and deterministic output behavior are preserved. Weighted random selection uses a different sampling algorithm, preserving the documented weighting contract rather than a particular random sequence.

`PooledStringBuilder` is excluded at the user's request. Its checkout is unchanged. Character-span joins now write directly into their final string and no longer depend on that package.

This is a source audit plus measured optimization of the runtime library graph. It is not a proof that no future improvement is possible. Results apply to the tested input distributions and machine. Startup costs, other CPUs, native allocations, general Roslyn generator throughput, and application-level performance are not established by these microbenchmarks.

## Retained changes

| Area | Change | Allocation effect |
|---|---|---|
| String whitespace | Search the complete Unicode whitespace set with `SearchValues`; retain a separate path for long ASCII input. | Unchanged inputs still return the same string; changed inputs allocate only the result. |
| Digits and alphanumeric checks | Vectorized ASCII searches with Unicode classification fallback. | Zero allocation for checks and unchanged digit strings. |
| HTTP-like checks | Fast scan for printable ASCII after the existing scheme check; retain Unicode/control fallback. | Zero allocation; remains a heuristic, not URI validation. |
| Ordinal casing | Portable `Vector<ushort>` conversion with a scalar tail and fallback; change only ASCII letters. | One final string when changed, same reference when unchanged. |
| Invariant fast casing | Route ASCII inputs of at least 16 characters through the vectorized ordinal implementation. Preserve existing non-ASCII behavior. | No additional intermediate allocation. |
| Slug generation | Recognize long normalized strings and uppercase ASCII identifiers; reuse unchanged short results; simplify the transforming state machine, copy long valid prefixes/suffixes, and guarantee pool return with `finally`. | Clean slugs allocate nothing; long clean slugs rent no buffer. Changed slugs allocate the final string. |
| String splitting | Reuse the input for a single untrimmed item in `SplitTrimmedNonEmpty`, `FromCommaSeparatedToList`, and `ToIds`. | Remove the copied item string; collection allocation remains. |
| Scriban escaping | Search for characters requiring work before entering the existing transformation. | Clean text reuses the original string. |
| Base64URL decoding | Decode ordinary unpadded URL data directly into the existing byte buffer. Retain normalization for mixed alphabets, padding, and legacy whitespace behavior. Avoid overflow in maximum byte-length calculation. | Remove the temporary character buffer/rental on the direct path; preserve buffer clearing. |
| Single-character shuffles | Reuse the only possible result. | Remove the result allocation for one-character input. |
| Span splitting | Use runtime separator searches; retain exact output arrays and stack/pooled range storage; return rented ranges in `finally`. Reuse the initial separator search. | Final strings and array only in a warmed pool. |
| Span joins | Pass source and trimmed ranges in a ref-struct state to `string.Create`; check output length arithmetic. | Eliminate the pooled character builder and its extra copy; only the result string is allocated. |
| ASCII comparisons | Use runtime vectorized comparisons for the assume-ASCII API and validated ASCII inputs in the safe character/byte APIs. Preserve safe APIs' exact non-ASCII comparisons. | Zero allocation. |
| Byte classification | Vectorized fast path when the bounded probe has no control characters; retain original density, BOM, and probe-boundary rules. | Zero per-call allocation. |
| SHA-256 hex output | Use runtime hex conversion instead of a scalar hex loop and temporary character buffer. | Final string only; destination-span variant remains allocation-free. |
| Random delay logging | Cache strongly typed logging delegates. Preserve cancellation logging and the asynchronous delay behavior. | Disabled debug logging drops from 56 B/op to 0 B/op in the zero-delay benchmark. |
| Weighted selection | Draw once from cumulative weights instead of drawing once per positive weight. Rescale overflowing/subnormal totals and preserve zero-weight exclusion. | Remains allocation-free. |

## Measurement evidence

See [accepted.csv](results/accepted.csv) for the selected measurements, including baseline and changed means, error estimates, allocation data, and the originating run. Full BenchmarkDotNet reports are retained beside it. The run selection is explicit because some experimental candidates were rejected.

Representative measurements (rounded; lengths refer to benchmark parameters, not necessarily the exact generated string length):

| Workload | Parameter | Before | After | Managed allocation before → after |
|---|---:|---:|---:|---:|
| Already normalized slug | 4096 | 3,349 ns | 123 ns | 8,216 B → 0 B |
| Dirty slug with a long valid suffix | 4096 | 3,334 ns | 292 ns | 8,240 B → 8,240 B |
| Alphanumeric ASCII | 4096 | 1,457 ns | 67 ns | 0 B → 0 B |
| Already digit-only string | 4096 | 971 ns | 29 ns | 0 B → 0 B |
| Printable ASCII HTTP-like string | 4096 | 1,551 ns | 30 ns | 0 B → 0 B |
| Invariant lowercase, ASCII | 4096 | 1,969 ns | 286 ns | 8,216 B → 8,216 B |
| Clean Scriban text | 4096 | 3,762 ns | 91 ns | 0 B → 0 B |
| Base64URL decode | 4096 | 11,838 ns | 3,173 ns | 8,216 B → 8,216 B |
| Unsplit string | 4096 | 192 ns | 35 ns | 8,248 B → 32 B |
| Span join of three segments | 4096 | 647 ns | 441 ns | 24,608 B → 24,608 B |
| Byte ASCII comparison | 4096 | 1,946 ns | 122 ns | 0 B → 0 B |
| Content classification, maximum 512-byte probe | 4096 | 154 ns | 15 ns | 0 B → 0 B |
| Delay with disabled debug logging | 0 ms | 20 ns | 5 ns | 56 B → 0 B |

Ordinary padded Base64 remained approximately unchanged. Long ASCII whitespace scanning remained approximately unchanged after restoring the specialized path; Unicode whitespace benefited substantially. Small SHA-256 inputs improved modestly; hashing dominates larger inputs. Error bars and input construction are in the raw reports and [Benchmarks.cs](Benchmarks.cs).

Separate-process slug tests also cover prose, Unicode, uppercase identifiers, whitespace-only inputs and a change at the end. The retained implementation improved all measured cases in that matrix; the large gains for a long valid prefix/suffix must not be generalized to arbitrary text. Exploratory versions that regressed prose or Unicode were replaced. Short-case and long-case benchmark results are both retained.

## Audit coverage and decisions

### String API families

| Methods/family reviewed | Decision |
|---|---|
| `Truncate`, `RemoveLeadingChar`, `RemoveTrailingChar`, `ToShortZipCode` | Retain existing bounds/identity fast paths and single substring allocation. |
| `IsAlphaNumeric`, `IsNumeric`, `RemoveNonDigits` | Improve ASCII scans where scalar work remained. `IsNumeric` already uses a runtime range search. |
| `RemoveWhiteSpace`, `RemoveAllChar`, `RemoveDashes`, `ToDashesFromWhiteSpace`, `ToDashesFromPeriods` | Improve whitespace discovery. Retain exact-sized creation, runtime character counting, and built-in replacement elsewhere. |
| `StartsWithAny`, `EndsWithAny`, `ContainsAny`, both `EqualsAny` forms | Existing ordinal/runtime comparisons are appropriate. Reject the collection specialization after inconsistent results. Keep null and empty semantics unchanged. |
| `EqualsIgnoreCase`, `StartsWithIgnoreCase`, `EndsWithIgnoreCase`, `ContainsIgnoreCase` | Already thin runtime wrappers; no custom replacement justified. |
| `IsNullOrEmpty`, `IsEmpty`, `HasContent`, `IsNullOrWhiteSpace`, `IsWhiteSpace`, argument guards | Existing allocation-free guards and short-circuit behavior retained. |
| Numeric, enum, date/time, date-offset and ISO parsing | Retain runtime parsers, explicit cultures/styles, overflow handling and failure behavior. No custom parser with narrower accepted input introduced. |
| GUID validation and deterministic GUID integer extraction | Retain runtime GUID parsing, stack buffer, and explicit endian handling. |
| First-character casing, full invariant fast casing, ordinal casing | Keep first-character single allocation/identity paths; vectorize applicable full-string paths. |
| Comma splitting, `SplitTrimmedNonEmpty`, `ToIds`, `ToSplitId`, `ToSplitIdRanges` | Reuse unsplit values and improve dependency splitting. Range APIs already avoid materialization. |
| `AddPartitionKey`, `AddDocumentId`, `ToMailToFormat` | Already one final concatenation; retained. |
| `ToBytes`, `ToBytesFromBase64`, `ToBytesFromHex`, `ToMemoryStream` | Retain runtime conversion and direct stream wrapping; returned bytes/stream are required by these APIs. |
| `ToBase64`, `ToStringFromBase64`, `ToUnixLineBreaks` | Improve URL decode; retain UTF-8 semantics, clearing, and built-in CRLF replacement. |
| `Shuffle`, `SecureShuffle` | Keep Fisher–Yates algorithms after runtime substitution regressed; optimize the single-character case only. |
| `Slugify`, `ToEscapedForScriban`, `ToEscaped`, `ToUnescaped` | Improve no-change and transformation paths; keep URI escaping delegated to the runtime. |
| `Mask`, display/sanitize/tel/sms phone formatting | Existing final-sized output or bounded stack/pooled filtering retained. Do not change phone interpretation during a performance audit. |
| File extension/URI filename extraction, `ToUri`, `IsUri`, `IsHttpUriLike` | Improve the printable ASCII heuristic path; retain full runtime URI validation and path behavior. |
| `RemoveCodeBlockMarkers`, `GetEncoding` | Existing span slicing, identity return, and cached common encodings retained. Unknown encodings still require name materialization/runtime lookup. |

### Dependency graph

Exact source revisions and project paths are recorded in [baseline.json](baseline.json).

| Dependency | Audit result |
|---|---|
| `Soenneker.Extensions.Spans.Readonly.Chars` | Improve split/join/comparison paths. Retain span trim/range helpers, alternate hash-set lookup for token insertion, bounded encoding buffers, stateful streaming SHA-256 encoding and buffer clearing. |
| `Soenneker.Extensions.Spans.Readonly.Bytes` | Improve hex formatting, safe ASCII comparison and common classification. Preserve content heuristics and the 512-byte probe limit. |
| `Soenneker.Extensions.Char` | ASCII arithmetic and Unicode runtime fallbacks are already allocation-free. No replacement justified; exhaustive classification checks exercise these semantics. |
| `Soenneker.Culture.English.US` | Single read-only cached culture instance. Retain culture identity and parsing semantics. |
| `Soenneker.Utils.Random` | Improve weighted selection and logging. Retain `Random.Shared` wrappers and existing decimal/range generation contracts. |
| `Soenneker.Hashing.Sha256` | Runtime hashing, destination spans, fixed-time verification and asynchronous sequential file access retained. A proposed async-wrapper removal was rejected to preserve exception timing. No retained source changes. |
| `Soenneker.Enums.ContentKinds` | Existing singleton values and generated equality require no per-classification allocation. No source changes. |
| `Soenneker.Gen.EnumValues` | Reviewed generator setup, emission model caching and generated `ContentKind` representation. No runtime dependency hot-path change needed; general generator/build throughput was not benchmarked. |
| `Soenneker.Utils.PooledStringBuilders` | Excluded from modification per user instruction. Removed from the character-span dependency graph; original source is used only for baseline comparisons. |
| Microsoft logging/DI and .NET runtime | Retain supported platform implementations. Optimize their call sites rather than fork framework packages. Test-only benchmarking/assertion/test-runner packages are outside the production dependency audit. |

## Validation

- String: 169 MTP/TUnit tests passed.
- Character spans: 100 MTP/TUnit tests passed.
- Byte spans: 3 MTP/TUnit tests passed, including SHA-256 destination boundaries and classification probes.
- Random utilities: 16 MTP/TUnit tests passed, including structured logging, invalid/zero/overflow/subnormal weights and distribution smoke checks.
- 466,212 deterministic differential/reference checks passed against the pinned original source and runtime reference operations, both with hardware intrinsics enabled and disabled.
- Randomized verification uses a fixed seed. Statistical random-selection tests use wide bounds; they are regression checks, not proofs of distribution quality.
- Published-package builds and local dependency builds are distinct; both were checked. The combined source graph is tested using opt-in references. See the reproduction and release instructions in [README.md](README.md).

## Compatibility notes and remaining limits

Several pre-existing behaviors deserve a separate API/bug decision rather than a silent change here: non-ASCII `ToLowerInvariantFast` has a prefix-dependent runtime fallback; invariant-fast methods are not interchangeable with full Unicode string casing; Scriban escaping normalizes internal Unicode whitespace despite comments about preserving it; the array `EqualsAny` overload treats null spans like empty spans while the enumerable overload uses string null semantics; phone URI formatting retains existing leading-plus behavior. Differential checks intentionally preserve these behaviors.

The benchmark suite covers selected representative hot paths, not every public method and input distribution. It cannot establish that future JITs, architecture-specific implementations, or different allocation/throughput tradeoffs will never improve these libraries. The retained changes have concrete measured benefits and regression coverage; unchanged methods are documented above rather than claimed to be mathematically optimal.

Dependency package releases and reference updates are required to distribute the entire improvement set. The validation above covers the audited source changes; package publication and downstream adoption are separate release steps.
