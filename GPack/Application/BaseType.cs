using System;
using System.Threading;

namespace GPack
{
    public abstract class BaseType
    {
        protected readonly CancellationToken _externalToken;
        protected readonly Action<ProgressInfo, Exception> _progressCallback;
        protected readonly CancellationTokenSource _innerCancellationTokenSource;

        public CancellationToken CancellationToken { get; }

        public BaseType(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback)
        {
            _externalToken = cancellationToken;
            _progressCallback = progressCallback;
            _innerCancellationTokenSource = new CancellationTokenSource();

            CancellationToken = _innerCancellationTokenSource.Token;
        }
    }
}
