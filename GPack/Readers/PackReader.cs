using gpack.Common;
using gpack.Queues;
using System.IO;
using System.Threading;

namespace gpack.Readers
{
    internal class PackReader : BaseReader
    {
        public PackReader(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress) { }

        protected override void BeginInternal(Stream stream, WriteQueue writeQueue)
        {
            var buffer = new byte[4];
            int index = 0;
            while (!_externalToken.IsCancellationRequested)
            {
                if (stream.Read(buffer, 0, buffer.Length) == 0)
                {
                    break;
                }
                else
                {
                    var blockLength = System.BitConverter.ToInt32(buffer, 0);
                    var bytes = new byte[blockLength];

                    stream.Read(bytes, 0, bytes.Length);
                    writeQueue.Add(index, bytes);
                    index++;
                }
            }

            writeQueue.Complete();
        }
    }
}
