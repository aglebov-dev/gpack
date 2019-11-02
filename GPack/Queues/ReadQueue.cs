using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace gpack.Queues
{
    internal class ReadQueue
    {
        private int length = 32;
        private int mask = int.MaxValue;
        private int readOffset;
        private int count;

        private QueueContainer[] containers;
        public bool IsComplete { get; private set; }
        public bool IsEmpty => count == 0;

        public ReadQueue()
        {
            containers = new QueueContainer[length];
        }

        public void Add(int order, byte[] bytes)
        {
            var index = order & mask % length;
            var container = GetContainer(index);
            lock (container)
            {
                while (true)
                {
                    if (order <= readOffset)
                    {
                        container.AddToHead(order, bytes);
                        Interlocked.Increment(ref count);
                        break;
                    }
                    else if (container.TryAdd(order, bytes))
                    {
                        Interlocked.Increment(ref count);
                        break;
                    }
                    else
                    {
                        Monitor.Wait(container);
                    }
                }
                Monitor.Pulse(container);
            }
        }

        private byte[] GetNext()
        {
            var order = Interlocked.Exchange(ref readOffset, readOffset + 1);
            while (!(IsComplete && IsEmpty))
            {
                var index = order & mask % length;
                var container = GetContainer(index);
                lock (container)
                {
                    while (true)
                    {
                        if (container.Get(order, out var bytes))
                        {
                            Interlocked.Decrement(ref count);
                            Monitor.PulseAll(container);
                            return bytes;
                        }
                        else if (IsComplete)
                        {
                            break;
                        }
                        else
                        {
                            Monitor.Wait(container);
                        }
                    }
                }
            }

            return default;
        }

        private QueueContainer GetContainer(int index) 
            => containers[index] ?? (containers[index] = new QueueContainer());

        public IEnumerator<byte[]> GetEnumerator() 
            => new QueueEnumerator(this);

        public void Complete()
        {
            IsComplete = true;
            foreach (var item in containers.Where(x => x != null))
            {
                lock (item)
                {
                    Monitor.PulseAll(item);
                }
            }
        }

        private class QueueEnumerator : IEnumerator<byte[]>
        {
            private readonly ReadQueue _queue;

            public QueueEnumerator(ReadQueue queue)
            {
                _queue = queue;
            }

            public bool MoveNext()
            {
                return !(_queue.IsComplete && _queue.IsEmpty);
            }

            public void Reset() { }
            public byte[] Current => _queue.GetNext();
            public void Dispose() { }
            object IEnumerator.Current => Current;
        }
    }
}
