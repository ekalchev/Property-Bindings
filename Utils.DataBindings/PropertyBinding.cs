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
        private int nextChangeId = 1;

        private Trigger leftTrigger;
        private Trigger rightTrigger;

        readonly HashSet<int> activeChangeIds = new HashSet<int>();
        private object Value;

        public PropertyBinding(Expression leftSide, Expression rightSide)
        {
            this.leftSide = leftSide;
            this.rightSide = rightSide;

            // Try evaling the right and assigning left
            Value = Evaluator.EvalExpression(this.rightSide);
            var leftSet = SetValue(this.leftSide, Value, nextChangeId);

            // If that didn't work, then try the other direction
            if (!leftSet)
            {
                Value = Evaluator.EvalExpression(this.leftSide);
                SetValue(this.rightSide, Value, nextChangeId);
            }

            nextChangeId++;

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
            Value = null;
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
            if (activeChangeIds.Contains(causeChangeId))
                return;

            var v = Evaluator.EvalExpression(expr);

            if (v == null && Value == null)
                return;

            if ((v == null && Value != null) ||
                (v != null && Value == null) ||
                ((v is IComparable) && ((IComparable)v).CompareTo(Value) != 0))
            {

                Value = v;

                var changeId = nextChangeId++;
                activeChangeIds.Add(changeId);
                SetValue(dependentExpr, v, changeId);
                activeChangeIds.Remove(changeId);
            }
            //			else {
            //				Debug.WriteLine ("Prevented needless update");
            //			}
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

                var value = Evaluator.EvalExpression(trigger.Expression);

                if (value != null)
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
