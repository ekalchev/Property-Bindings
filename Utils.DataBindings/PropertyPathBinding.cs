using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    internal class PropertyPathBinding
    {
        private PropertyInfo propertyInfo;

        public PropertyPathBinding(BindingNode bindingItem)
        {
            BindingItem = bindingItem;

            var lastChild = GetLastChild(bindingItem);

            var expression = lastChild.BindingExpression.Expression;
            if (lastChild.BindingExpression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                propertyInfo = ((MemberExpression)expression).Member as PropertyInfo;
            }
            else
            {
                throw new NotSupportedException("Please provide property expression");
            }
        }

        public BindingNode BindingItem { get; }

        public bool TryGetValue(out object value)
        {
            value = null;
            BindingNode current = BindingItem;

            while (current != null)
            {
                value = current.BindingExpression.Value;

                if (value == null)
                {
                    return current.Next != null ? false : true;
                }

                current = current.Next;
            }

            return true;
        }

        private BindingNode GetLastChild(BindingNode item)
        {
            if(item.Next != null)
            {
                return GetLastChild(item.Next);
            }

            return item;
        }

        public bool TrySetValue(object value)
        {
            bool result = false;
            BindingNode current = BindingItem;
            while (current != null)
            {
                // that will be the target of Last property of the path
                if (current.Next?.Next == null)
                {
                    var target = current.BindingExpression.Value;

                    if (target != null)
                    {
                        if((value == null && current.Next.BindingExpression.Value != null)
                            || value != null && value.Equals(current.Next.BindingExpression.Value) == false)
                        {
                            propertyInfo.SetValue(target, value, null);
                        }
                        result = true;
                    }
                    else
                    {
                        result = false;
                    }

                    break;
                }
                else
                {
                    if(current.BindingExpression.Value == null)
                    {
                        return false;
                    }
                }

                current = current.Next;
            }

            return result;
        }

        public void Subscribe(Action action)
        {
            Subscribe(BindingItem, action);
        }

        private void Subscribe(BindingNode bindingItem, Action action)
        {
            var current = bindingItem;
            BindingNode prev = null;

            while (current != null)
            {
                var innerCurrent = current;
                current.BindingExpression.Subscribe(prev?.BindingExpression?.Value, () => 
                {
                    action();

                    // if that is leaf property dont resubscribe
                    if (innerCurrent?.Next != null)
                    {
                        Unsubscribe(innerCurrent);
                        Subscribe(innerCurrent, action);
                    }
                });

                prev = current;
                current = current.Next;
            }
        }

        public void Unsubscribe(BindingNode bindingItem)
        {
            var current = bindingItem;
            while (current != null)
            {
                current.BindingExpression.Unsubscribe();
                current = current.Next;
            }
        }

        public void Unsubscribe()
        {
            Unsubscribe(BindingItem);
        }
    }
}
