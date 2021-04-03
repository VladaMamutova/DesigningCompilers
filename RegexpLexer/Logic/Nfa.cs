using System.Collections.Generic;

namespace RegexpLexer.Logic
{
    public class Nfa
    {
        public State Start { get; }
        public List<State> Final { get; }

        public Nfa(State start)
        {
            Start = start;
            Final = new List<State>();
        }

        public Nfa(State start, State final)
        {
            Start = start;
            Final = new List<State> { final };
        }

        public Nfa(State start, List<State> final)
        {
            Start = start;
            Final = final;
        }

        public void AddFinalState(State state)
        {
            Final.Add(state);
        }

        public override string ToString()
        {
            return $"{Start} -> {{{string.Join(", ", Final)}}}";
        }
    }
}
