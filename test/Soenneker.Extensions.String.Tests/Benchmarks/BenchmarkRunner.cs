using System.Threading.Tasks;
using BenchmarkDotNet.Reports;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;
using Xunit;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

public class BenchmarkRunner : BenchmarkTest
{
    public BenchmarkRunner(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

  //  [Fact]
    public async ValueTask IsNullOrEmpty()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<IsNullOrEmptyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [Fact]
    public async ValueTask ToUpperInvariant()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToUpperInvariantBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Fact]
    public async ValueTask ToInt()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToIntBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Fact]
    public async ValueTask RemoveWhiteSpace()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<RemoveWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Fact]
    public async ValueTask EqualsAny()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<EqualsAnyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [Fact]
    public async ValueTask ToBytes()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToBytesBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Fact]
    public async ValueTask ToBytesFromBase64()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToBytesFromBase64Benchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [Fact]
    public async ValueTask ToDashesFromWhiteSpace()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToDashesFromWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [Fact]
    public async ValueTask ToSplitId()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToSplitIdBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

    //[Fact]
    public async ValueTask ToBool()
    {
        Summary summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ToBoolBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }
}