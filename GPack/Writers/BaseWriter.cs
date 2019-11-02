using gpack.Common;
using gpack.Queues;
using System.IO;
using System.Threading;

namespace gpack.Writers
{
    internal abstract class BaseWriter : BaseType
    {
        public BaseWriter(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress) { }

        public void Write(Stream stream, ReadQueue writeQueue)
        {
            try
            {
                foreach (var bytes in writeQueue)
                {
                    if (bytes != null)
                    {
                        WriteInternal(bytes, stream);
                    }
                }
            }
            catch (System.Exception ex)
            {
                _progress?.Error(ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        protected abstract void WriteInternal(byte[] bytes, Stream stream);
    }
}
