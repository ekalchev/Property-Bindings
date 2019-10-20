using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Utils.DataBindings.UnitTests
{
    internal class TestClass : ITestEntity
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ITestEntity nested;
        public ITestEntity Nested
        {
            get
            {
                return nested;
            }

            set
            {
                SetProperty(ref nested, value);
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                SetProperty(ref name, value);
            }
        }
    }
}
