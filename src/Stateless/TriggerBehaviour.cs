using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            protected TriggerBehaviour(TTrigger trigger, Func<bool> guard, string guardDescription)
            {
                Trigger = trigger;
                Guard = guard;
                GuardDescription = Enforce.ArgumentNotNull(guardDescription, nameof(guardDescription));
            }

            public TTrigger Trigger { get; }
            internal Func<bool> Guard { get; }
            internal string GuardDescription { get; }

            public bool IsGuardConditionMet => Guard();

            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);
        }
    }
}
