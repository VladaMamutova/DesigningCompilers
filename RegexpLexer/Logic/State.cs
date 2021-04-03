using System.Collections.Generic;
using System.Linq;

namespace RegexpLexer.Logic
{
    public class State
    {
        private const int StartId = -1;
        private const int FinalId = -2;
        public static readonly State Start = new State(StartId);
        public static readonly State Match = new State(FinalId);

        public int Id { get; }
        public List<KeyValuePair<char, State>> Moves { get; }

        public State(int id)
        {
            Id = id;
            Moves = new List<KeyValuePair<char, State>>();
        }

        public void AddMove(char c, State state)
        {
            Moves.Add(new KeyValuePair<char, State>(c, state));
        }

        public void AddAllMoves(List<KeyValuePair<char, State>> moves)
        {
            Moves.AddRange(moves);
        }

        public List<char> GetOutputAlphabet()
        {
            List<char> chars = new List<char>();
            foreach (var move in Moves)
            {
                chars.AddRange(move.Value.Moves.Select(m => m.Key));
            }

            return chars.Distinct().ToList();
        }

        public List<State> FindOutStates(char c)
        {
            List<State> states = new List<State>();
            foreach (var move in Moves)
            {
                foreach (var nextMove in move.Value.Moves)
                {
                    if (nextMove.Key == c)
                    {
                        states.Add(nextMove.Value);
                    }
                }
            }

            return states;
        }

        public bool CheckOutStates(List<State> states)
        {
            var nextStates = Moves.Select(move => move.Value).ToList();
            if (states.Count != nextStates.Count) return false;
            foreach (var state in nextStates)
            {
                if (!states.Contains(state)) return false;
            }

            return true;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is State item))
            {
                return false;
            }

            return Id.Equals(item.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString()
        {
            switch (Id)
            {
                case StartId: return "start";
                case FinalId: return "match";
                default: return $"s{Id}";
            }
        }

        public string ShowOutStates()
        {
            return $"{string.Join(", ", Moves.Select(state => state.Value))}";
        }
    }
}
