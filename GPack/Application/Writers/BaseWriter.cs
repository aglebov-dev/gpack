using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public abstract class BaseWriter : BaseType
    {
        public BaseWriter(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback) { }

        public void Write(Stream stream, BlockingQueue writeQueue)
        {
            try
            {
                while (!_externalToken.IsCancellationRequested)
                {
                    if (writeQueue.TryDequeue(out var value))
                    {
                        var data = GetBytes(value);
                        stream.Write(data, 0, data.Length);
                    }
                    else if (writeQueue.IsDataReceptionCompleted && writeQueue.Count == 0)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Yield();
                    }
                }
            }
            catch (Exception ex)
            {
                _exceptionCallback?.Invoke(ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        protected abstract byte[] GetBytes(ByteBlock block);
    }
}
