using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#region interface IAsyncSemaphoreSlim
public interface IAsyncSemaphoreSlim
{
    Task WaitAsync();
    void Release();
}
#endregion

#region class AsyncSemaphoreSlim_Baseline
public class AsyncSemaphoreSlim_Baseline : IAsyncSemaphoreSlim
{
    private SemaphoreSlim m_sema;

    public AsyncSemaphoreSlim_Baseline(int initialCount, int maxCount)
    {
        m_sema = new SemaphoreSlim(initialCount, maxCount);
    }

    public Task WaitAsync() => m_sema.WaitAsync();

    public void Release() => m_sema.Release();
}
#endregion

#region class AsyncSemaphoreSlim
public class AsyncSemaphoreSlim : IAsyncSemaphoreSlim
{
    #region Private data
    private int m_count;
    private readonly int m_maxCount;

    private readonly Stack<TaskCompletionSource<bool>> m_waiting;
    #endregion

    #region Construction
    public AsyncSemaphoreSlim(int initialCount, int maxCount)
    {
        m_count = initialCount;
        m_maxCount = maxCount;
        m_waiting = new Stack<TaskCompletionSource<bool>>();
    }
    #endregion

    #region Public API
    public Task WaitAsync()
    {
        lock (m_waiting)
        {
            if (m_count > 0)
            {
                m_count--;
                return Task.CompletedTask;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                m_waiting.Push(tcs);
                return tcs.Task;
            }
        }
    }

    public void Release()
    {
        TaskCompletionSource<bool> tcs = null;
        lock (m_waiting)
        {
            m_count++;
            if (m_waiting.Count > 0)
            {
                tcs = m_waiting.Pop();
            }
            else
            {
                return;
            }
        }
        tcs.SetResult(true);
    }
    #endregion
}
#endregion
