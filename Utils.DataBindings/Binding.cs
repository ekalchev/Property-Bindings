using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utils.DataBindings
{
    public class Binding : IBinding
    {
        static readonly ConditionalWeakTable<object, Dictionary<MemberInfo, MemberActions>> objectSubs = new ConditionalWeakTable<object, Dictionary<MemberInfo, MemberActions>>();

        public virtual void Unbind()
        {
            
        }

        public static IBinding Create<T>(Expression<Func<T>> leftSide, Expression<Func<T>> rightSide)
        {
            return new PropertyBinding(leftSide.Body, rightSide.Body);
        }

        internal MemberChangeAction AddMemberChangeAction(object target, MemberInfo member, Action<int> k)
        {
            var key = Tuple.Create(target, member);
            if (objectSubs.TryGetValue(target, out var subs) == false)
            {
                subs = new Dictionary<MemberInfo, MemberActions>
                {
                    { member, new MemberActions(target, member)},
                };

                objectSubs.Add(target, subs);
            }

            // Debug.WriteLine ("ADD CHANGE ACTION " + target + " " + member);
            var sub = new MemberChangeAction(target, member, k);

            if(subs.ContainsKey(member) == false)
            {
                subs.Add(member, new MemberActions(target, member));
            }

            subs[member].AddAction(sub);
            return sub;
        }

        protected static bool SetValue(Expression expr, object value, int changeId)
        {
            if (expr.NodeType == ExpressionType.MemberAccess)
            {
                var m = (MemberExpression)expr;
                var mem = m.Member;

                var result = Evaluator.TryEvalExpression(m.Expression, out var target);

                if (result == true && target != null)
                {
                    var f = mem as FieldInfo;
                    var p = mem as PropertyInfo;

                    if (f != null)
                    {
                        f.SetValue(target, value);
                    }
                    else if (p != null)
                    {
                        p.SetValue(target, value, null);
                    }
                    else
                    {
                        //ReportError("Trying to SetValue on " + mem.GetType() + " member");
                        return false;
                    }

                    InvalidateMember(target, mem, changeId);
                    return true;
                }
            }

            //ReportError("Trying to SetValue on " + expr.NodeType + " expression");
            return false;
        }

        static internal void InvalidateMember(object target, MemberInfo member, int changeId = 0)
        {
            if (objectSubs.TryGetValue(target, out var subs))
            {
                // Debug.WriteLine ("INVALIDATE {0} {1}", target, member.Name);
                subs[member].Notify(changeId);
            }
        }

        internal void RemoveMemberChangeAction(MemberChangeAction sub)
        {
            if (objectSubs.TryGetValue(sub.Target, out var subs))
            {
                if (subs.TryGetValue(sub.Member, out var memberActions))
                {
                    // Debug.WriteLine ("REMOVE CHANGE ACTION " + sub.Target + " " + sub.Member);
                    memberActions.RemoveAction(sub);

                    if(memberActions.Count == 0)
                    {
                        subs.Remove(sub.Member);
                    }
                }

                if (subs.Count == 0)
                {
                    objectSubs.Remove(sub.Target);
                }
            }
        }
    }
}
