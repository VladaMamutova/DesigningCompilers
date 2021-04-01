﻿using System.Collections.Generic;

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
    }
}
