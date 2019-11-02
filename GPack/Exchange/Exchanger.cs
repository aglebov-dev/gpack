using gpack.Common;
using gpack.Queues;
using System.Linq;
using System.Threading;

namespace gpack.Exchange
{
    internal class Exchanger: BaseType
    {
        private class SharedState
        {
            public int Counter;
            public bool IsPack;
            public WriteQueue Source;
            public ReadQueue Target;

            public int IncrementCounter() => Interlocked.Increment(ref Counter);
            public int DecrementCounter() => Interlocked.Decrement(ref Counter);
        }

        private readonly Packager _packager;

        public Exchanger(CancellationToken cancellationToken, Progress progress) 
            : base(cancellationToken, progress)
        {
            _packager = new Packager();
        }

        public void BeginCompressExchangeAsync(WriteQueue source, ReadQueue target)
        {
            var sharedState = new SharedState
            {
                Source = source,
                Target = target,
                IsPack = true
            };

            BeginWithState(sharedState);
        }

        public void BeginDecompressExchangeAsync(WriteQueue source, ReadQueue target)
        {
            var sharedState = new SharedState
            {
                Source = source,
                Target = target,
                IsPack = false
            };

            BeginWithState(sharedState);
        }

        private void BeginWithState(SharedState sharedState)
        {
            var threads = Enumerable.Range(0, Constants.THREADS_COUNT)
               .Select(x => new Thread(Exchange) { Name = $"Exchange {x} {(sharedState.IsPack ? "pack" : "unpack")}" })
               .ToArray();

            foreach (var thread in threads)
            {
                thread.Start(sharedState);
            }
        }

        private void Exchange(object state)
        {
            try
            {
                var sharedState = (SharedState)state;
                ExchangeInternal(sharedState);
            }
            catch (System.Exception ex)
            {
                _progress?.Error(ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        private void ExchangeInternal(SharedState sharedState)
        {
            var source = sharedState.Source;
            var target = sharedState.Target;
            var offset = sharedState.IncrementCounter();

            while (!_externalToken.IsCancellationRequested)
            {
                var (order, bytes) = source.Get(offset);
                if (bytes != null)
                {
                    var data = sharedState.IsPack ? _packager.Pack(bytes) : _packager.UnPack(bytes);
                    target.Add(order, data);
                }
                else if (source.IsComplete && source.IsEmpty)
                {
                    break;
                }
            }

            var currentCount = sharedState.DecrementCounter();
            if (currentCount == 0)
            {
                target.Complete();
            }
        }
    }
}
