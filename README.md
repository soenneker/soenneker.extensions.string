[![](https://img.shields.io/nuget/v/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.String
A highly optimized library of string extension methods designed to improve performance, readability, and efficiency in .NET applications. This library is ideal for developers looking to streamline common string operations while adhering to best practices and achieving maximum performance.

## Features

- **High Performance:** All methods are optimized for speed and low memory usage, ensuring top-notch performance in critical applications.
- **Comprehensive Functionality:** Includes a wide range of utility methods for common string operations, such as trimming, splitting, joining, and formatting.
- **Readability-First:** Enhances code clarity with intuitive method names and streamlined syntax.
- **Error-Resilient:** Methods are built to handle edge cases gracefully, minimizing potential bugs and exceptions.
- **Lightweight:** Minimal overhead and dependencies, making it perfect for high-performance applications.

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
