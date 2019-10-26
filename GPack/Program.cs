using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CommandLine;
using GPack.CommandLine;

namespace GPack
{
    class Program
    {
        private static CancellationTokenSource _cts;

        static int Main(string[] args)
        {
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
            _cts.Cancel();
        }

        private static int CompressCommand(Compress opts)
        {
            var stopwatch = Stopwatch.StartNew();
            if (CheckPaths(opts.Source, opts.Target))
            {
                var compressor = new Compressor(_cts.Token);
                TokenRegistration(compressor.InnerToken);
                compressor.Pack(opts.Source, opts.Target);

                if (compressor.Progress?.Exception == null)
                {
                    Console.WriteLine($"Total compress time is {stopwatch.ElapsedMilliseconds:n0} ms");
                    return 0;
                }
                else
                {
                    Console.WriteLine(compressor.Progress.Exception.Message);
                }
            }

            return 1;
        }

        private static int DecompressCommand(Decompress opts)
        {
            var stopwatch = Stopwatch.StartNew();
            if (CheckPaths(opts.Source, opts.Target))
            {
                var compressor = new Compressor(_cts.Token);
                TokenRegistration(compressor.InnerToken);
                compressor.UnPack(opts.Source, opts.Target);

                if (compressor.Progress?.Exception == null)
                {
                    Console.WriteLine($"Total compress time is {stopwatch.ElapsedMilliseconds:n0} ms");
                    return 0;
                }
                else
                {
                    Console.WriteLine(compressor.Progress.Exception.Message);
                }
            }

            return 1;
        }

        private static  bool CheckPaths(string sourceFilePath, string targetFilePath)
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
