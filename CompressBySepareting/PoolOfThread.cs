using System.Collections.Generic;
using System.Threading;

namespace CompressBySepareting
{
    public class PoolOfThread
    {
        private List<Thread> _threads;

        private bool _resetThreads;

        public static int MaxThreads { get; set; } = 20;

        public PoolOfThread()
        {
            _threads = new List<Thread>();
        }

        public void Wait()
        {
            foreach (var thread in _threads)
            {
                thread.Join();
            }
        }

        public void Add(Thread thread)
        {
            if (_resetThreads)
            {
                _threads = new List<Thread>();
                _resetThreads = false;
            }

            _threads.Add(thread);

            StartThreads(_threads, thread);
        }

        private static void StartThreads(List<Thread> threads, Thread newThread)
        {
            if (threads.Count < MaxThreads)
            {
                newThread.Start();
            }
            else
            {
                for (int i = 0; i < threads.Count; i++)
                {
                    if (threads[i].ThreadState == ThreadState.Stopped ||
                        (threads[i].ThreadState == ThreadState.Running && threads[i].Join(100)))
                    {
                        threads.Remove(threads[i]);
                    }
                }

                if (threads.Count < MaxThreads)
                {
                    newThread.Start();
                }
            }
        }
    }
}
