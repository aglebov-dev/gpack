using System;
using System.Threading;

namespace GPack
{
    public abstract class BaseType
    {
        protected readonly CancellationToken _externalToken;
        protected readonly Action<Exception> _exceptionCallback;
        protected readonly CancellationTokenSource _innerCancellationTokenSource;

        public CancellationToken CancellationToken { get; }

        public BaseType(CancellationToken cancellationToken, Action<Exception> progressCallback)
        {
            _externalToken = cancellationToken;
            _exceptionCallback = progressCallback;
            _innerCancellationTokenSource = new CancellationTokenSource();

            CancellationToken = _innerCancellationTokenSource.Token;
        }
    }
}
