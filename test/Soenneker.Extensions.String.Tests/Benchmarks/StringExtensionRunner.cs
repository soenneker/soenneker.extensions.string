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
    public async System.Threading.Tasks.ValueTask IsNullOrEmpty()
    {
        Summary summary = BenchmarkRunner.Run<IsNullOrEmptyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToUpperInvariant()
    {
        Summary summary = BenchmarkRunner.Run<ToUpperInvariantBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToInt()
    {
        Summary summary = BenchmarkRunner.Run<ToIntBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask RemoveWhiteSpace()
    {
        Summary summary = BenchmarkRunner.Run<RemoveWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask EqualsAny()
    {
        Summary summary = BenchmarkRunner.Run<EqualsAnyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToBytes()
    {
        Summary summary = BenchmarkRunner.Run<ToBytesBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

  //  [Test]
    public async System.Threading.Tasks.ValueTask ToBytesFromBase64()
    {
        Summary summary = BenchmarkRunner.Run<ToBytesFromBase64Benchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToDashesFromWhiteSpace()
    {
        Summary summary = BenchmarkRunner.Run<ToDashesFromWhiteSpaceBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToSplitId()
    {
        Summary summary = BenchmarkRunner.Run<ToSplitIdBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask ToBool()
    {
        Summary summary = BenchmarkRunner.Run<ToBoolBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

//    [Test]
    public async System.Threading.Tasks.ValueTask AddPartitionKey()
    {
        Summary summary = BenchmarkRunner.Run<AddPartitionKeyBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }

   // [Test]
    public async System.Threading.Tasks.ValueTask AddDocumentId()
    {
        Summary summary = BenchmarkRunner.Run<AddDocumentIdBenchmark>(DefaultConf);

        await summary.OutputSummaryToLog();
    }
}


