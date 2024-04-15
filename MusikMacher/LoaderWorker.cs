using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusikMacher
{
  internal abstract class LoaderWorker<I, O>
  {
    private BlockingCollection<Tuple<I, Action<O>>> bc;
    private Task t;
    private bool _empty;

    public LoaderWorker(bool stack, bool empty)
    {
      _empty = empty; // empty queue before handeling

      if (stack)
      {
        bc = new BlockingCollection<Tuple<I, Action<O>>>(new ConcurrentStack<Tuple<I, Action<O>>>());
      } else
      {
        bc = new BlockingCollection<Tuple<I, Action<O>>>();
      }
      t = Task.Run(WorkerMain);
    }

    internal void Shedule(Tuple<I, Action<O>> element)
    {
      bc.Add(element);
    }

    private void WorkerMain()
    {
      while (true)
      {
        var task = bc.Take();
        // empty till last element
        Tuple <I, Action<O>>? next = null;
        if (_empty)
        {
          while (bc.TryTake(out next))
          {
            task = next;
          }
        }
        // TODO: check if there is another LOL
        //System.Diagnostics.Debug.WriteLine($"try to load waveform of {task.Item1}");

        try
        {
          var points = Handle(task.Item1);
          Application.Current.Dispatcher.Invoke(() =>
          {
            task.Item2.Invoke(points);
          });
        }
        catch(Exception e)
        {
          System.Diagnostics.Debug.WriteLine($"Exception in worker thread {e}");
        }
      }
    }

    internal abstract O Handle(I item);
  }
}
