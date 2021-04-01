using System.Collections.Generic;

namespace RegexpLexer.Logic
{
    public class Nfa
    {
        public State Start;
        public List<State> Final;

        public Nfa(State start, List<State> final)
        {
            Start = start;
            Final = final;
        }

        public Nfa(State start, State final)
        {
            Start = start;
            Final = new List<State> { final };
        }

        public override string ToString()
        {
            return $"{Start} -> {{{string.Join(", ", Final)}}}";
        }
    }
}
