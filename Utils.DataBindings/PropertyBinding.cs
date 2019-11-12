using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Utils.DataBindings
{
    internal class PropertyBinding : Binding
    {
        private Expression leftSide;
        private Expression rightSide;

        private BindingExpression leftExpression;
        private BindingExpression rightExpression;

        readonly HashSet<int> activeChangeIds = new HashSet<int>();

        private BindingExpression Build(Expression expression, BindingExpression parent)
        {
            BindingExpression child = null;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                child = Build(((MemberExpression)expression).Expression, parent);
            }

            return new BindingExpression(expression, child, parent);
        }

        public PropertyBinding(Expression leftSide, Expression rightSide)
        {
            this.leftSide = leftSide;
            this.rightSide = rightSide;

            leftExpression = Build(leftSide, null);
            //rightExpression = new BindingExpression(rightSide, null);

            // Try evaling the right and assigning left
            object value;
            var result = rightExpression.TryGetValue(out value);

            bool leftSet = false;
            if (result == true)
            {
                leftSet = leftExpression.TrySetValue(value);
            }

            // If that didn't work, then try the other direction
            if (leftSet == false)
            {
                result = leftExpression.TryGetValue(out value);

                if (result == true)
                {
                    rightExpression.TrySetValue(value);
                }
            }

            Resubscribe(this.leftExpression, this.rightExpression);
            Resubscribe(this.rightExpression, this.leftExpression);
        }

        public override void Unbind()
        {
            leftExpression.Unsubscribe();
            rightExpression.Unsubscribe();

            // remove all reference to left and right side to GC can collect them
            leftSide = null;
            rightSide = null;
            leftExpression = null;
            rightExpression = null;

            base.Unbind();
        }

        void Resubscribe(BindingExpression expr, BindingExpression dependentExpr)
        {
            expr.Unsubscribe();
            Action action = null;
            action = () =>
            {
                OnSideChanged(expr, dependentExpr);

                // if we have child triggers we must update the subscriptions
                if(expr.Child != null)
                {
                    expr.Child.Unsubscribe();
                    expr.Child.Subscribe(action);
                }
            };

            expr.Subscribe(action);
        }

        void OnSideChanged(BindingExpression expr, BindingExpression dependentExpr)
        {
            var result = expr.TryGetValue(out var evaluatedValue);

            if (result == true)
            {
                dependentExpr.TrySetValue(evaluatedValue);
            }
        }
    }
}
