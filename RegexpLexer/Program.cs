using System;
using RegexpLexer.Logic;

namespace RegexpLexer
{
    class Program
    {
        static void Main(string[] args)
        {
            var regexp = "(a|b)*abb"; 
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);

            if (nfa != null)
            {
                Console.WriteLine();
                Console.Write("NFA = ");
                FsmEngine.DisplayFsm(nfa);


                var dfa = FsmEngine.NfaToDfa(nfa);
                Console.WriteLine();
                Console.Write("DFA = ");
                FsmEngine.DisplayFsm(dfa);

                var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
                Console.WriteLine();
                Console.Write("Minimized DFA = ");
                FsmEngine.DisplayFsm(minimizedDfa);

                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "abb"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "aabb"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "babb"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "aabbbabb"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "ababaabb"));

                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "bbb"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "b"));
                Console.WriteLine(FsmEngine.DfaSimulation(minimizedDfa, "abab"));
            }
            else
            {
                Console.WriteLine("Failed to construct NFA from a regular expression.");
                Console.WriteLine($"Regular expression \"{regexp}\" is incorrect.");
            }

            Console.ReadLine();
        }
    }
}
