using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class EntryActionBehavior
        {
            public EntryActionBehavior(Action<Transition, object[]> action, string actionDescription)
            {
                Action = action;
                ActionDescription = Enforce.ArgumentNotNull(actionDescription, nameof(actionDescription));
            }

            internal string ActionDescription { get; }

            internal Action<Transition, object[]> Action { get; }
        }
    }
}
