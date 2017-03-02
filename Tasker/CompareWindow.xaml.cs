using System.Collections;
using System.Windows;

namespace Tasker
{
    /// <summary>
    ///     Interaction logic for CompareWindow.xaml
    /// </summary>
    public partial class CompareWindow : Window
    {
        public CompareWindow(IList processes)
        {
            this.processes = processes;
            InitializeComponent();
        }

        public IList processes { get; set; }
    }
}