using BenchmarkDotNet.Attributes;
using Candidate = BuilderSnapshot.StringExtension;

[MemoryDiagnoser]
public class BuilderBenchmarks
{
    public IEnumerable<string> Operations => (Environment.GetEnvironmentVariable("BuilderOperations") ?? "Digits,Whitespace,RemoveChar,Scriban,Slug,Phone,Tel,Newlines").Split(',');
    public IEnumerable<int> Lengths => (Environment.GetEnvironmentVariable("BuilderLengths") ?? "32,512,4096").Split(',').Select(int.Parse);
    public IEnumerable<string> Shapes => (Environment.GetEnvironmentVariable("BuilderShapes") ?? "Mixed").Split(',');
    [ParamsSource(nameof(Operations))]
    public string Operation { get; set; } = null!;
    [ParamsSource(nameof(Lengths))] public int Length { get; set; }
    [ParamsSource(nameof(Shapes))] public string Shape { get; set; } = null!;
    private string _text = null!;
    [GlobalSetup]
    public void Setup()
    {
        _text = Input(Operation, Shape, Length);
        string? expected = Current();
        if (Builder() != expected || Buffer() != expected || BuilderSpan() != expected) throw new Exception("Candidate mismatch");
    }
    public static string Input(string operation, string shape, int length)
    {
        if (shape == "Sparse")
            return string.Create(length, operation, static (chars, op) =>
            {
                chars.Fill(op == "Digits" ? 'x' : ' ');
                for (int i = 1023; i < chars.Length; i += 1024) chars[i] = '1';
            });
        if (shape == "Removed") return new string(operation == "Digits" ? 'x' : ' ', length);
        string clean = operation is "Digits" or "Phone" or "Tel" ? "1234567890" : "helloworld";
        string mixed = operation switch
        {
            "Digits" => "+1 (234) 567-8900. ", "Whitespace" => "hello world\tline\r\n", "RemoveChar" => "hello--world-title-",
            "Scriban" => " {{hello}} \"world\"\\folder\r\n", "Slug" => "Hello WORLD___long title!! ", "Newlines" => "hello world\r\nsecond line\r\n",
            _ => "+1 (234) 567-8900. "
        };
        string token = shape switch { "Clean" or "Late" => clean, "Unicode" => "École\u3000à\u2000Zürich\t١२3!{{x}}", "EmptyOutput" => operation == "RemoveChar" ? "---" : " \t{}", _ => mixed };
        string value = string.Concat(Enumerable.Repeat(token, (length + token.Length - 1) / token.Length))[..length];
        if (shape == "Late") value = operation == "Newlines" && value.Length >= 2 ? value[..^2] + "\r\n" : value[..^1] + (operation == "RemoveChar" ? "-" : "\t");
        return value;
    }
    [Benchmark(Baseline = true)] public string? Current() => Operation switch
    {
        "Digits" => Candidate.RemoveNonDigits(_text), "Whitespace" => Candidate.RemoveWhiteSpace(_text),
        "RemoveChar" => Candidate.RemoveAllChar(_text, '-'), "Scriban" => Candidate.ToEscapedForScriban(_text),
        "Slug" => Candidate.Slugify(_text), "Phone" => Candidate.SanitizePhoneNumber(_text), "Newlines" => Candidate.ToUnixLineBreaks(_text), _ => Candidate.ToTelFormat(_text)
    };
    [Benchmark] public string? Builder() => Operation switch
    {
        "Digits" => Candidate.RemoveNonDigitsBuilder(_text), "Whitespace" => Candidate.RemoveWhiteSpaceBuilder(_text),
        "RemoveChar" => Candidate.RemoveAllCharBuilder(_text, '-'), "Scriban" => Candidate.ScribanBuilder(_text),
        "Slug" => Candidate.SlugifyBuilder(_text), "Phone" => Candidate.PhoneBuilder(_text), "Newlines" => Candidate.NewlinesBuilder(_text), _ => Candidate.TelBuilder(_text)
    };
    [Benchmark] public string? Buffer() => Operation switch
    {
        "Digits" => Candidate.RemoveNonDigitsBuffer(_text), "Whitespace" => Candidate.RemoveWhiteSpaceBuffer(_text),
        "RemoveChar" => Candidate.RemoveAllCharBuffer(_text, '-'), "Scriban" => Candidate.ScribanBuffer(_text),
        "Tel" => Candidate.TelBuffer(_text), "Newlines" => Candidate.NewlinesBuffer(_text), _ => Current()
    };
    [Benchmark] public string? BuilderSpan() => Operation switch
    {
        "Digits" => Candidate.RemoveNonDigitsBuilderSpan(_text), "Whitespace" => Candidate.RemoveWhiteSpaceBuilderSpan(_text),
        "RemoveChar" => Candidate.RemoveAllCharBuilderSpan(_text, '-'), "Scriban" => Candidate.ScribanBuilderSpan(_text), _ => Builder()
    };

    [Benchmark] public string? Integrated() => Operation switch
    {
        "Digits" => Soenneker.Extensions.String.StringExtension.RemoveNonDigits(_text),
        "Whitespace" => Soenneker.Extensions.String.StringExtension.RemoveWhiteSpace(_text),
        "RemoveChar" => Soenneker.Extensions.String.StringExtension.RemoveAllChar(_text, '-'),
        "Scriban" => Soenneker.Extensions.String.StringExtension.ToEscapedForScriban(_text),
        "Slug" => Soenneker.Extensions.String.StringExtension.Slugify(_text),
        "Phone" => Soenneker.Extensions.String.StringExtension.SanitizePhoneNumber(_text),
        "Newlines" => Soenneker.Extensions.String.StringExtension.ToUnixLineBreaks(_text),
        _ => Soenneker.Extensions.String.StringExtension.ToTelFormat(_text)
    };
}
