using System.Threading.Tasks;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Soenneker.Benchmarking.Extensions.Summary;
using Soenneker.Tests.Benchmark;

namespace Soenneker.Extensions.String.Tests.Benchmarks;

public class StringExtensionRunner : BenchmarkTest
{
    public StringExtensionRunner() : base()
    {
    }

   // [Test]
    public async ValueTask IsNullOrEmpty()
    {
        Summary summary = BenchmarkRunner.Run<IsNullOrEmptyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToUpperInvariant()
    {
        Summary summary = BenchmarkRunner.Run<ToUpperInvariantBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToInt()
    {
        Summary summary = BenchmarkRunner.Run<ToIntBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask RemoveWhiteSpace()
    {
        Summary summary = BenchmarkRunner.Run<RemoveWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask EqualsAny()
    {
        Summary summary = BenchmarkRunner.Run<EqualsAnyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToBytes()
    {
        Summary summary = BenchmarkRunner.Run<ToBytesBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

  //  [Test]
    public async ValueTask ToBytesFromBase64()
    {
        Summary summary = BenchmarkRunner.Run<ToBytesFromBase64Benchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToDashesFromWhiteSpace()
    {
        Summary summary = BenchmarkRunner.Run<ToDashesFromWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToSplitId()
    {
        Summary summary = BenchmarkRunner.Run<ToSplitIdBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask ToBool()
    {
        Summary summary = BenchmarkRunner.Run<ToBoolBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

//    [Test]
    public async ValueTask AddPartitionKey()
    {
        Summary summary = BenchmarkRunner.Run<AddPartitionKeyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }

   // [Test]
    public async ValueTask AddDocumentId()
    {
        Summary summary = BenchmarkRunner.Run<AddDocumentIdBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog(OutputHelper, CancellationToken);
    }
}