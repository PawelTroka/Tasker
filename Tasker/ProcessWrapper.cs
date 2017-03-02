using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tasker
{
    public class ProcessWrapper : INotifyPropertyChanged
    {
        //public int Id { get; private set; }

        private Task KeepAliveTask;
        private CancellationToken ct;
        private ImageSource icon;
        // private string fileName;
        private bool m_KeepAlive;
        private long m_MemoryUsage;
        private Process m_Process;
        private double m_ProcessorUsage;
        private ProcessPriorityClass m_backupPriority;
        private ProcessStartInfo m_backupStartInfo;
        private CancellationTokenSource ts;

        public ProcessWrapper(Process process)
        {
            m_Process = process;
            if (process != null)
            {
                // Id = process.Id;
                MemoryUsage = process.WorkingSet64;

                m_KeepAlive = false;
                try
                {
                    m_Process.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                }
                //InitNewKeepAliveTask();
            }
            //else
            // Id = -1;
        }

        public int Id
        {
            get { return Process.Id; }
        }

        public long MemoryUsage
        {
            get { return m_MemoryUsage; }
            set
            {
                m_MemoryUsage = value;
                RaisePropertyChanged("MemoryUsage");
            }
        }

        public double ProcessorUsage
        {
            get { return m_ProcessorUsage; }
            set
            {
                m_ProcessorUsage = value;
                RaisePropertyChanged("ProcessorUsage");
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (icon == null && File.Exists(FileName))
                {
                    using (Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(FileName))
                    {
                        icon = Imaging.CreateBitmapSourceFromHIcon(
                            sysicon.Handle,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());
                    }
                }

                return icon;
            }
        }

        public Process Process
        {
            get { return m_Process; }
            set
            {
                if (m_Process != value)
                {
                    m_Process = value;
                    RaisePropertyChanged("Process");
                }
            }
        }

        public string FileName
        {
            get
            {
                string str;
                try
                {
                    str = Process.MainModule.FileName;
                }
                catch (Exception ex)
                {
                    return "";
                }
                return str;
            }
        }

        public bool KeepAlive
        {
            get { return m_KeepAlive; }
            set
            {
                if (value != m_KeepAlive && value)
                {
                    m_KeepAlive = value;
                    m_backupStartInfo = (ProcessStartInfo) (deepCopyOfObject(Process.StartInfo));


                    string wmiQuery = string.Format("select CommandLine from Win32_Process where ProcessId='{0}'",
                        Process.Id);
                    var searcher = new ManagementObjectSearcher(wmiQuery);
                    ManagementObjectCollection retObjectCollection = searcher.Get();

                    var sb = new StringBuilder();
                    foreach (ManagementObject retObject in retObjectCollection)
                    {
                        sb.AppendFormat("{0}", retObject["CommandLine"]);
                        // sb.AppendFormat("{0}", retObject["CommandLine"]);
                    }

                    string commandLine = sb.ToString();
                    string args = null, file = null;

                    try
                    {
                        args = commandLine.Substring(commandLine.IndexOf(@"""", 1) + 1);
                    }
                    catch (Exception ex)
                    {
                    }

                    try
                    {
                        file = commandLine.Substring(commandLine.IndexOf(@"""") + 1,
                            commandLine.IndexOf(@"""", 1) - 1);
                    }
                    catch (Exception ex)
                    {
                    }


                    m_backupStartInfo.FileName = !string.IsNullOrEmpty(file) ? file : Process.MainModule.FileName;
                    //  m_backupStartInfo.FileName = Process.MainModule.FileName;//!string.IsNullOrEmpty(file) ? file : Process.MainModule.FileName;
                    m_backupStartInfo.Arguments = !string.IsNullOrEmpty(args) ? args : m_backupStartInfo.Arguments;
                    m_backupPriority = Process.PriorityClass;
                    //  InitNewKeepAliveTask();
                    Process.Exited += _keepProcessAlive; /*WaitForExit();*/

                    //   KeepAliveTask.Start();
                }
                else if (value != m_KeepAlive && !value)
                {
                    m_KeepAlive = value;
                    //    ts.Cancel();
                    Process.Exited -= _keepProcessAlive;
                }
                RaisePropertyChanged("KeepAlive");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitNewKeepAliveTask()
        {
            ts = new CancellationTokenSource();
            ct = ts.Token;
            KeepAliveTask = new Task(_keepProcessAlive);
        }

        private Object deepCopyOfObject(Object obj)
        {
            Object ret = Activator.CreateInstance(obj.GetType());

            foreach (PropertyInfo prop in obj.GetType().GetProperties())
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    prop.SetValue(ret, prop.GetValue(obj));
                }
            }
            return ret;
        }

        public void Kill()
        {
            //if (!KeepAlive)
            Process.Kill();
        }


        private void _keepProcessAlive()
        {
            while (KeepAlive && !Process.HasExited)
            {
                if (ct.IsCancellationRequested)
                    break;
                Process.WaitForExit();
            }
        }

        private void _keepProcessAlive(object sender, EventArgs e)
        {
            var p = sender as Process;
            while (!p.HasExited)
                p.WaitForExit();

            p.StartInfo = m_backupStartInfo;
            //p.StartInfo.UseShellExecute = true;
            // p=Process.Start(fileName, m_backupStartInfo.Arguments);
            //p=Process.Start(m_backupStartInfo);

            p.Start();
            p.PriorityClass = m_backupPriority;
            //p=Process.Start(m_backupStartInfo);
            // if(KeepAlive)
            // Task.Run(() => _keepProcessAlive(p));
        }

        public void IncreasePriority()
        {
            switch (Process.PriorityClass)
            {
                case ProcessPriorityClass.Idle:
                    Process.PriorityClass = ProcessPriorityClass.BelowNormal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.BelowNormal:
                    Process.PriorityClass = ProcessPriorityClass.Normal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.Normal:
                    Process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.AboveNormal:
                    Process.PriorityClass = ProcessPriorityClass.High;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.High:
                    Process.PriorityClass = ProcessPriorityClass.RealTime;
                    RaisePropertyChanged("Process");
                    break;

                    // case ProcessPriorityClass.RealTime:
                    // break;
            }
            m_backupPriority = Process.PriorityClass;
        }

        public void DecreasePriority()
        {
            switch (Process.PriorityClass)
            {
                    // case ProcessPriorityClass.Idle:
                    //   Process.PriorityClass =ProcessPriorityClass.BelowNormal;
                    //  break;

                case ProcessPriorityClass.BelowNormal:
                    Process.PriorityClass = ProcessPriorityClass.Idle;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.Normal:
                    Process.PriorityClass = ProcessPriorityClass.BelowNormal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.AboveNormal:
                    Process.PriorityClass = ProcessPriorityClass.Normal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.High:
                    Process.PriorityClass = ProcessPriorityClass.AboveNormal;
                    RaisePropertyChanged("Process");
                    break;

                case ProcessPriorityClass.RealTime:
                    Process.PriorityClass = ProcessPriorityClass.High;
                    RaisePropertyChanged("Process");
                    break;
            }
            m_backupPriority = Process.PriorityClass;
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