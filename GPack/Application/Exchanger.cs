using System;
using System.Linq;
using System.Threading;

namespace GPack
{
    public class Exchanger: BaseType
    {
        private class SharedState
        {
            public int Index;
            public int Counter;
            public BlockingQueue Source;
            public BlockingQueue Target;
            public bool IsPack;

            public int IncrementIndex()
            {
                return Interlocked.Increment(ref Index);
            }
            public int IncrementCounter()
            {
                return Interlocked.Increment(ref Counter);
            }
            public int DecrementCounter()
            {
                return Interlocked.Decrement(ref Counter);
            }
        }

        private readonly Packager _packager;

        public Exchanger(CancellationToken cancellationToken, Action<ProgressInfo, Exception> progressCallback) 
            : base(cancellationToken, progressCallback)
        {
            _packager = new Packager();
        }

        public void BeginCompressExchangeAsync(BlockingQueue source, BlockingQueue target)
        {
            var threads = Enumerable.Range(0, Constants.THREADS_COUNT)
                .Select(x => new Thread(Exchange))
                .ToArray();

            var sharedState = new SharedState
            {
                Source = source,
                Target = target,
                IsPack = true
            };

            foreach (var thread in threads)
            {
                thread.Start(sharedState);
            }
        }

        public void BeginDecompressExchangeAsync(BlockingQueue source, BlockingQueue target)
        {
            var threads = Enumerable.Range(0, Constants.THREADS_COUNT)
                .Select(x => new Thread(Exchange))
                .ToArray();

            var sharedState = new SharedState
            {
                Source = source,
                Target = target,
                IsPack = false
            };
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
            catch (Exception ex)
            {
                _progressCallback?.Invoke(default, ex);
                _innerCancellationTokenSource.Cancel();
            }
        }

        private void ExchangeInternal(SharedState sharedState)
        {
            
            var source = sharedState.Source;
            var target = sharedState.Target;

            sharedState.IncrementCounter();

            while (!_externalToken.IsCancellationRequested && !source.IsEmpty)
            {
                if (source.TryDequeue(out var value))
                {
                    var data = sharedState.IsPack ? _packager.Pack(value) : _packager.UnPack(value);

                    while (!_externalToken.IsCancellationRequested && sharedState.Index != data.Index)
                    {
                        Thread.Yield();
                    }

                    while (!_externalToken.IsCancellationRequested && !target.TryEnqueue(data))
                    {
                        Thread.Yield();
                    }

                    sharedState.IncrementIndex();
                }
                else
                {
                    Thread.Yield();
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
