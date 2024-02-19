[![](https://img.shields.io/nuget/v/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.String
### A collection of useful string extension methods

## Installation

```
dotnet add package Soenneker.Extensions.String
```

## Usage

### Truncate()

```csharp
string longString = "This is a long string that needs to be truncated";
string truncatedString = longString.Truncate(10);
// truncatedString = "This is a ..."
```

### IsAlphaNumeric()

```csharp
string alphanumeric = "abc123";
bool isAlphanumeric = alphanumeric.IsAlphaNumeric();
// isAlphanumeric = true

string nonAlphanumeric = "abc123!";
bool isNonAlphanumeric = nonAlphanumeric.IsAlphaNumeric();
// isNonAlphanumeric = false
```

### Slugify()

```csharp
string test = "this string&is%bad#for\\urls"

test.Slugify() // "this-string-is-bad-for-urls"
```

### ToDouble()

```csharp
string numericString = "3.14";
double? doubleValue = numericString.ToDouble();
// doubleValue = 3.14

string nonNumericString = "abc";
double? nonDoubleValue = nonNumericString.ToDouble();
// nonDoubleValue = null
```

### RemoveNonDigits()
```csharp
string stringWithNonDigits = "abc123xyz456";
string digitsOnly = stringWithNonDigits.RemoveNonDigits();
// digitsOnly = "123456"
```

### Shuffle()
```csharp
string originalString = "hello";
string shuffledString = originalString.Shuffle();
// shuffledString = "olhel"
```

... and more
