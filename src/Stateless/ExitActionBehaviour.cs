using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ExitActionBehavior
        {
            public ExitActionBehavior(Action<Transition> action, string actionDescription)
            {
                Action = action;
                ActionDescription = Enforce.ArgumentNotNull(actionDescription, nameof(actionDescription));
            }

            internal string ActionDescription { get; }
            internal Action<Transition> Action { get; }
        }
    }
}
