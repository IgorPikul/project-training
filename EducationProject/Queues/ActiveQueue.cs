﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EducationProject.Queues
{
    class ActiveQueue<T> : IDisposable where T : class
    {
        private readonly Queue<T> queue;
        private readonly object syncObject = new object();
        private readonly int capacity;

        public ActiveQueue(int capacity, int workersCount)
        {
            this.capacity = capacity;
            for (int i = 0; i < workersCount; i++)
                ThreadPool.QueueUserWorkItem(ThreadFn);

            queue = new Queue<T>(capacity);
        }
        ~ActiveQueue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
        }
        
        public event Action<T> ProcessMessage;
        private void FireProcessMessage(T message)
        {
            var handler = ProcessMessage;
            if (handler != null)
                handler(message);
        }

        public void Enqueue(T item)
        {
            lock (syncObject)
            {
                while (queue.Count == capacity)
                    Monitor.Wait(syncObject);

                if (queue.Count == 0)
                    Monitor.Pulse(syncObject);

                queue.Enqueue(item);
            }
        }

   
        private void ThreadFn(object obj)
        {
            while (true)
            {
                T item = default(T);
                lock (syncObject)
                {
                    while (queue.Count == 0)
                        Monitor.Wait(syncObject);

                    if (queue.Count == capacity)
                        Monitor.Pulse(syncObject);

                    item = queue.Dequeue();
                }

                if (item != null)
                    FireProcessMessage(item);
            }
        }
      
    }
}
