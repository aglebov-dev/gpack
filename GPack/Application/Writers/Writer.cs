using System;
using System.Threading;

namespace GPack
{

    public class Writer : BaseWriter
    {
        public Writer(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback) { }

        protected override byte[] GetBytes(ByteBlock block)
        {
            return ByteBlock.Extractdata(block);
        }
    }
}
