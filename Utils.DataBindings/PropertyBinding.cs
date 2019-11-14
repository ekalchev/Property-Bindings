using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            List<BindingItem> bindingItems = new List<BindingItem>();

            Expression currentExpression = expression;
            BindingItem currentItem = null;
            BindingItem nextItem = null;

            while (currentExpression != null)
            {
                Type constructedType = typeof(BindingExpression<>).MakeGenericType(currentExpression.Type);
                currentItem = new BindingItem(Activator.CreateInstance(constructedType, currentExpression) as IBindingExpression);
                currentItem.Next = nextItem;

                if(nextItem != null)
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

        public PropertyBinding(Expression leftSide, Expression rightSide)
        {
            this.leftSide = leftSide;
            this.rightSide = rightSide;

            leftBinding = CreateBindingExpression(leftSide);
            rightBinding = CreateBindingExpression(rightSide);

            // Try evaling the right and assigning left
            object value;
            var result = rightBinding.TryGetValue(out value);

            bool leftSet = false;
            if (result == true)
            {
                leftSet = leftBinding.TrySetValue(value);
            }

            //// If that didn't work, then try the other direction
            if (leftSet == false)
            {
                result = leftBinding.TryGetValue(out value);

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
}
