using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    internal class MemberChangeAction
    {
        readonly Action<int> action;

        public object Target { get; private set; }
        public MemberInfo Member { get; private set; }


        public MemberChangeAction(object target, MemberInfo member, Action<int> action)
        {
            Target = target;
            Member = member ?? throw new ArgumentNullException("member");
            this.action = action ?? throw new ArgumentNullException("action");
        }

        public void Notify(int changeId)
        {
            action(changeId);
        }
    }
}
