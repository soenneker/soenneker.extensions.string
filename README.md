[![](https://img.shields.io/nuget/v/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/main.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/main.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)

# Soenneker.Extensions.String
### An extension library containing useful string operations

## Installation

```
Install-Package Soenneker.Extensions.String
```

## Usage

```csharp
string test = "522";

test.IsNumeric(); //true

test.Truncate(1); //"5"
```

```csharp
string test = "this string&is%bad#for\\urls"

test.Slugify() // "this-string-is-bad-for-urls"
```