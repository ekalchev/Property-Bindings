using System;
using System.Collections.Generic;
using System.Text;

namespace Utils.DataBindings
{
    internal class BindingNode
    {
        public BindingNode(IBindingExpression bindingExpression)
        {
            BindingExpression = bindingExpression;
        }

        public BindingNode Prev { get; set; }
        public BindingNode Next { get; set; }
        public IBindingExpression BindingExpression { get; }
    }
}
