using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class PackReader : BaseReader
    {
        public PackReader(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback) { }

        protected override void BeginInternal(Stream stream, BlockingQueue writeQueue)
        {
            var buffer = new byte[8];
            var index = 0;
            int length;
            while (!_externalToken.IsCancellationRequested)
            {
                if (stream.Read(buffer, 0, buffer.Length) == 0)
                {
                    break;
                }
                else
                {
                    var blockLength = BitConverter.ToInt32(buffer, 0);
                    var dataIndex = BitConverter.ToInt32(buffer, 4);
                    length = blockLength - 8;
                    var bytes = new byte[length];

                    stream.Read(bytes, 0, length);

                    var block = ByteBlock.Create(dataIndex, bytes, length);

                    while (!_externalToken.IsCancellationRequested && writeQueue.TryEnqueue(block) == false)
                    {
                        Thread.Yield();
                    }

                    index++;
                }
            }
        }
    }
}
