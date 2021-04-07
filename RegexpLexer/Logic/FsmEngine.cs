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

        private static List<char> GetAlphabet(List<State> states)
        {
            var alphabet = new List<char>();
            states.ForEach(state => alphabet.AddRange(state.GetAlphabet()));
            alphabet = alphabet.Distinct().ToList();
            alphabet.Remove(Epsilon);
            return alphabet;
        }

        private static List<State> GetNextStatesByChar(List<State> states,
            char c)
        {
            var nextStates = new List<State>();
            states.ForEach(state =>
                nextStates.AddRange(state.GetNextStatesByChar(c)));
            return nextStates;
        }

        /// <summary>
        /// Вычисляет є-замыкание, возвращая множество состоний НКА,
        /// достижимых из каждого состония множества startStates
        /// при одном є-переходе.
        /// </summary>
        /// <param name="startStates">Множества состояний.</param>
        /// <returns>Множество состоний при є-замыкании.</returns>
        private static HashSet<State> EpsilonClosure(List<State> startStates)
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

        private static HashSet<State> EpsilonClosure(State start)
        {
            return EpsilonClosure(new List<State> {start});
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
            //var step = 1;
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

                //DisplayNfaOnStep(nfa, step++);
                nfaStack.Push(nfa);
            }

            if (nfaStack.Count == 1)
            {
                return nfaStack.Pop();
            }

            throw new Exception(
                "Failed to construct NFA from a regular expression.");
        }

        /// <summary>
        /// Преобразование НКА в ДКА по алгоритму построения подмножеств
        /// (subset construction) ДКА из НКА
        /// (алгоритм 3.20. Ахо А.В, Лам М.С., Сети Р., Ульман Дж.Д.
        /// Компиляторы: принципы, технологии и инструменты)
        /// </summary>
        /// <param name="nfa">Недетерминированный конечный автомат.</param>
        /// <returns>Детерминированный конечный автомат.</returns>
        public static Fsm NfaToDfa(Fsm nfa)
        {
            _stateId = 0;

            var stateSubsets = new List<State>
                {NewState(Epsilon, EpsilonClosure(nfa.Start))};
            var dfaStates = new Dictionary<int, State>
                {{stateSubsets[0].Id, new State(stateSubsets[0].Id)}};
            var dfa = new Fsm(dfaStates[stateSubsets[0].Id]);

            var passedStates = new HashSet<int>();

            while (stateSubsets.Exists(
                state => !passedStates.Contains(state.Id)))
            {
                var state =
                    stateSubsets.Find(s => !passedStates.Contains(s.Id));
                // Если в подмножестве состояний (замыкании) было одно из
                // конечных состояний НКА, то новое состояние ДКА, соответствующее
                // этому подмножеству, также будет конечным.
                if (!dfa.Final.Contains(dfaStates[state.Id]))
                {
                    var closure = state.Moves.Select(m => m.Value).ToList();
                    nfa.Final.ForEach(finalState =>
                    {
                        if (closure.Contains(finalState))
                        {
                            dfa.AddFinalState(dfaStates[state.Id]);
                        }
                    });
                }

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
                    }

                    dfaStates[state.Id].AddMove(c, dfaStates[dfaState.Id]);
                }
            }

            //DisplayDfaTransitionTable(dfaStates.Select(state => state.Value), stateSubsets);
            //DisplayFsm(dfa, "DFA");
            return dfa;
        }

        /// <summary>
        /// Возвращает обратный автомат для A, который получается из A
        /// сменой местами начальных и конечных состояний
        /// и сменой направлений переходов.
        /// </summary>
        /// <param name="fsm">Конечный автомат.</param>
        /// <returns>Обратный автомат.</returns>
        public static Fsm ReverseFsm(Fsm fsm)
        {
            // Словарь состояний обратного автомата.
            var states = new Dictionary<int, State>();
            // Очередь состояний исходного автомата.
            var stateQueue = new Queue<State>();
            stateQueue.Enqueue(fsm.Start);

            // Так как класс Fsm имеет одно начальное состояние, то при смене
            // мест начального и конечных состояний добавим новое начальное
            // состояние, которое будет иметь є-переходы во все конечные.
            int newStartId = fsm.Final.Count > 1
                ? fsm.GetAllStates().Max(state => state.Id) + 1
                : fsm.Final[0].Id;
            var reversedFsm =
                new Fsm(new State(newStartId), new State(fsm.Start.Id));
            states.Add(reversedFsm.Start.Id, reversedFsm.Start);

            // Начальное состояние могло быть одновременно и конечным, поэтому
            // проверяем, что оно ещё не было добавлено в словарь
            // состояний обратного автомата.
            if (reversedFsm.Final[0].Id != reversedFsm.Start.Id)
            {
                states.Add(reversedFsm.Final[0].Id, reversedFsm.Final[0]);
            }

            fsm.Final.ForEach(final =>
            {
                // Добавляем предыдущие конечные состояния в словарь состояний
                // обратного автомата и є-переходы от начального ко всем конечным.
                if (fsm.Final.Count > 1)
                {
                    if (!states.ContainsKey(final.Id))
                    {
                        states.Add(final.Id, new State(final.Id));
                    }

                    states[newStartId].AddMove(Epsilon, states[final.Id]);
                }

                if (fsm.Start.Id != final.Id)
                {
                    stateQueue.Enqueue(final);
                }
            });


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

            //DisplayFsm(reversedFsm, "Reversed FSM");

            return reversedFsm;
        }

        /// <summary>
        /// Минимизирует автомат по алгоритму Бржовского:
        /// минимальный детерминированный автомат для A = drdr(A),
        /// где
        /// d(A) — детерминизированный автомат для A,
        /// r(A) — обратный автомат для A,
        /// dr(A) — результат d(r(A)). Аналогично для rdr(A) и drdr(A).
        /// (http://neerc.ifmo.ru/wiki/index.php?title=Алгоритм_Бржозовского)
        /// </summary>
        /// <param name="fsm">Конечный автомат.</param>
        /// <returns>Мнимальный конечный автомат.</returns>
        public static Fsm MinimizeFsmByBrzozowski(Fsm fsm)
        {
            // var n = ReverseFsm(fsm);
            //DisplayFsm();
            return NfaToDfa(ReverseFsm(NfaToDfa(ReverseFsm(fsm))));
        }

        /// <summary>
        /// Моделирует ДКА для заданной входной строки и возвращает
        /// true, если ДКА принимает её принимает, false - в противном случае.
        /// </summary>
        /// <param name="dfa"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool DfaSimulation(Fsm dfa, string input)
        {
            var state = dfa.Start;
            //Console.Write(state);
            foreach (var c in input)
            {
                try
                {
                    state = state.GetNextStateByChar(c);
                    //Console.Write($" -{c}-> {state}");
                }
                catch
                {
                    //throw new Exception(
                    //    $"Undefined char '{c}' in state {state}");
                    return false;
                }
            }

            //Console.WriteLine($"\n{state} is " +
            //                  (dfa.Final.Contains(state) ? "" : "not ") +
            //                  "in " + dfa.ShowFinalStates());
            return dfa.Final.Contains(state);
        }


        public static void DisplayFsm(Fsm fsm, string fsmName = "FSM")
        {
            Console.WriteLine($"{fsmName} = {fsm}");
            Console.WriteLine(
                "–––––––––––––––––––––––––––––––––––––––––––––––––––");
            DisplayState(fsm.Start);
            Console.WriteLine();
        }

        public static List<State> DisplayState(State state,
            List<int> passedStatesId = null, string indent = Indent)
        {
            if (state.Moves.Count == 0) return new List<State> {state};

            if (passedStatesId == null) passedStatesId = new List<int>();
            passedStatesId.Add(state.Id);

            var nextStates = new List<State>();
            foreach (var move in state.Moves)
            {
                DisplayMove(state.ToString(), move.Key, move.Value.ToString(),
                    indent);
                if (!passedStatesId.Contains(move.Value.Id))
                {
                    nextStates.AddRange(DisplayState(move.Value, passedStatesId,
                        indent + Indent));
                }
            }

            return nextStates;
        }

        public static void DisplayMove(string from, char c, string to,
            string indent = Indent)
        {
            Console.WriteLine($"{indent}{from} ––{c}––> {to}");
        }

        public static void DisplayNfaOnStep(Fsm nfa, int step)
        {
            Console.WriteLine();
            Console.Write($"Step {step}. NFA = ");
            DisplayFsm(nfa);
        }

        public static void DisplayDfaTransitionTable(
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
