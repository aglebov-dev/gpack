using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class PackWriter: BaseType
    {
        public PackWriter(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback)
            : base(cancellationToken, progressCallback)
        {
        }

        public void Write(Stream stream, BlockingQueue writeQueue)
        {
            try
            {
                while (!_externalToken.IsCancellationRequested)
                {
                    if (writeQueue.TryDequeue(out var value))
                    {
                        var bytes = value.Bytes;
                        stream.Write(bytes, 0, bytes.Length);

                        _progressCallback?.Invoke(new ProgressInfo(bytes.Length), default);

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
                _progressCallback?.Invoke(default, ex);
                _innerCancellationTokenSource.Cancel();
            }
        }
    }
}
