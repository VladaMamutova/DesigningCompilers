using System;
using System.Collections.Generic;
using System.Linq;
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

            if (nfa == null)
            {
                Console.WriteLine("Failed to construct NFA from a regular expression.");
                Console.WriteLine($"Regular expression \"{regexp}\" is incorrect.");
            }
            else
            {
                Console.WriteLine();
                Console.Write("Result NFA = ");
                FsmEngine.DisplayFsm(nfa);
            }

            var dfa = FsmEngine.NfaToDfa(nfa);
            Console.WriteLine();
            Console.Write("Result DFA = ");
            FsmEngine.DisplayFsm(dfa);

            Console.ReadLine();
        }
    }
}
