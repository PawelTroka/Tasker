using System.ComponentModel;

namespace Tasker
{
    public class Detail : INotifyPropertyChanged
    {
        private string m_Name;
        private object m_Value;

        public Detail(string name, object value)
        {
            m_Name = name;
            m_Value = value;
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public object Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                RaisePropertyChanged("Value");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}