using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;

namespace HIO.Backend
{
    public class QueueManager
    {
        Queue functionsQueue;

        public bool IsEmpty
        {
            get
            {
                if (functionsQueue.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        public QueueManager()
        {
            functionsQueue = new Queue();
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += Dt_TickAsync;
            dt.Start();
        }

        private async void Dt_TickAsync(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            while (!IsEmpty)
            {
                await Task.Run(() =>
                {
                    //   fq();
                    var task = Task.Run((Action)Pop());
                    task.Wait();
                });
            }
             (sender as DispatcherTimer).Start();
        }

        public bool Contains(Action action)
        {
            if (functionsQueue.Contains(action))
                return true;
            else
                return false;
        }

        public Action Pop()
        {
            return functionsQueue.Dequeue() as Action;
        }

        public void Add(Action function)
        {
            functionsQueue.Enqueue(function);
        }
       

    }

    public class CacheObject
    {
    }
}
