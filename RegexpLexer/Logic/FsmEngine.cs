using System;
using System.Collections.Generic;
using System.Linq;

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

        private static State NewState(char c, List<State> nextStates)
        {
            var start = NewState();
            nextStates.ForEach(state => start.AddMove(c, state));
            return start;
        }

        private static Nfa NewSimpleNfa(char c)
        {
            var start = NewState();
            var final = NewState();
            start.AddMove(c, final);
            return new Nfa(start, final);
        }

        /// <summary>
        /// Преобразует регулярное выражение, представленное в постфиксной
        /// нотации, в НКА по алгоритму Мак-Нотона–Ямады–Томпсона
        /// (Алгоритм 3.23 из книги Ахо, Ульмана "Компиляторы. Принципы,
        /// технологии и инструментарий").
        /// </summary>
        /// <param name="postfix">Регулярное выражение в постфиксной нотации.</param>
        /// <returns>Начальное состояние построенного НКА.</returns>
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

                // DisplayNfaOnStep(nfa, step++);
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

        /// <summary>
        /// Преобразование НКА в ДКА
        /// Алгоритм 3.20. Построение подмножества (subset construction) ДКА из НКА
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static State NfaToDfa(State start)
        {
            _stateId = 0;

            var moves = new Dictionary<KeyValuePair<int, char>, List<State>>();
            var dfaMoves = new Dictionary<KeyValuePair<int, char>, int>();

            var states = new List<State>
                {NewState(Epsilon, EpsilonClosure(start))};
            var passedStates = new List<int>();
            while (states.Exists(state => !passedStates.Contains(state.Id)))
            {
                var state = states.Find(s => !passedStates.Contains(s.Id));
                passedStates.Add(state.Id);
                var chars = state.GetOutputAlphabet();
                chars.Remove(Epsilon);
                foreach (var c in chars)
                {
                    var closure = EpsilonClosure(state.FindOutStates(c));
                    var dfaState =
                        states.FirstOrDefault(s => s.CheckOutStates(closure));
                    if (dfaState == null)
                    {
                        dfaState = NewState(c, closure);
                        states.Add(dfaState);
                    }

                    moves.Add(new KeyValuePair<int, char>(state.Id, c),
                        closure);
                    dfaMoves.Add(new KeyValuePair<int, char>(state.Id, c),
                        dfaState.Id);
                }
            }

            DisplayDfaTransitionTable(states, moves);

            return ConstructDfaByTransitionTable(states, dfaMoves);
        }

        private static State ConstructDfaByTransitionTable(List<State> states,
            Dictionary<KeyValuePair<int, char>, int> moves)
        {
            List<State> dfaStates =
                states.Select(state => new State(state.Id)).ToList();
            foreach (var dfaState in dfaStates)
            {
                var dfaMoves = moves.Where(move => move.Key.Key == dfaState.Id);
                foreach (var move in dfaMoves)
                {
                    dfaState.AddMove(move.Key.Value,
                        dfaStates.Find(s => s.Id == move.Value));
                }
            }

            return dfaStates[0];
        }

        public static List<State> EpsilonClosure(State start)
        {
            return EpsilonClosure(new List<State> {start});
        }

        // Множество состояний НКА, достижимых из состояния s из
        // множества T при одном є-переходе; = ∪s  T-closure(s)
        public static List<State> EpsilonClosure(List<State> startStates)
        {
            var states = new Stack<State>();
            startStates.ForEach(state => states.Push(state));

            var closure = states.ToList();
            while (states.Count > 0)
            {
                var state = states.Pop();
                state.Moves.ForEach(move =>
                {
                    if (move.Key == Epsilon && !closure.Contains(move.Value))
                    {
                        closure.Add(move.Value);
                        states.Push(move.Value);
                    }
                });
            }

            return closure;
        }

        private static void DisplayNfaOnStep(Nfa nfa, int step)
        {
            Console.WriteLine();
            Console.WriteLine($"Step {step}. NFA = {nfa}");
            Console.WriteLine(
                "–––––––––––––––––––––––––––––––––––––––––––––––––––");
            DisplayState(nfa.Start);
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

        private static void DisplayDfaTransitionTable(List<State> states,
            Dictionary<KeyValuePair<int, char>, List<State>> moves)
        {
            Console.WriteLine();
            Console.WriteLine("Transition Table: NFA -> DFA");
            Console.WriteLine("–––––––––––––––––––––––––––––––––––––––––––––––––––");
            foreach (var move in moves)
            {
                var dfaState = states.Find(state => state.Id == move.Key.Key);
                Console.WriteLine(
                    $"{Indent}S{move.Key.Key} ({dfaState.ShowOutStates()}) " +
                    $"--{move.Key.Value}--> " +
                    $"{{{string.Join(", ", move.Value)}}}");
            }
        }
    }
}
