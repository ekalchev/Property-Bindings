using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.DataBindings
{
    internal class BindingItem
    {
        public BindingItem(BindingExpression bindingExpression)
        {
            BindingExpression = bindingExpression;
        }

        public BindingItem Prev { get; set; }
        public BindingItem Next { get; set; }
        public BindingExpression BindingExpression { get; }
    }
}
