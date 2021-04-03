using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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

        private static State NewState(char c, HashSet<State> nextStates)
        {
            var start = NewState();
            nextStates.ToList().ForEach(state => start.AddMove(c, state));
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
        public static Nfa PostfixToNfa(string postfix)
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

                DisplayNfaOnStep(nfa, step++);
                nfaStack.Push(nfa);
            }

            return nfaStack.Count == 1 && nfaStack.Peek().Final.Count == 1
                ? nfaStack.Pop()
                : null;
        }

        /// <summary>
        /// Преобразование НКА в ДКА
        /// Алгоритм 3.20. Построение подмножества (subset construction) ДКА из НКА
        /// </summary>
        /// <param name="nfa"></param>
        /// <returns></returns>
        public static Nfa NfaToDfa(Nfa nfa)
        {
            _stateId = 0;
            
            var stateSubsets = new List<State> { NewState(Epsilon, EpsilonClosure(nfa.Start)) };
            var dfaStates = new Dictionary<int, State>
                {{stateSubsets[0].Id, new State(stateSubsets[0].Id)}};
            var dfa = new Nfa(dfaStates[stateSubsets[0].Id]);

            var passedStates = new HashSet<int>();

            while (stateSubsets.Exists(state => !passedStates.Contains(state.Id)))
            {
                var state = stateSubsets.Find(s => !passedStates.Contains(s.Id));
                passedStates.Add(state.Id);

                var chars = state.GetOutputAlphabet();
                chars.Remove(Epsilon);
                foreach (var c in chars)
                {
                    var closure = EpsilonClosure(state.FindOutStates(c));
                    var dfaState =
                        stateSubsets.FirstOrDefault(s => s.CheckOutStates(closure));
                    if (dfaState == null)
                    {
                        dfaState = NewState(c, closure);
                        stateSubsets.Add(dfaState);
                        dfaStates.Add(dfaState.Id, new State(dfaState.Id));
                        nfa.Final.ForEach(finalState =>
                        {
                            if (closure.Contains(finalState))
                            {
                                dfa.AddFinalState(dfaStates[dfaState.Id]);
                            }
                        });
                    }

                    dfaStates[state.Id].AddMove(c, dfaStates[dfaState.Id]);
                }
            }

            DisplayDfaTransitionTable(dfaStates.Select(state => state.Value), stateSubsets);

            return dfa;
        }

        public static HashSet<State> EpsilonClosure(State start)
        {
            return EpsilonClosure(new List<State> {start});
        }

        // Множество состояний НКА, достижимых из состояния s из
        // множества T при одном є-переходе; = ∪s  T-closure(s)
        public static HashSet<State> EpsilonClosure(List<State> startStates)
        {
            var states = new Stack<State>();
            startStates.ForEach(state => states.Push(state));

            var closure = states.ToHashSet();
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

        // A — конечный автомат,
        // d(A) — детерминизированный автомат для A,
        // r(A) — обратный автомат для A,
        // dr(A) — результат d(r(A)). Аналогично для rdr(A) и drdr(A).
        // По алгоритму Бржовского, мининимальный детерминированный автомат = drdr(A)
        public static State MinimizeDfaByBrzozowski(State state)
        {
            return null;
        }

        public static State ReverseFsm(State state)
        {
            return null;
        }
        
        public static void DisplayFsm(Nfa nfa)
        {
            Console.WriteLine(nfa);
            Console.WriteLine("–––––––––––––––––––––––––––––––––––––––––––––––––––");
            DisplayState(nfa.Start);
        }
        
        private static List<State> DisplayState(State state,
            List<int> passedStatesId = null, string indent = Indent)
        {
            if (state.Moves.Count == 0) return new List<State> {state};

            if (passedStatesId == null) passedStatesId = new List<int>();
            passedStatesId.Add(state.Id);

            var nextStates = new List<State>();
            foreach (var move in state.Moves)
            {
                DisplayMove(state.ToString(), move.Key, move.Value.ToString(), indent);
                if (!passedStatesId.Contains(move.Value.Id))
                {
                    nextStates.AddRange(DisplayState(move.Value, passedStatesId,
                        indent + Indent));
                }
            }

            return nextStates;
        }

        private static void DisplayMove(string from, char c, string to, string indent = Indent)
        {
            Console.WriteLine($"{indent}{from} ––{c}––> {to}");
        }

        private static void DisplayNfaOnStep(Nfa nfa, int step)
        {
            Console.WriteLine();
            Console.Write($"Step {step}. NFA = ");
            DisplayFsm(nfa);
        }

        private static void DisplayDfaTransitionTable(
            IEnumerable<State> dfaStates, List<State> stateSubsets)
        {
            Console.WriteLine();
            Console.WriteLine("Transition Table: NFA -> DFA");
            Console.WriteLine(
                "–––––––––––––––––––––––––––––––––––––––––––––––––––");
            foreach (var dfaState in dfaStates)
            {
                foreach (var move in dfaState.Moves)
                {
                    var stateSubset =
                        stateSubsets.Find(state => state.Id == dfaState.Id);
                    DisplayMove(
                        $"{dfaState.ToString("S")} ({stateSubset.ShowOutStates()})",
                        move.Key, string.Join(", ", move.Value.ToString("S")));
                }
            }
        }
    }
}
