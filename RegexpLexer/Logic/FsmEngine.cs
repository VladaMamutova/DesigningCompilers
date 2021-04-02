using System;
using System.Collections.Generic;

namespace RegexpLexer.Logic
{
    public static class FsmEngine
    {
        public const char Epsilon = 'є';
        public const string Indent = "   ";
        
        private static int _stateId;

        private static State NewState()
        {
            return new State(_stateId++);
        }

        private static Nfa NewSimpleNfa(char c)
        {
            var start = NewState();
            var final = NewState();
            start.AddMove(c, final);
            return new Nfa(start, final);
        }
        
        public static State PostfixToNfa(string postfix)
        {
            _stateId = 0;

            var nfaStack = new Stack<Nfa>();
            var step = 1;
            foreach (var c in postfix)
            {
                Nfa nfa;
                switch (c)
                {
                    case '.': // конкатенация
                    {
                        var nfa2 = nfaStack.Pop();
                        var nfa1 = nfaStack.Pop();

                        nfa1.Final.ForEach(f =>
                            f.AddAllMoves(nfa2.Start.Moves));

                        nfa = new Nfa(nfa1.Start, nfa2.Final);
                        break;
                    }
                    case '|': // альтернатива
                    {
                        var nfa2 = nfaStack.Pop();
                        var nfa1 = nfaStack.Pop();

                        var start = NewState();
                        start.AddMove(Epsilon, nfa1.Start);
                        start.AddMove(Epsilon, nfa2.Start);

                        var final = NewState();
                        nfa1.Final.ForEach(f => f.AddMove(Epsilon, final));
                        nfa2.Final.ForEach(f => f.AddMove(Epsilon, final));

                        nfa = new Nfa(start, final);
                        break;
                    }
                    case '*': // ноль или больше
                    {
                        var nfa1 = nfaStack.Pop();

                        var start = NewState();
                        var final = NewState();

                        start.AddMove(Epsilon, nfa1.Start);
                        start.AddMove(Epsilon, final);

                        nfa1.Final.ForEach(f =>
                        {
                            f.AddMove(Epsilon, nfa1.Start);
                            f.AddMove(Epsilon, final);
                        });

                        nfa = new Nfa(start, final);
                        break;
                    }
                    case '+': // один или больше
                    {
                        var nfa1 = nfaStack.Pop();

                        var start = NewState();
                        var final = NewState();

                        start.AddMove(Epsilon, nfa1.Start);

                        nfa1.Final.ForEach(f =>
                        {
                            f.AddMove(Epsilon, nfa1.Start);
                            f.AddMove(Epsilon, final);
                        });

                        nfa = new Nfa(start, final);
                        break;
                    }
                    default: // символ
                    {
                        nfa = NewSimpleNfa(c);
                        break;
                    }
                }

                DisplayNfa(nfa, step++);
                nfaStack.Push(nfa);
            }

            if (nfaStack.Count == 1 && nfaStack.Peek().Final.Count == 1)
            {
                var nfa = nfaStack.Pop();
                nfa.Final[0].AddMove(Epsilon, State.Match);
                var start = State.Start;
                start.AddMove(Epsilon, nfa.Start);
                return start;
            }

            return null;
        }

        public static void DisplayNfa(Nfa nfa, int number)
        {
            Console.WriteLine($"Step {number}. NFA = {nfa}");
            Console.WriteLine("–––––––––––––––––––––––––––––––––––––––––––––––––––");
            DisplayState(nfa.Start);
            Console.WriteLine();
        }

        public static List<State> DisplayState(State state,
            List<int> passedStatesId = null, string indent = Indent)
        {
            if (state.Moves.Count == 0) return new List<State> {state};

            if (passedStatesId == null) passedStatesId = new List<int>();
            passedStatesId.Add(state.Id);

            var final = new List<State>();
            foreach (var move in state.Moves)
            {
                Console.WriteLine(
                    $"{indent}{state} ––{move.Key}––> {move.Value}");
                if (!passedStatesId.Contains(move.Value.Id))
                {
                    final.AddRange(DisplayState(move.Value, passedStatesId,
                        indent + Indent));
                }
            }

            return final;
        }
    }
}
