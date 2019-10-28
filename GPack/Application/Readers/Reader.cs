using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class Reader : BaseReader
    {
        public Reader(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback) { }

        protected override void BeginInternal(Stream stream, BlockingQueue writeQueue)
        {
            var buffer = new byte[Constants.FILE_READ_BUFFER_SIZE];
            var index = 0;
            int length;
            while (!_externalToken.IsCancellationRequested)
            {
                if ((length = stream.Read(buffer, 0, buffer.Length)) == 0)
                {
                    break;
                }
                else
                {
                    var block = ByteBlock.Create(index, buffer, length);
                    while (!_externalToken.IsCancellationRequested && !writeQueue.TryEnqueue(block))
                    {
                        Thread.Yield();
                    }

                    index++;
                }
            }
        }
    }
}
