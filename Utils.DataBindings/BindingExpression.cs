using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    class BindingExpression
    {
        private Trigger trigger;
        private readonly Expression expression;
        public BindingExpression Child { get; }
        private Delegate bindingDelegate;

        public BindingExpression(Expression expression, BindingExpression child, BindingExpression parent)
        {
            this.expression = expression;
            Child = child;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var lambda = Expression.Lambda(expression, Enumerable.Empty<ParameterExpression>());
                bindingDelegate = lambda.Compile();

                var memberExpression = (MemberExpression)expression;
                trigger = new Trigger(memberExpression.Expression, memberExpression.Member);
            }
        }

        public void Subscribe(Action action)
        {
            SubscribeRecursive(this, action);
        }

        private void SubscribeRecursive(BindingExpression expression, Action action)
        {
            if (expression.trigger != null)
            {
                SubscribeRecursive(expression.Child, action);

                if (expression.TryGetValue(out var target) == true && target != null)
                {
                    trigger.ChangeAction = Binding.AddMemberChangeAction(target, expression.trigger.Member, i => action());
                }
            }
        }

        public void Unsubscribe()
        {
            UnsubscribeRecursive(this);
        }

        private void UnsubscribeRecursive(BindingExpression expression)
        {
            if (expression.trigger != null)
            {
                UnsubscribeRecursive(expression.Child);

                if (expression.trigger.ChangeAction != null)
                {
                    Binding.RemoveMemberChangeAction(expression.trigger.ChangeAction);
                }
            }
        }

        public bool TryGetValue(out object result)
        {
            return TryGetValue(this, out result);
        }

        public bool TrySetValue(object value)
        {
            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)expression;
                var member = m.Member;

                var result = Child.TryGetValue(out var target);

                if (result == true && target != null)
                {
                    var propertyInfo = member as PropertyInfo;

                    if (propertyInfo != null)
                    {
                        var currentValue = propertyInfo.GetValue(target);
                        if (((dynamic)currentValue) != (dynamic)value)
                        {
                            propertyInfo.SetValue(target, value, null);
                            //Binding.InvalidateMember(target, member, 0);
                        }
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                }
            }

            //ReportError("Trying to SetValue on " + expr.NodeType + " expression");
            return false;
        }

        private bool TryGetValue(BindingExpression bindingExpression, out object result)
        {
            bool ret = true;
            result = null;

            if (bindingExpression.Child != null)
            {
                ret = TryGetValue(bindingExpression.Child, out result);
                ret = result != null;
            }

            if (ret == true)
            {
                var expr = bindingExpression.expression;
                if (expr.NodeType == ExpressionType.Constant)
                {
                    result = ((ConstantExpression)expr).Value;
                    ret = true;
                }
                else if (expr.NodeType == ExpressionType.MemberAccess)
                {
                    // we must have precompiled binding delegate
                    result = bindingExpression.bindingDelegate.DynamicInvoke();
                    ret = true;
                }
                else
                {
                    throw new NotSupportedException("Not supported expression");
                }
            }

            return ret;
        }
    }
}
