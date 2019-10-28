using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class Compressor
    {
        private readonly Reader _reader;
        private readonly PackWriter _packWriter;
        private readonly PackReader _packReader;
        private readonly Writer _writer;
        private readonly Exchanger _exchanger;
        private readonly CancellationTokenSource _innerToken;

        public CancellationToken InnerToken => _innerToken.Token;
        public Progress Progress { get;  }

        public Compressor(CancellationToken token)
        {
            _reader = new Reader(token, ProgressHandler);
            _writer = new Writer(token, ProgressHandler);
            _packWriter = new PackWriter(token, ProgressHandler);
            _packReader = new PackReader(token, ProgressHandler);
            _exchanger = new Exchanger(token, ProgressHandler);

            Progress = new Progress();

            _innerToken = CancellationTokenSource.CreateLinkedTokenSource
            (
                _reader.CancellationToken,
                _writer.CancellationToken,
                _packWriter.CancellationToken,
                _packReader.CancellationToken,
                _exchanger.CancellationToken
            );
        }

        private void ProgressHandler(Exception exception)
        {
            if (exception != null)
            {
                Progress.Error(exception);
            }
        }

        public void Pack(string source, string target)
        {
            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(target))
            {
                var data = _reader.BeginReadAsync(readStream);
                var compressedData = CompressDataAsync(data);
                _packWriter.Write(writeStream, compressedData);
            }
        }

        public void UnPack(string source, string target)
        {
            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(target))
            {
                var data = _packReader.BeginReadAsync(readStream);
                var decompressData = DecompressDataAsync(data);
                _writer.Write(writeStream, decompressData);
            }
        }

        private BlockingQueue CompressDataAsync(BlockingQueue source)
        {
            var result = new BlockingQueue();
            _exchanger.BeginCompressExchangeAsync(source, result);
            return result;
        }

        private BlockingQueue DecompressDataAsync(BlockingQueue source)
        {
            var result = new BlockingQueue();
            _exchanger.BeginDecompressExchangeAsync(source, result);
            return result;
        }
    }
}
