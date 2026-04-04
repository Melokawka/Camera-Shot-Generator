using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BackgroundFrameWriter
{
    private ConcurrentQueue<(string path, byte[] data)> writeQueue = new();
    private CancellationTokenSource cts = new();
    private Thread worker;

    public BackgroundFrameWriter()
    {
        worker = new Thread(ProcessQueue);
        worker.Start();
    }

    public void Enqueue(string path, byte[] data)
    {
        writeQueue.Enqueue((path, data));
    }

    private void ProcessQueue()
    {
        while (!cts.Token.IsCancellationRequested)
        {
            if (writeQueue.TryDequeue(out var item))
            {
                Task.Run(() =>
                {
                    //Debug.Log($"Zapisuję plik na wątku: {Thread.CurrentThread.ManagedThreadId}");
                    File.WriteAllBytes(item.path, item.data);
                    //Debug.Log($"Zapisuję plik {item.path}");
                });
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    public void Stop()
    {
        cts.Cancel();
        worker.Join();
    }

    public void Flush()
    {
        while (!writeQueue.IsEmpty)
        {
            Thread.Sleep(10);
        }
    }
}