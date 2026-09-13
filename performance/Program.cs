using BenchmarkDotNet.Running;

if (args.Contains("--verify"))
    Verification.Run();
else
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
