using System.Threading;

namespace gpack.Queues
{
    internal class QueueContainer
    {
        private class QueueEntity
        {
            public int order;
            public byte[] bytes;
            public QueueEntity next;
        }

        private int length;
        private QueueEntity head;
        private QueueEntity tail;

        public bool TryAdd(int order, byte[] bytes)
        {
            if (length < Constants.CHAINE_SIZE)
            {
                if (head is null)
                {
                    head = tail = new QueueEntity { order = order, bytes = bytes };
                }
                else
                {
                    tail = tail.next = new QueueEntity { order = order, bytes = bytes };
                }

                Interlocked.Increment(ref length);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryGet(out int order, out byte[] bytes)
        {
            if (head is null)
            {
                bytes = default;
                order = default;
                return false;
            }
            else
            {
                bytes = head.bytes;
                order = head.order;
                head = head.next;
                Interlocked.Decrement(ref length);
                return true;
            }
        }

        public bool Get(int order, out byte[] bytes)
        {
            var local = head;
            while (local != null && local.order != order)
            {
                local = local.next;
            }

            if (local is null)
            {
                bytes = default;
                return false;
            }
            else
            {
                bytes = head.bytes;
                head = head.next;
                length--;
                return true;
            }
        }

        public void AddToHead(int order, byte[] bytes)
        {
            var entity = new QueueEntity { order = order, bytes = bytes };

            if (head is null)
            {
                head = tail = entity;
            }
            else
            {
                entity.next = head;
                head = entity;
                length++;
            }
        }
    }
}
