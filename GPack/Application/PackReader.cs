using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public class PackReader : BaseType
    {
        private class State
        {
            public BlockingQueue Queue;
            public Stream Stream;
        }

        private readonly Thread _thread;

        public PackReader(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback) 
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
            var metaBuffer = new byte[8];
            var index = 0;
            while (!_externalToken.IsCancellationRequested)
            {
                if (stream.Read(metaBuffer, 0, metaBuffer.Length) == 0)
                {
                    break;
                }
                else
                {
                    var blockLength = BitConverter.ToInt32(metaBuffer, 0);
                    var dataIndex = BitConverter.ToInt32(metaBuffer, 4);
                    var dataLength = blockLength - 8;
                    var bytes = new byte[dataLength];

                    stream.Read(bytes, 0, dataLength);

                    var block = ByteBlock.Create(dataIndex, bytes, dataLength);

                    while (!_externalToken.IsCancellationRequested && writeQueue.TryEnqueue(block) == false)
                    {
                        Thread.Yield();
                    }

                    _progressCallback?.Invoke(new ProgressInfo(blockLength), null);
                    index++;
                }
            }

            writeQueue.Complete();
        }
    }
}
