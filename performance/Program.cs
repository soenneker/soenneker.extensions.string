using BenchmarkDotNet.Running;

if (args.Length == 2 && args[0] == "--cold-builder")
    ColdBuilderProbe.Run(args[1]);
else if (args.Contains("--verify-builder"))
    BuilderVerification.Run();
else if (args.Contains("--verify"))
    Verification.Run();
else
    BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
