using System.Linq;
using System.Threading;

namespace gpack.Queues
{
    internal class WriteQueue
    {
        private int length;
        private int mask;
        private QueueContainer[] containers;
        private int readOffest;
        private int count;

        public bool IsComplete { get; private set; }
        public bool IsEmpty => count == 0;

        public WriteQueue()
        {
            mask = int.MaxValue;
            length = Constants.THREADS_COUNT;
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
                    if (order <= readOffest)
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

        public (int order, byte[] bytes) Get(int offset)
        {
            var index = offset & mask % length;
            var container = GetContainer(index);
            lock (container)
            {
                while (!IsComplete || !IsEmpty)
                {
                    if (container.TryGet(out var order, out var bytes))
                    {
                        Interlocked.Decrement(ref count);
                        Interlocked.Increment(ref readOffest);
                        Monitor.Pulse(container);
                        return (order, bytes);
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

            return (default, default);
        }


        internal void Complete()
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

        private QueueContainer GetContainer(int index) => containers[index] ?? (containers[index] = new QueueContainer());
    }
}
