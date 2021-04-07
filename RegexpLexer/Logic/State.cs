using System.Collections.Generic;
using System.Linq;

namespace RegexpLexer.Logic
{
    public class State
    {
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

        public List<char> GetAlphabet()
        {
            return Moves.Select(move => move.Key).Distinct().ToList();
        }

        public List<State> GetNextStatesByChar(char c)
        {
            return Moves.Where(move => move.Key == c).Select(move => move.Value)
                .ToList();
        }
        
        public State GetNextStateByChar(char c)
        {
            return Moves.First(move => move.Key == c).Value;
        }

        public bool CompareNextStates(HashSet<State> states)
        {
            var nextStates = Moves.Select(move => move.Value).ToList();
            return states.Count == nextStates.Count && nextStates.All(states.Contains);
        }

        public override bool Equals(object obj)
        {
            return obj is State item && Id.Equals(item.Id);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString()
        {
            return $"s{Id}";
        }

        public string ToString(string name)
        {
            return $"{name}{Id}";
        }

        public string ShowOutStates()
        {
            return string.Join(", ", Moves.Select(state => state.Value));
        }
    }
}
