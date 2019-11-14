using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    class BindingExpression<T> : IBindingExpression
    {
        private MemberChangeAction changeAction;
        public Expression Expression { get; }
        private MemberInfo memberInfo;
        private Func<T> bindingDelegate;

        public BindingExpression(Expression expression)
        {
            this.Expression = expression;

            var lambda = Expression.Lambda(expression, Enumerable.Empty<ParameterExpression>());
            bindingDelegate = (Func<T>)lambda.Compile();

            if(expression.NodeType == ExpressionType.MemberAccess)
            {
                memberInfo = ((MemberExpression)Expression).Member;
            }
        }

        public void Subscribe(object target, Action action)
        {
            if (target != null && memberInfo != null)
            {
                changeAction = Binding.AddMemberChangeAction(target, memberInfo, i => action());
            }
        }

        public void Unsubscribe()
        {
            if (changeAction != null)
            {
                Binding.RemoveMemberChangeAction(changeAction);
            }
        }

        public object Value
        {
            get
            {
                return Evaluate();
            }
        }

        private object Evaluate()
        {
            if (Expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)Expression).Value;
            }
            else if (Expression.NodeType == ExpressionType.MemberAccess)
            {
                return bindingDelegate.Invoke();
            }
            else
            {
                throw new NotSupportedException("Not supported expression");
            }
        }
    }

    public interface IBindingExpression
    {
        object Value { get; }
        void Unsubscribe();
        void Subscribe(object target, Action action);
        Expression Expression { get; }
    }
}
