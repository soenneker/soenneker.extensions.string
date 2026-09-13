using C = BuilderSnapshot.StringExtension;

internal static class ColdBuilderProbe
{
    // Run each variant in a fresh process; warm only small inputs, never a large pool bucket.
    public static void Run(string variant)
    {
        Func<string, string?> transform = variant switch
        {
            "Current" => C.RemoveNonDigits,
            "Builder" => C.RemoveNonDigitsBuilderGrowTail,
            "Buffer" => C.RemoveNonDigitsBufferTail,
            _ => throw new ArgumentException("Use Current, Builder, or Buffer.")
        };
        for (int i = 0; i < 1000; i++) transform("x123x");
        // Exercise long vector searches without renting a character buffer.
        transform(new string('1', 65536));
        string input = BuilderBenchmarks.Input("Digits", "Sparse", 65536);
        long before = GC.GetAllocatedBytesForCurrentThread();
        string? output = transform(input);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (output != new string('1', 64)) throw new Exception("Cold probe mismatch");
        before = GC.GetAllocatedBytesForCurrentThread();
        string? repeated = transform(input);
        long warmAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        if (repeated != output) throw new Exception("Repeated probe mismatch");
        Console.WriteLine($"{variant}: input={input.Length}, output={output.Length}, first large changed call={allocated} B, repeated={warmAllocated} B");
    }
}
