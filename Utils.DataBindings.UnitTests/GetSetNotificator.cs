using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Utils.DataBindings.UnitTests
{
    internal class GetSetNotificator : TestClass
    {
        public event EventHandler<EventArgs> GetInvoked;
        public event EventHandler<EventArgs> SetInvoked;

        public int GetCounts { get; private set; }
        public int SetCounts { get; private set; }

        public void ResetCounts()
        {
            GetCounts = 0;
            SetCounts = 0;
        }

        public override string Name
        {
            get
            {
                GetCounts++;
                GetInvoked?.Invoke(this, EventArgs.Empty);
                return base.Name;
            }

            set
            {
                SetCounts++;
                SetInvoked?.Invoke(this, EventArgs.Empty);
                base.Name = value;
            }
        }
    }
}
