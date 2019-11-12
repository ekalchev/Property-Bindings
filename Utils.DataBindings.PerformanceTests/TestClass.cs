using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

internal class TestClass : INotifyPropertyChanged
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

    private TestClass nested;
    public TestClass Nested
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
    public virtual string Name
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

    private int intProperty;
    public int IntProperty
    {
        get
        {
            return intProperty;
        }

        set
        {
            SetProperty(ref intProperty, value);
        }
    }

    private int selfModifingProperty;
    public int SelfModifingProperty
    {
        get
        {
            return selfModifingProperty;
        }

        set
        {
            if (value == 0)
            {
                value++;
            }

            SetProperty(ref selfModifingProperty, value);
        }
    }
}
