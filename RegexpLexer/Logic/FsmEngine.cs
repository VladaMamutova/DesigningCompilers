﻿using System;
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

        private static State NewState(char c, HashSet<State> nextStates)
        {
            var start = NewState();
            nextStates.ToList().ForEach(state => start.AddMove(c, state));
            return start;
        }

        private static Fsm NewSimpleFsm(char c)
        {
            var start = NewState();
            var final = NewState();
            start.AddMove(c, final);
            return new Fsm(start, final);
        }

        /// <summary>
        /// Преобразует регулярное выражение, представленное в постфиксной
        /// нотации, в НКА по алгоритму Мак-Нотона–Ямады–Томпсона
        /// (Алгоритм 3.23 из книги Ахо, Ульмана "Компиляторы. Принципы,
        /// технологии и инструментарий").
        /// </summary>
        /// <param name="postfix">Регулярное выражение в постфиксной нотации.</param>
        /// <returns>Начальное состояние построенного НКА.</returns>
        public static Fsm PostfixToNfa(string postfix)
        {
            _stateId = 0;

            var nfaStack = new Stack<Fsm>();
            var step = 1;
            foreach (var c in postfix)
            {
                Fsm nfa;
                switch (c)
                {
                    case '.': // конкатенация
                    {
                        var nfa2 = nfaStack.Pop();
                        var nfa1 = nfaStack.Pop();

                        nfa1.Final.ForEach(f =>
                            f.AddAllMoves(nfa2.Start.Moves));

                        nfa = new Fsm(nfa1.Start, nfa2.Final);
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

                        nfa = new Fsm(start, final);
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

                        nfa = new Fsm(start, final);
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

                        nfa = new Fsm(start, final);
                        break;
                    }
                    default: // символ
                    {
                        nfa = NewSimpleFsm(c);
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
        public static Fsm NfaToDfa(Fsm nfa)
        {
            _stateId = 0;

            var stateSubsets = new List<State> { NewState(Epsilon, EpsilonClosure(nfa.Start)) };
            var dfaStates = new Dictionary<int, State>
                {{stateSubsets[0].Id, new State(stateSubsets[0].Id)}};
            var dfa = new Fsm(dfaStates[stateSubsets[0].Id]);

            var passedStates = new HashSet<int>();

            while (stateSubsets.Exists(
                state => !passedStates.Contains(state.Id)))
            {
                var state =
                    stateSubsets.Find(s => !passedStates.Contains(s.Id));
                passedStates.Add(state.Id);

                var stateSubset =
                    state.Moves.Select(move => move.Value).ToList();
                var alphabet = GetAlphabet(stateSubset);
                foreach (var c in alphabet)
                {
                    var closure =
                        EpsilonClosure(GetNextStatesByChar(stateSubset, c));
                    var dfaState = stateSubsets.FirstOrDefault(s =>
                        s.CompareNextStates(closure));
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
        public static Fsm MinimizeFsmByBrzozowski(Fsm fsm)
        {
            return DeterminizeFsm(ReverseFsm(DeterminizeFsm(ReverseFsm(fsm))));
        }

        public static Fsm MinimizeFsmByBrzozowski_1(Fsm fsm)
        {
            return NfaToDfa(ReverseFsm(NfaToDfa(ReverseFsm(fsm))));
        }

        // Обратный автомат для A - автомат, полученный из A сменой местами начальных
        // и конечных состояний и сменой направлений переходов.
        public static Fsm ReverseFsm(Fsm fsm)
        {
            var states = new Dictionary<int, State>();
            var stateQueue = new Queue<State>();

            var newStartId = fsm.Final.Count > 1
                ? fsm.Final.Select(state => state.Id).Max() + 1
                : fsm.Final[0].Id;

            var reversedFsm = new Fsm(new State(newStartId), new State(fsm.Start.Id));
            states.Add(reversedFsm.Start.Id, reversedFsm.Start);
            states.Add(reversedFsm.Final[0].Id, reversedFsm.Final[0]);
            stateQueue.Enqueue(fsm.Start);

            if (fsm.Final.Count > 1)
            {
                // Так как класс Fsm имеет одно начальное состояние, то при смене
                // мест начального и конечных состояний добавим новое начальное
                // состояние, которое будет иметь є-переходы со все конечные.
                fsm.Final.ForEach(state =>
                {
                    states.Add(state.Id, new State(state.Id));
                    stateQueue.Enqueue(state);
                    reversedFsm.Start.AddMove(Epsilon, states[state.Id]);
                });
            }

            while (stateQueue.Count > 0)
            {
                var state = stateQueue.Dequeue();
                foreach (var move in state.Moves)
                {
                    if (!states.ContainsKey(move.Value.Id))
                    {
                        states.Add(move.Value.Id, new State(move.Value.Id));
                        stateQueue.Enqueue(move.Value);
                    }
                    states[move.Value.Id].AddMove(move.Key, states[state.Id]);
                }
            }
            
            return reversedFsm;
        }

        public static Fsm DeterminizeFsm(Fsm fsm)
        {
            _stateId = 0;

            State start;
            if (fsm.Start.Moves.TrueForAll(move => move.Key == Epsilon))
            {
                start = NewState(Epsilon, EpsilonClosure(fsm.Start));
            }
            else
            {
                start = NewState(Epsilon, new HashSet<State> {fsm.Start});
            }

            var stateSubsets = new List<State> { start };
            var dfaStates = new Dictionary<int, State>
                {{start.Id, new State(start.Id)}};
            var dfa = new Fsm(dfaStates[start.Id]);

            var passedStates = new HashSet<int>();

            while (stateSubsets.Exists(
                state => !passedStates.Contains(state.Id)))
            {
                var state =
                    stateSubsets.Find(s => !passedStates.Contains(s.Id));
                passedStates.Add(state.Id);

                var stateSubset = state.Moves.Select(move => move.Value).ToList();
                var alphabet = GetAlphabet(stateSubset);
                foreach (var c in alphabet)
                {
                    var closure = GetNextStatesByChar(stateSubset, c).ToHashSet();
                    var dfaState =
                        stateSubsets.FirstOrDefault(s => s.CompareNextStates(closure));
                    if (dfaState == null)
                    {
                        dfaState = NewState(c, closure);
                        stateSubsets.Add(dfaState);
                        dfaStates.Add(dfaState.Id, new State(dfaState.Id));
                        fsm.Final.ForEach(finalState =>
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

        public static List<char> GetAlphabet(List<State> states)
        {
            var alphabet = new List<char>();
            states.ForEach(state => alphabet.AddRange(state.GetAlphabet()));
            alphabet = alphabet.Distinct().ToList();
            alphabet.Remove(Epsilon);
            return alphabet;
        }

        public static List<State> GetNextStatesByChar(List<State> states, char c)
        {
            var nextStates = new List<State>();
            states.ForEach(state => nextStates.AddRange(state.GetNextStatesByChar(c)));
            return nextStates;
        }

        public static void DisplayFsm(Fsm nfa)
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

        private static void DisplayNfaOnStep(Fsm nfa, int step)
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
