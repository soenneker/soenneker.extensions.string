using System;
using System.Buffers;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

[MemoryDiagnoser]
public class ToStringFromBase64Benchmark
{
    private string _value = null!;

    [GlobalSetup]
    public void Setup() => _value = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('a', 8192)));

    [Benchmark]
    public string PooledChars() => _value.ToStringFromBase64();

    [Benchmark(Baseline = true)]
    public string AllocatedChars() => Legacy(_value);

    private static string Legacy(string value)
    {
        ReadOnlySpan<char> input = value;
        int pad = input.Length & 3;
        int extraPad = pad == 0 ? 0 : 4 - pad;
        int charLength = input.Length + extraPad;
        var chars = new char[charLength];

        for (var i = 0; i < input.Length; i++)
        {
            char c = input[i];
            chars[i] = c == '-' ? '+' : c == '_' ? '/' : c;
        }

        for (var i = 0; i < extraPad; i++)
            chars[input.Length + i] = '=';

        byte[] bytes = ArrayPool<byte>.Shared.Rent(charLength * 3 / 4);
        try
        {
            if (!Convert.TryFromBase64Chars(chars, bytes, out int written))
                throw new FormatException();
            return Encoding.UTF8.GetString(bytes, 0, written);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}
