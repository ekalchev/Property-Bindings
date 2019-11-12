using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    internal class Trigger
    {
        public Trigger(Expression expression, MemberInfo member)
        {
            Expression = expression;
            Member = member;
        }

        public Expression Expression { get; }
        public MemberInfo Member { get; }
        public MemberChangeAction ChangeAction { get; set; }
    }
}
