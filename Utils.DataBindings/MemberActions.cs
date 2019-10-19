using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Utils.DataBindings
{
    internal class MemberActions
    {
        readonly object target;
        readonly MemberInfo member;

        EventInfo eventInfo;
        Delegate eventHandler;

        public MemberActions(object target, MemberInfo mem)
        {
            this.target = target;
            member = mem;
        }

        void AddChangeNotificationEventHandler()
        {
            if (target != null)
            {
                var npc = target as INotifyPropertyChanged;
                if (npc != null && (member is PropertyInfo))
                {
                    npc.PropertyChanged += HandleNotifyPropertyChanged;
                }
            }
        }

        bool AddHandlerForFirstExistingEvent(params string[] names)
        {
            var type = target.GetType();
            foreach (var name in names)
            {
                var ev = GetEvent(type, name);

                if (ev != null)
                {
                    eventInfo = ev;
                    var isClassicHandler = typeof(EventHandler).GetTypeInfo().IsAssignableFrom(ev.EventHandlerType.GetTypeInfo());

                    eventHandler = isClassicHandler ?
                        (EventHandler)HandleAnyEvent :
                        CreateGenericEventHandler(ev, () => HandleAnyEvent(null, EventArgs.Empty));

                    ev.AddEventHandler(target, eventHandler);
                    //Debug.WriteLine("BIND: Added handler for {0} on {1}", eventInfo.Name, target);
                    return true;
                }
            }
            return false;
        }

        static EventInfo GetEvent(Type type, string eventName)
        {
            var t = type;
            while (t != null && t != typeof(object))
            {
                var ti = t.GetTypeInfo();
                var ev = t.GetTypeInfo().GetDeclaredEvent(eventName);
                if (ev != null)
                    return ev;
                t = ti.BaseType;
            }
            return null;
        }

        static Delegate CreateGenericEventHandler(EventInfo evt, Action d)
        {
            var handlerType = evt.EventHandlerType;
            var handlerTypeInfo = handlerType.GetTypeInfo();
            var handlerInvokeInfo = handlerTypeInfo.GetDeclaredMethod("Invoke");
            var eventParams = handlerInvokeInfo.GetParameters();

            //lambda: (object x0, EventArgs x1) => d()
            var parameters = eventParams.Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
            var body = Expression.Call(Expression.Constant(d), d.GetType().GetTypeInfo().GetDeclaredMethod("Invoke"));
            var lambda = Expression.Lambda(body, parameters);

            var delegateInvokeInfo = lambda.Compile().GetMethodInfo();
            return delegateInvokeInfo.CreateDelegate(handlerType, null);
        }

        void UnsubscribeFromChangeNotificationEvent()
        {
            var npc = target as INotifyPropertyChanged;
            if (npc != null && (member is PropertyInfo))
            {
                npc.PropertyChanged -= HandleNotifyPropertyChanged;
                return;
            }

            if (eventInfo == null)
                return;

            eventInfo.RemoveEventHandler(target, eventHandler);

            Debug.WriteLine("BIND: Removed handler for {0} on {1}", eventInfo.Name, target);

            eventInfo = null;
            eventHandler = null;
        }

        void HandleNotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == member.Name)
            {
                Binding.InvalidateMember(target, member);
            }
        }

        void HandleAnyEvent(object sender, EventArgs e)
        {
            Binding.InvalidateMember(target, member);
        }

        readonly List<MemberChangeAction> actions = new List<MemberChangeAction>();

        /// <summary>
        /// Add the specified action to be executed when Notify() is called.
        /// </summary>
        /// <param name="action">Action.</param>
        public void AddAction(MemberChangeAction action)
        {
            if (actions.Count == 0)
            {
                AddChangeNotificationEventHandler();
            }

            actions.Add(action);
        }

        public void RemoveAction(MemberChangeAction action)
        {
            actions.Remove(action);

            if (actions.Count == 0)
            {
                UnsubscribeFromChangeNotificationEvent();
            }
        }

        public int Count
        {
            get
            {
                return actions.Count;
            }
        }

        /// <summary>
        /// Execute all the actions.
        /// </summary>
        /// <param name="changeId">Change identifier.</param>
        public void Notify(int changeId)
        {
            foreach (var s in actions.ToArray())
            {
                s.Notify(changeId);
            }
        }
    }
}
