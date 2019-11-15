using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    internal class PropertyBinding : Binding
    {
        private Expression leftSide;
        private Expression rightSide;

        private PropertyPathBinding leftBinding;
        private PropertyPathBinding rightBinding;

        private PropertyPathBinding CreateBindingExpression(Expression expression)
        {
            List<BindingNode> bindingItems = new List<BindingNode>();

            Expression currentExpression = expression;
            BindingNode currentItem = null;
            BindingNode nextItem = null;

            while (currentExpression != null)
            {
                Type constructedType = typeof(BindingExpression<>).MakeGenericType(currentExpression.Type);
                currentItem = new BindingNode(Activator.CreateInstance(constructedType, currentExpression) as IBindingExpression);
                currentItem.Next = nextItem;

                if (nextItem != null)
                {
                    nextItem.Prev = currentItem;
                }

                nextItem = currentItem;

                if (currentExpression.NodeType == ExpressionType.MemberAccess)
                {
                    currentExpression = ((MemberExpression)currentExpression).Expression;
                }
                else
                {
                    currentExpression = null;
                }
            }

            return new PropertyPathBinding(currentItem);
        }

        private void ValidateExpression(Expression expression)
        {
            if (expression.NodeType != ExpressionType.MemberAccess
                || ((MemberExpression)expression).Member.MemberType != MemberTypes.Property)
            {
                throw new ArgumentException("Binding expression must be property accessor");
            }
        }

        public PropertyBinding(Expression leftSide, Expression rightSide, BindingDirection bindingDirection)
        {
            ValidateExpression(leftSide);
            ValidateExpression(rightSide);

            if(((PropertyInfo)((MemberExpression)leftSide).Member).PropertyType != ((PropertyInfo)((MemberExpression)rightSide).Member).PropertyType)
            {
                throw new ArgumentException("Both properties should be the same type");
            }

            this.leftSide = leftSide;
            this.rightSide = rightSide;

            leftBinding = CreateBindingExpression(leftSide);
            rightBinding = CreateBindingExpression(rightSide);

            if (bindingDirection == BindingDirection.RightToLeft)
            {
                var result = rightBinding.TryGetValue(out var value);

                bool leftSet = false;
                if (result == true)
                {
                    leftSet = leftBinding.TrySetValue(value);
                }
            }
            else
            {
                var result = leftBinding.TryGetValue(out var value);

                if (result == true)
                {
                    rightBinding.TrySetValue(value);
                }
            }

            Subscribe(this.leftBinding, this.rightBinding);
            Subscribe(this.rightBinding, this.leftBinding);
        }

        public override void Unbind()
        {
            leftBinding.Unsubscribe();
            rightBinding.Unsubscribe();

            // remove all reference to left and right side to GC can collect them
            leftSide = null;
            rightSide = null;
            leftBinding = null;
            rightBinding = null;

            base.Unbind();
        }

        void Subscribe(PropertyPathBinding expr, PropertyPathBinding dependentExpr)
        {
            Action action = null;
            action = () =>
            {
                OnSideChanged(expr, dependentExpr);
            };

            expr.Subscribe(action);
        }

        void OnSideChanged(PropertyPathBinding leftPropertyBinding, PropertyPathBinding rightPropertyBinding)
        {
            var result = leftPropertyBinding.TryGetValue(out var evaluatedValue);

            if (result == true)
            {
                rightPropertyBinding.TrySetValue(evaluatedValue);
            }
        }
    }

    public enum BindingDirection
    {
        RightToLeft,
        LeftToRight
    }
}
