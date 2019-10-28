using System;
using System.IO;
using System.Threading;

namespace GPack
{
    public abstract class BaseReader : BaseType
    {
        private class State
        {
            public BlockingQueue Queue;
            public Stream Stream;
        }

        private readonly Thread _thread;

        public BaseReader(CancellationToken cancellationToken, Action<Exception> progressCallback)
            : base(cancellationToken, progressCallback)
        {
            _thread = new Thread(ThreadHandler);
        }

        private void ThreadHandler(object state)
        {
            var data = (State)state;
            try
            {
                BeginInternal(data.Stream, data.Queue);
            }
            catch (Exception ex)
            {
                _exceptionCallback?.Invoke(ex);
                _innerCancellationTokenSource.Cancel();
            }
            finally
            {
                data.Queue.Complete();
            }
        }

        public BlockingQueue BeginReadAsync(Stream stream)
        {
            var state = new State { Stream = stream, Queue = new BlockingQueue() };
            _thread.Start(state);

            return state.Queue;
        }

        protected abstract void BeginInternal(Stream stream, BlockingQueue writeQueue);
    }
}
