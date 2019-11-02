using gpack.Common;
using System.Threading;

namespace gpack.Common
{
    internal abstract class BaseType
    {
        protected readonly CancellationToken _externalToken;
        protected readonly Progress _progress;
        protected readonly CancellationTokenSource _innerCancellationTokenSource;

        public CancellationToken CancellationToken { get; }

        public BaseType(CancellationToken cancellationToken, Progress progress)
        {
            _externalToken = cancellationToken;
            _progress = progress;
            _innerCancellationTokenSource = new CancellationTokenSource();

            CancellationToken = _innerCancellationTokenSource.Token;
        }
    }
}
