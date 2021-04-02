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
            var start = FsmUtils.PostfixToNfa(postfix);

            if (start == null)
            {
                Console.WriteLine("Failed to construct NFA from a regular expression.");
                Console.WriteLine($"Regular expression \"{regexp}\" is incorrect.");
            }
            else
            {
                Console.WriteLine("Result NFA");
                Console.WriteLine("–––––––––––––––––––––––––––––––––––––––––––––––––––");
                FsmUtils.DisplayState(start);
            }

            Console.ReadLine();
        }
    }
}
