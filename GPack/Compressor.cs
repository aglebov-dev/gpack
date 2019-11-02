using gpack.Common;
using gpack.Exchange;
using gpack.Queues;
using gpack.Readers;
using gpack.Writers;
using System.IO;
using System.Threading;

namespace gpack
{
    internal class Compressor
    {
        private readonly Progress _progress;
        private readonly Reader _reader;
        private readonly PackWriter _packWriter;
        private readonly PackReader _packReader;
        private readonly Writer _writer;
        private readonly Exchanger _exchanger;
        private readonly CancellationTokenSource _innerToken;

        public CancellationToken InnerToken => _innerToken.Token;
        public bool HasError => _progress.Exception != null;

        public Compressor(CancellationToken token)
        {
            _progress = new Progress();
            _reader = new Reader(token, _progress);
            _writer = new Writer(token, _progress);
            _packWriter = new PackWriter(token, _progress);
            _packReader = new PackReader(token, _progress);
            _exchanger = new Exchanger(token, _progress);

            _innerToken = CancellationTokenSource.CreateLinkedTokenSource
            (
                _reader.CancellationToken,
                _writer.CancellationToken,
                _packWriter.CancellationToken,
                _packReader.CancellationToken,
                _exchanger.CancellationToken
            );
        }

        public void Pack(string source, string target)
        {
            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(target))
            {
                var compressedData = new ReadQueue();
                var data = _reader.BeginReadAsync(readStream);
                _exchanger.BeginCompressExchangeAsync(data, compressedData);
                _packWriter.Write(writeStream, compressedData);
            }
        }

        public void UnPack(string source, string target)
        {
            using (var readStream = File.OpenRead(source))
            using (var writeStream = File.OpenWrite(target))
            {
                var result = new ReadQueue();
                var data = _packReader.BeginReadAsync(readStream);
                _exchanger.BeginDecompressExchangeAsync(data, result);
                _writer.Write(writeStream, result);
            }
        }
    }
}
