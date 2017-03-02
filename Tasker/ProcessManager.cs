using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using TopSmartphones;

namespace Tasker
{
    public class ProcessManager : INotifyPropertyChanged
    {
        private readonly Timer timer;
        private readonly int waitTime;
        // private ProcessWrapper[] m_processes;
        // private ProcessWrapper[] tmp_processes;

        private ObservableDictionary<int, ProcessWrapper> m_ProcessesDictionary;

        public ProcessManager()
        {
            Process[] procs = Process.GetProcesses();
            //  m_processes = new ProcessWrapper[procs.Length];
            m_ProcessesDictionary = new ObservableDictionary<int, ProcessWrapper>();

            for (int i = 0; i < procs.Length; i++)
            {
                var processWrapper = new ProcessWrapper(procs[i]);
                //      m_processes[i] = processWrapper;
                m_ProcessesDictionary.Add(procs[i].Id, processWrapper);
            }

            waitTime = 3000;

            timer = new Timer(o => refresh(), null, waitTime, Timeout.Infinite);

            //Task.Factory.StartNew(refreshProcessesInBackground, TaskCreationOptions.LongRunning);
        }

        public ObservableDictionary<int, ProcessWrapper> ProcessesDictionary
        {
            get { return m_ProcessesDictionary; }
            private set
            {
                m_ProcessesDictionary = value;
                RaisePropertyChanged("ProcessesDictionary");
            }
        }

        /* public ProcessWrapper[] Processes
         {
             get { return m_processes; }
             private set
             {
                 m_processes = value;
                 RaisePropertyChanged("Processes");
             }
         }*/

        public event PropertyChangedEventHandler PropertyChanged;


        private void refreshProcessesInBackground()
        {
            while (true)
            {
                Thread.Sleep(waitTime);
                refresh();
            }
        }

        private void refresh()
        {
            Process[] procs = Process.GetProcesses();

            var counters = new List<PerformanceCounter>();

            foreach (Process process in procs)
            {
                var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                try
                {
                    counter.NextValue();
                }
                catch (Exception ex)
                {
                }
                counters.Add(counter);
            }


            var toRemove = new List<KeyValuePair<int, ProcessWrapper>>();
            foreach (var processWrapper in m_ProcessesDictionary)
            {
                try
                {
                    if (processWrapper.Value.Process.HasExited && !processWrapper.Value.KeepAlive)
                        toRemove.Add(processWrapper);
                }
                catch (Exception ex)
                {
                }
            }
            foreach (var item in toRemove)
            {
                Application.Current.Dispatcher.Invoke(() => m_ProcessesDictionary.Remove(item));
                    //TODO: using dispatcher is not very efficient
            }

            //Thread.Sleep(1000);

            for (int i = 0; i < procs.Length; i++)
            {
                if (!m_ProcessesDictionary.ContainsKey(procs[i].Id))
                {
                    var record = m_ProcessesDictionary.Where(p => p.Value.Process.Id == procs[i].Id)
                        .Select(p => new {p.Key, p.Value})
                        .FirstOrDefault();

                    if (record != null)
                    {
                        Application.Current.Dispatcher.Invoke(
                            () => ProcessesDictionary.RenameKey(record.Key, record.Value.Process.Id));
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(
                            () => m_ProcessesDictionary.Add(procs[i].Id, new ProcessWrapper(procs[i])));
                            //TODO: using dispatcher is not very efficient
                    }
                }
                else
                {
//treat it here a little like ViewModel, tho we update only those properties that we use in ViewModel instead of updating reference
                    // App.Current.Dispatcher.Invoke(() => m_ProcessesDictionary[procs[i].Id].Process.wor = procs[i]);//TODO: using dispatcher is not very efficient
                    Application.Current.Dispatcher.Invoke(
                        () => m_ProcessesDictionary[procs[i].Id].MemoryUsage = procs[i].WorkingSet64);
                        //TODO: using dispatcher is not very efficient

                    float perf = 0.0f;
                    try
                    {
                        perf = counters[i].NextValue();
                    }
                    catch (Exception)
                    {
                    }

                    Application.Current.Dispatcher.Invoke(
                        () => m_ProcessesDictionary[procs[i].Id].ProcessorUsage = perf/(100*Environment.ProcessorCount));
                        //TODO: using dispatcher is not very efficient
                }
            }

            RaisePropertyChanged("ProcessesDictionary");

            timer.Change(waitTime, Timeout.Infinite); //enable again
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}