using System;
using System.Diagnostics;
using System.Threading;

public class AsyncSemaphoreTests
{
    public void TestAsyncSemaphoreStress()
    {
        var numWorkers = 30;
        var numRounds = 10_000_000;
        Stress(new AsyncSemaphoreSlim_Baseline(10, 10), numWorkers, numRounds);
        Stress(new AsyncSemaphoreSlim(10, 10), numWorkers, numRounds);
    }

    private void Stress(IAsyncSemaphoreSlim sema, int numWorkers, int numRounds)
    {
        Console.WriteLine("Stress test for {0}", sema.GetType().FullName);
        var sw = Stopwatch.StartNew();
        Worker.s_rounds = numRounds;
        Worker.s_totalCount = Worker.s_rounds * numWorkers;
        Worker.s_done = new ManualResetEvent(false);

        var semaphore = new AsyncSemaphoreSlim(10, 10);
        for (int i = 0; i < numWorkers; i++)
        {
            Worker.Run(semaphore, i);
        }
        Worker.s_done.WaitOne();
        Console.WriteLine("...Done in {0} msec", sw.ElapsedMilliseconds);
    }

    private class Worker
    {
        private string m_name;
        private AsyncSemaphoreSlim m_semaphore;

        public static int s_rounds;
        public static long s_totalCount;
        public static ManualResetEvent s_done;

        public string Name => m_name;

        public static Worker Run(AsyncSemaphoreSlim semaphore, int t)
        {
            Worker ret = new Worker();
            ret.m_name = "Worker:" + t.ToString();
            ret.m_semaphore = semaphore;
            var thread = new Thread(ret.DoWork);
            thread.IsBackground = true;
            thread.Start();
            return ret;
        }

        private void DoWork()
        {
            for (int i = 0; i < s_rounds; i++)
            {
                var task = m_semaphore.WaitAsync();
                task.Wait();
                Interlocked.Decrement(ref s_totalCount);
                m_semaphore.Release();
            }
            if (s_totalCount == 0)
            {
                s_done.Set();
            }
        }
    }
}
