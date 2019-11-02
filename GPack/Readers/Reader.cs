using gpack.Common;
using gpack.Queues;
using System.IO;
using System.Threading;

namespace gpack.Readers
{
    internal class Reader : BaseReader
    {
        public Reader(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress) { }

        protected override void BeginInternal(Stream stream, WriteQueue writeQueue)
        {
            var buffer = new byte[Constants.FILE_READ_PAGE_SIZE];
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
                    var result = new byte[length];
                    System.Array.Copy(buffer, 0, result, 0, length);
                    writeQueue.Add(index, result);

                    index++;
                }
            }

            writeQueue.Complete();
        }
    }
}
