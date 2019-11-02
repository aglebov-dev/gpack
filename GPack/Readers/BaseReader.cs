using gpack.Common;
using gpack.Queues;
using System.IO;
using System.Threading;

namespace gpack.Readers
{
    internal abstract class BaseReader : BaseType
    {
        private class State
        {
            public WriteQueue Queue;
            public Stream Stream;
        }

        private readonly Thread _thread;

        public BaseReader(CancellationToken cancellationToken, Progress progress)
            : base(cancellationToken, progress)
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
            catch (System.Exception ex)
            {
                data.Queue.Complete();
                _progress?.Error(ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        public WriteQueue BeginReadAsync(Stream stream)
        {
            var state = new State { Stream = stream, Queue = new WriteQueue() };
            _thread.Start(state);

            return state.Queue;
        }

        protected abstract void BeginInternal(Stream stream, WriteQueue writeQueue);
    }
}
