using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Utils.DataBindings.UnitTests
{
    interface ITestEntity : INotifyPropertyChanged
    {
        ITestEntity Nested { get; set; }
        string Name { get; set; }
    }
}
