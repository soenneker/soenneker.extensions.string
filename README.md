[![](https://img.shields.io/nuget/v/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.string/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.string/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Extensions.String.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Extensions.String/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.String
### A collection of useful string extension methods

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