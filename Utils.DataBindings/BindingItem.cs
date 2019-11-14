using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.DataBindings
{
    internal class BindingItem
    {
        public BindingItem(IBindingExpression bindingExpression)
        {
            BindingExpression = bindingExpression;
        }

        public BindingItem Prev { get; set; }
        public BindingItem Next { get; set; }
        public IBindingExpression BindingExpression { get; }
    }
}
