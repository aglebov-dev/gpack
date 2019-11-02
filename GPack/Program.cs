using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CommandLine;
using gpack.CommandLine;

namespace gpack
{
    class Program
    {
        private static CancellationTokenSource _cts;

        static int Main(string[] args)
        {
            args = new[]
            {
                "decompress",
                "d:\\x.jpg.pack",
                "d:\\x.unpack.jpg"
            };
            _cts = new CancellationTokenSource();
            Console.CancelKeyPress += Canceled;

            return Parser.Default.ParseArguments<Compress, Decompress>(args)
                .MapResult(
                    (Compress x) => CompressCommand(x), 
                    (Decompress x) => DecompressCommand(x), 
                    err => 1);
        }

        private static void Canceled(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey == ConsoleSpecialKey.ControlC || e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                _cts.Cancel();
            }
        }

        private static int CompressCommand(Compress opts)
        {
            return Command(true, opts);
        }

        private static int DecompressCommand(Decompress opts)
        {
            return Command(false, opts);
        }

        private static int Command(bool isPack, Command opts)
        {
            var stopwatch = Stopwatch.StartNew();
            if (CheckSourcePath(opts.Source))
            {
                var compressor = new Compressor(_cts.Token);
                TokenRegistration(compressor.InnerToken);

                if (isPack)
                {
                    compressor.Pack(opts.Source, opts.Target);
                }
                else
                {
                    compressor.UnPack(opts.Source, opts.Target);
                }

                if (!compressor.HasError)
                {
                    Console.WriteLine($"Total compress time is {stopwatch.ElapsedMilliseconds:n0} ms");
                    return 0;
                }
            }

            return 1;
        }

        private static  bool CheckSourcePath(string sourceFilePath)
        {
            var source = new FileInfo(sourceFilePath);
            if (!source.Exists)
            {
                Console.WriteLine($"File '{source.FullName}' wasn't found");
                return false;
            }

            return true;
        }

        private static void TokenRegistration(CancellationToken token)
        {
            var registrator = default(CancellationTokenRegistration);
            registrator = token.Register(() =>
            {
                registrator.Dispose();
                _cts.Cancel();
            });
        }
    }
}
