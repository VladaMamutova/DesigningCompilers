using System.Collections.Generic;
using System.Linq;

namespace RegexpLexer.Logic
{
    public class Fsm
    {
        public State Start { get; }
        public List<State> Final { get; }

        public Fsm(State start)
        {
            Start = start;
            Final = new List<State>();
        }

        public Fsm(State start, State final)
        {
            Start = start;
            Final = new List<State> { final };
        }

        public Fsm(State start, List<State> final)
        {
            Start = start;
            Final = final;
        }

        public void AddFinalState(State state)
        {
            if (!Final.Contains(state))
            {
                Final.Add(state);
            }
        }

        public HashSet<State> GetAllStates()
        {
            var states = new Dictionary<State, bool> {{Start, false}};
            while (states.Values.Contains(false))
            {
                var state = states.First(s => !s.Value).Key;
                states[state] = true;
                foreach (var move in state.Moves)
                {
                    if (!states.ContainsKey(move.Value))
                    {
                        states.Add(move.Value, false);
                    }
                }
            }

            return states.Keys.ToHashSet();
        }

        public override string ToString()
        {
            return $"{Start} -> {{{string.Join(", ", Final)}}}";
        }

        public string ShowFinalStates()
        {
            return $"{{{string.Join(", ", Final)}}}";
        }
    }
}
