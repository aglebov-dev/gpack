using System;
using System.Threading;

namespace GPack
{
    public class PackWriter: BaseWriter
    {
        public PackWriter(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback) { }

        protected override byte[] GetBytes(ByteBlock block)
        {
            return block.Bytes;
        }
    }
}
