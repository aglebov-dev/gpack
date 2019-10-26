using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class Writer : BaseType
    {
        public Writer(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback)
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
                        var data = ByteBlock.Extractdata(value);
                        stream.Write(data, 0, data.Length);
                        _progressCallback?.Invoke(new ProgressInfo(data.Length), default);
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
