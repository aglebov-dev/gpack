using gpack.Common;
using System.IO;
using System.Threading;

namespace gpack.Writers
{
    internal class Writer : BaseWriter
    {
        public Writer(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress) { }

        protected override void WriteInternal(byte[] bytes, Stream stream)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
