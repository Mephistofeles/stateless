using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class StateRepresentation
        {
            internal IDictionary<TTrigger, ICollection<TriggerBehaviour>> TriggerBehaviours { get; } = new Dictionary<TTrigger, ICollection<TriggerBehaviour>>();

            internal ICollection<EntryActionBehavior> EntryActions { get; } = new List<EntryActionBehavior>();
            internal ICollection<ExitActionBehavior> ExitActions { get; } = new List<ExitActionBehavior>();

            private readonly ICollection<StateRepresentation> _substates = new List<StateRepresentation>();

            public StateRepresentation(TState state)
            {
                UnderlyingState = state;
            }

            public bool CanHandle(TTrigger trigger)
            {
                TriggerBehaviour unused;
                return TryFindHandler(trigger, out unused);
            }

            public bool TryFindHandler(TTrigger trigger, out TriggerBehaviour handler)
            {
                return TryFindLocalHandler(trigger, out handler, t => t.IsGuardConditionMet) ||
                       (Superstate != null && Superstate.TryFindHandler(trigger, out handler));
            }

            private bool TryFindLocalHandler(TTrigger trigger, out TriggerBehaviour handler, params Func<TriggerBehaviour, bool>[] filters)
            {
                ICollection<TriggerBehaviour> possible;
                if (!TriggerBehaviours.TryGetValue(trigger, out possible))
                {
                    handler = null;
                    return false;
                }

                var actual = filters.Aggregate(possible, (current, filter) => current.Where(filter).ToArray());

                if (actual.Count() > 1)
                    throw new InvalidOperationException(
                        string.Format(StateRepresentationResources.MultipleTransitionsPermitted,
                        trigger, UnderlyingState));

                handler = actual.FirstOrDefault();
                return handler != null;
            }

            public bool TryFindHandlerWithUnmetGuardCondition(TTrigger trigger, out TriggerBehaviour handler)
            {
                return TryFindLocalHandler(trigger, out handler, t => !t.IsGuardConditionMet) || 
                       (Superstate != null && Superstate.TryFindHandlerWithUnmetGuardCondition(trigger, out handler));
            }

            public void AddEntryAction(TTrigger trigger, Action<Transition, object[]> action, string entryActionDescription)
            {
                Enforce.ArgumentNotNull(action, nameof(action));
                EntryActions.Add(
                    new EntryActionBehavior((t, args) =>
                    {
                        if (t.Trigger.Equals(trigger))
                            action(t, args);
                    },
                    Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddEntryAction(Action<Transition, object[]> action, string entryActionDescription)
            {
                EntryActions.Add(
                    new EntryActionBehavior(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddExitAction(Action<Transition> action, string exitActionDescription)
            {
                ExitActions.Add(
                    new ExitActionBehavior(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(exitActionDescription, nameof(exitActionDescription))));
            }

            public void Enter(Transition transition, params object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    ExecuteEntryActions(transition, entryArgs);
                }
                else if (!Includes(transition.Source))
                {
                    Superstate?.Enter(transition, entryArgs);

                    ExecuteEntryActions(transition, entryArgs);
                }
            }

            public void Exit(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    ExecuteExitActions(transition);
                }
                else if (!Includes(transition.Destination))
                {
                    ExecuteExitActions(transition);
                    Superstate?.Exit(transition);
                }
            }

            private void ExecuteEntryActions(Transition transition, object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                Enforce.ArgumentNotNull(entryArgs, nameof(entryArgs));
                foreach (var action in EntryActions)
                    action.Action(transition, entryArgs);
            }

            private void ExecuteExitActions(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                foreach (var action in ExitActions)
                    action.Action(transition);
            }

            public void AddTriggerBehaviour(TriggerBehaviour triggerBehaviour)
            {
                ICollection<TriggerBehaviour> allowed;
                if (!TriggerBehaviours.TryGetValue(triggerBehaviour.Trigger, out allowed))
                {
                    allowed = new List<TriggerBehaviour>();
                    TriggerBehaviours.Add(triggerBehaviour.Trigger, allowed);
                }
                allowed.Add(triggerBehaviour);
            }

            public StateRepresentation Superstate { get; set; }

            public TState UnderlyingState { get; }

            public void AddSubstate(StateRepresentation substate)
            {
                Enforce.ArgumentNotNull(substate, nameof(substate));
                _substates.Add(substate);
            }

            public bool Includes(TState state)
            {
                return UnderlyingState.Equals(state) || _substates.Any(s => s.Includes(state));
            }

            public bool IsIncludedIn(TState state)
            {
                return
                    UnderlyingState.Equals(state) ||
                    (Superstate != null && Superstate.IsIncludedIn(state));
            }

            public IEnumerable<TTrigger> PermittedTriggers
            {
                get
                {
                    var result = TriggerBehaviours
                        .Where(t => t.Value.Any(a => a.IsGuardConditionMet))
                        .Select(t => t.Key);

                    if (Superstate != null)
                        result = result.Union(Superstate.PermittedTriggers);

                    return result.ToArray();
                }
            }
        }
    }
}
