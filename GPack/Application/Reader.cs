using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class Reader : BaseType
    {
        private class State
        {
            public BlockingQueue Queue;
            public Stream Stream;
        }

        private readonly Thread _thread;

        public Reader(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback)
            : base(cancellationToken, progressCallback)
        {
            _thread = new Thread(ThreadHandler);
        }

        private void ThreadHandler(object state)
        {
            try
            {
                var data = (State)state;
                BeginInternal(data.Stream, data.Queue);
            }
            catch (Exception ex)
            {
                _progressCallback?.Invoke(default, ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        public BlockingQueue BeginReadAsync(Stream stream)
        {
            var state = new State { Stream = stream, Queue = new BlockingQueue() };
            _thread.Start(state);

            return state.Queue;
        }

        private void BeginInternal(Stream stream, BlockingQueue writeQueue)
        {
            var buffer = new byte[Constants.FILE_READ_BUFFER_SIZE];
            var index = 0;
            int length;
            while (!_externalToken.IsCancellationRequested)
            {
                if ((length = stream.Read(buffer, 0, buffer.Length)) == 0)
                {
                    break;
                }
                else
                {
                    var block = ByteBlock.Create(index, buffer, length);
                    while (!_externalToken.IsCancellationRequested && !writeQueue.TryEnqueue(block))
                    {
                        Thread.Yield();
                    }

                    _progressCallback?.Invoke(new ProgressInfo(block.BlockLength), null);

                    index++;
                }
            }

            writeQueue.Complete();
        }
    }
}
