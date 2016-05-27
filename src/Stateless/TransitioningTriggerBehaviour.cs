using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitioningTriggerBehaviour : TriggerBehaviour
        {
            internal TState Destination { get; }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard)
                : this(trigger, destination, guard, string.Empty)
            {
            }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard, string description)
                : base(trigger, guard, description)
            {
                Destination = destination;
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = Destination;
                return true;
            }
        }
    }
}
