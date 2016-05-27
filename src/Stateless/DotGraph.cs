using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// A string representation of the state machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public string ToDotGraph()
        {
            var lines = new List<string>();
            var unknownDestinations = new List<string>();

            foreach (var stateCfg in _stateConfiguration)
            {
                var source = stateCfg.Key;
                foreach (var behaviours in stateCfg.Value.TriggerBehaviours)
                {
                    foreach (var behaviour in behaviours.Value)
                    {
                        string destination;

                        var triggerBehaviour = behaviour as TransitioningTriggerBehaviour;
                        if (triggerBehaviour != null)
                        {
                            destination = triggerBehaviour.Destination.ToString();
                        }
                        else
                        {
                            destination = "unknownDestination_" + unknownDestinations.Count;
                            unknownDestinations.Add(destination);
                        }

                        var line = behaviour.Guard.GetMethodInfo().DeclaringType.Namespace.Equals("Stateless") ?
                            $" {source} -> {destination} [label=\"{behaviour.Trigger}\"];"
                            : $" {source} -> {destination} [label=\"{behaviour.Trigger} [{behaviour.GuardDescription}]\"];";

                        lines.Add(line);
                    }
                }
            }

            if (unknownDestinations.Any())
            {
                var label = $" {{ node [label=\"?\"] {string.Join(" ", unknownDestinations)} }};";
                lines.Insert(0, label);
            }

            if (_stateConfiguration.Any(s => s.Value.EntryActions.Any() || s.Value.ExitActions.Any()))
            {
                lines.Add("node [shape=box];");

                foreach (var stateCfg in _stateConfiguration)
                {
                    var source = stateCfg.Key;

                    lines.AddRange(stateCfg.Value.EntryActions.Select(entryActionBehaviour =>
                        $" {source} -> \"{entryActionBehaviour.ActionDescription}\" [label=\"On Entry\" style=dotted];"));

                    lines.AddRange(stateCfg.Value.ExitActions.Select(exitActionBehaviour =>
                        $" {source} -> \"{exitActionBehaviour.ActionDescription}\" [label=\"On Exit\" style=dotted];"));
                }
            }

            return "digraph {" + System.Environment.NewLine +
                     string.Join(System.Environment.NewLine, lines) + System.Environment.NewLine +
                   "}";
        }
    }
}
