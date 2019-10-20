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
        private int nextChangeId = 0;

        private Trigger leftTrigger;
        private Trigger rightTrigger;

        readonly HashSet<int> activeChangeIds = new HashSet<int>();

        public PropertyBinding(Expression leftSide, Expression rightSide)
        {
            this.leftSide = leftSide;
            this.rightSide = rightSide;

            // Try evaling the right and assigning left
            object value;
            var result = Evaluator.TryEvalExpression(this.rightSide, out value);

            bool leftSet = false;
            if (result == true)
            {
                leftSet = SetValue(this.leftSide, value, nextChangeId);
            }

            // If that didn't work, then try the other direction
            if (leftSet == false)
            {
                result = Evaluator.TryEvalExpression(this.leftSide, out value);

                if (result == true)
                {
                    SetValue(this.rightSide, value, nextChangeId);
                }
            }

            leftTrigger = CollectTriggers(this.leftSide);
            rightTrigger = CollectTriggers(this.rightSide);

            Resubscribe(leftTrigger, this.leftSide, this.rightSide);
            Resubscribe(rightTrigger, this.rightSide, this.leftSide);
        }

        public override void Unbind()
        {
            Unsubscribe(leftTrigger);
            Unsubscribe(rightTrigger);

            // remove all reference to left and right side to GC can collect them
            leftTrigger = null;
            rightTrigger = null;
            leftSide = null;
            rightSide = null;

            base.Unbind();
        }

        void Resubscribe(Trigger trigger, Expression expr, Expression dependentExpr)
        {
            Unsubscribe(trigger);
            Action<int> action = null;
            action = (changeId) =>
            {
                OnSideChanged(expr, dependentExpr, changeId);

                // if we have child triggers we must update the subscriptions
                Unsubscribe(trigger.Child);
                Subscribe(trigger.Child, action);
            };

            Subscribe(trigger, action);
        }

        void OnSideChanged(Expression expr, Expression dependentExpr, int causeChangeId)
        {
            if (activeChangeIds.Contains(causeChangeId) == false)
            {
                var result = Evaluator.TryEvalExpression(expr, out var evaluatedValue);

                if (result == true)
                {
                    var changeId = nextChangeId++;
                    activeChangeIds.Add(changeId);
                    SetValue(dependentExpr, evaluatedValue, changeId);
                    activeChangeIds.Remove(changeId);
                }
            }
        }

        private void Unsubscribe(Trigger trigger)
        {
            if (trigger != null)
            {
                Unsubscribe(trigger.Child);

                if (trigger.ChangeAction != null)
                {
                    RemoveMemberChangeAction(trigger.ChangeAction);
                }
            }
        }

        private void Subscribe(Trigger trigger, Action<int> action)
        {
            if (trigger != null)
            {
                Subscribe(trigger.Child, action);

                var result = Evaluator.TryEvalExpression(trigger.Expression, out var value);

                if (result == true && value != null)
                {
                    trigger.ChangeAction = AddMemberChangeAction(value, trigger.Member, action);
                }
            }
        }

        private Trigger CollectTriggers(Expression expression, Trigger child = null)
        {
            Trigger result = null;

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)expression;
                var trigger = new Trigger(m.Expression, m.Member, child);
                result = CollectTriggers(m.Expression, trigger) ?? trigger;
            }

            return result;
        }
    }
}
