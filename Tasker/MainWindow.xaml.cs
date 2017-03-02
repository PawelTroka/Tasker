using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace Tasker
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    /*
Manager zadan
Wymagania: 
     * aplikacja powinna byc zrealizowana w WPF. /DONE
     * Dostepna powinna byc lista procesow w systemie. /DONE
     * Dla wybranego procesu powinny byc prezentowane dodatkowe szczegoly (zawartosc klasy process) /DONE
     * w szczegolnosci lista watkow, modulow itd. /DONE
     * Dla wybranego procesu powinna istniec mozliwosc wymuszenia zakonczenia, zmiany priorytetu. /DONE
     
     * Ponadto aplikacja powinna dla wybranych procesow realizowac podtrzywanie przy zyciu, /DONE
       tj. po wykryciu zakonczenia takiego procesu aplikacja powinna wystartowac proces ponownie. /DONE 
     
     * Dodatkowe utrudnienie (punktowane osobno), tj. nie jest to obowiazkowe /DONE
       - aplikacja moglaby dbac o to by parametry wywolania wznawianego procesu byly zgodne z pierwowzorem. /DONE
     
Termin oddawania: zajecia projektowe w tygodniu 19.11-21.11.
     */
    public partial class MainWindow : Window
    {
        private int m_SelectedProcessId;

        public MainWindow()
        {
            SelectedProcessWrapper = new ProcessWrapper(null);
            ProcessManager = new ProcessManager();
            ProcessDetails = new ObservableCollectionWithItemNotify<Detail>();
            InitializeComponent();
        }

        public ProcessManager ProcessManager { get; set; }
        public ProcessWrapper SelectedProcessWrapper { get; set; }
        public ObservableCollectionWithItemNotify<Detail> ProcessDetails { get; set; }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Paweł Troka, 132334, Programowanie Lokalnych Aplikacji .NET");
        }

        private void ProcessListBox_OnSelected(object sender, RoutedEventArgs e)
        {
            _refreshDetails();
        }

        private void _refreshDetails()
        {
            ProcessDetails.Clear();
            if (processesListBox.SelectedItem != null)
            {
                ProcessWrapper p = ((KeyValuePair<int, ProcessWrapper>) processesListBox.SelectedItem).Value;
                if (p != null)
                {
                    m_SelectedProcessId = p.Process.Id;
                    SelectedProcessWrapper.Process = p.Process;
                    //SelectedProcessWrapper.KeepAlive=
                }

                foreach (PropertyInfo property in typeof (Process).GetProperties())
                {
                    if (property.CanRead)
                    {
                        Detail d = null;
                        try
                        {
                            d = new Detail(property.Name, property.GetValue(Process.GetProcessById(m_SelectedProcessId)));
                            ProcessDetails.Add(d);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        private void EditItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (processesListBox.SelectedItem != null)
            {
                var editWindow =
                    new EditSelectedItems(
                        ((KeyValuePair<int, ProcessWrapper>) processesListBox.SelectedItem).Value.Process);
                bool? result = editWindow.ShowDialog();
                //if(result==true)
                //  processesListBox.
            }
        }

        private void CompareItemsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (processesListBox.SelectedItems != null)
            {
                IList processes = new ArrayList();
                for (int i = 0; i < processesListBox.SelectedItems.Count; i++)
                    processes.Add(((KeyValuePair<int, ProcessWrapper>) processesListBox.SelectedItems[i]).Value.Process);

                var compareWindow = new CompareWindow(processes);
                compareWindow.ShowDialog();
            }
        }

        private void AddMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog().Value)
            {
                if (openDialog.CheckFileExists)
                    Process.Start(openDialog.FileName);
            }
        }

        private void smartphonesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*   var item =
                ItemsControl.ContainerFromElement(processesListBox, e.OriginalSource as DependencyObject) as
                    ListBoxItem;
            if (item != null)
            {
                var imageWindow = new ImageWindow((item.Content as Smartphone).PictureLocation);
                imageWindow.Show();
            }*/

            //var list = new List<object>();
            //  foreach(var its in processDetailsListBox.ItemsSource)
            //   list.Add(its);
            // MessageBox.Show( (list[1] as CollectionContainer).ToString());
        }

        private void KillButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var item =
                ItemsControl.ContainerFromElement(processesListBox, e.OriginalSource as DependencyObject) as
                    ListBoxItem;

            if (item != null)
            {
                // MessageBox.Show((item.Content as ProcessWrapper).FileName);
                ProcessWrapper p = ((KeyValuePair<int, ProcessWrapper>) item.Content).Value;
                p.Kill();
                //_refreshDetails();/////////////////////////////////////////////////////////////////
            }
        }

        private void IncreasePriorityButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var item =
                ItemsControl.ContainerFromElement(processesListBox, e.OriginalSource as DependencyObject) as
                    ListBoxItem;

            if (item != null)
            {
                // MessageBox.Show((item.Content as ProcessWrapper).FileName);
                ProcessWrapper p = ((KeyValuePair<int, ProcessWrapper>) item.Content).Value;
                p.IncreasePriority();
                _refreshDetails();
            }
        }

        private void DecreasePriorityButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            var item =
                ItemsControl.ContainerFromElement(processesListBox, e.OriginalSource as DependencyObject) as
                    ListBoxItem;

            if (item != null)
            {
                // MessageBox.Show((item.Content as ProcessWrapper).FileName);
                ProcessWrapper p = ((KeyValuePair<int, ProcessWrapper>) item.Content).Value;
                p.DecreasePriority();
                _refreshDetails();
            }
        }
    }
}