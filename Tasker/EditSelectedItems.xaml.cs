using System;
using System.Reflection;
using System.Windows;

namespace Tasker
{
    /// <summary>
    ///     Interaction logic for EditSelectedItems.xaml
    /// </summary>
    public partial class EditSelectedItems : Window
    {
        private readonly Object _Object;

        public EditSelectedItems(Object _object)
        {
            _Object = _object;

            DataContext = this;
            InitializeComponent();

            PropertyGrid.CurrentObject = _Object; //new Object(_Object); //= this.ObjectCopy;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            PropertyInfo[] props = _Object.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                if (prop.CanRead && prop.CanWrite)
                    prop.SetValue(_Object, prop.GetValue(PropertyGrid.CurrentObject));
            }
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}