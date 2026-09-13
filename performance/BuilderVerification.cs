using C = BuilderSnapshot.StringExtension;
using New = Soenneker.Extensions.String.StringExtension;

internal static class BuilderVerification
{
    public static void Run()
    {
        int checks = 0;
        void Equal(Func<string?> expected, params Func<string?>[] candidates)
        {
            (string? Value, Type? Error) Invoke(Func<string?> call)
            {
                try { return (call(), null); } catch (Exception ex) { return (null, ex.GetType()); }
            }
            var baseline = Invoke(expected);
            foreach (var candidate in candidates)
            {
                checks++;
                if (Invoke(candidate) != baseline) throw new Exception($"Builder differential check {checks} failed");
            }
        }
        void Check(string? value)
        {
            Equal(() => C.RemoveNonDigits(value), () => New.RemoveNonDigits(value));
            Equal(() => C.RemoveWhiteSpace(value), () => New.RemoveWhiteSpace(value));
            Equal(() => C.RemoveNonDigits(value), () => C.RemoveNonDigitsBuilderGrowTail(value));
            Equal(() => C.RemoveWhiteSpace(value), () => C.RemoveWhiteSpaceBuilderGrowTail(value));
            Equal(() => C.ToEscapedForScriban(value), () => New.ToEscapedForScriban(value));
            Equal(() => C.RemoveNonDigits(value), () => C.RemoveNonDigitsBuilder(value), () => C.RemoveNonDigitsBuffer(value), () => C.RemoveNonDigitsBuilderSpan(value), () => C.RemoveNonDigitsBuilderTail(value), () => C.RemoveNonDigitsBufferTail(value));
            Equal(() => C.RemoveWhiteSpace(value), () => C.RemoveWhiteSpaceBuilder(value), () => C.RemoveWhiteSpaceBuffer(value), () => C.RemoveWhiteSpaceBuilderSpan(value), () => C.RemoveWhiteSpaceBuilderTail(value), () => C.RemoveWhiteSpaceBufferTail(value));
            Equal(() => C.RemoveAllChar(value, '-'), () => C.RemoveAllCharBuilder(value, '-'), () => C.RemoveAllCharBuffer(value, '-'), () => C.RemoveAllCharBuilderSpan(value, '-'));
            Equal(() => C.ToEscapedForScriban(value), () => C.ScribanBuilder(value), () => C.ScribanBuffer(value), () => C.ScribanBuilderSpan(value));
            Equal(() => C.ToUnixLineBreaks(value!), () => C.NewlinesBuilder(value!), () => C.NewlinesBuffer(value!));
            Equal(() => C.Slugify(value), () => C.SlugifyBuilder(value));
            Equal(() => C.SanitizePhoneNumber(value!), () => C.PhoneBuilder(value!));
            foreach (int country in new[] { 1, -44, 0, int.MinValue, int.MaxValue })
            {
                Equal(() => C.ToTelFormat(value!, country), () => C.TelBuilder(value!, country), () => C.TelBuffer(value!, country));
                Equal(() => C.ToTelFormat(value!, country), () => New.ToTelFormat(value!, country));
                Equal(() => C.ToSmsFormat(value!, country), () => New.ToSmsFormat(value!, country));
            }
        }
        Check(null); Check("");
        for (int i = 0; i <= char.MaxValue; i++) Check("a1" + (char)i + "{{\t+2}} ");
        Random random = new(104729);
        const string alphabet = "abcXYZ09+-_ \r\n\t{}\"\\éÉ\u3000\u2000١२\ud800\udc00";
        for (int i = 0; i < 3000; i++)
        {
            int length = i < 1025 ? i : random.Next(4097);
            Check(string.Create(length, random, static (destination, rng) =>
            {
                for (int j = 0; j < destination.Length; j++) destination[j] = alphabet[rng.Next(alphabet.Length)];
            }));
        }
        foreach (string operation in new[] { "Digits", "Whitespace", "RemoveChar", "Scriban", "Slug", "Phone", "Tel" })
        foreach (string shape in new[] { "Clean", "Mixed", "Late", "Unicode", "EmptyOutput", "Sparse", "Removed" })
        foreach (int length in new[] { 1, 31, 32, 127, 128, 511, 512, 513, 4096, 65536 })
            Check(BuilderBenchmarks.Input(operation, shape, length));
        Console.WriteLine($"Builder candidates: {checks:N0} differential checks passed.");
    }
}
