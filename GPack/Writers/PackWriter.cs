using gpack.Common;
using System.IO;
using System.Threading;

namespace gpack.Writers
{
    internal class PackWriter: BaseWriter
    {
        public PackWriter(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress) { }

        protected override void WriteInternal(byte[] bytes, Stream stream)
        {
            stream.Write(System.BitConverter.GetBytes(bytes.Length), 0, 4);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
