﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.DataBindings
{
    public interface IBinding : IDisposable
    {
        void Unbind();
    }
}
