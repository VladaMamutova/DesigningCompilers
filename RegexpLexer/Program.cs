using System;
using RegexpLexer.Logic;

namespace RegexpLexer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Regexp Lexer\n");
            Console.WriteLine("Press");
            Console.WriteLine("- any key to continue");
            Console.WriteLine("- HOME to enter regular expression again");
            Console.WriteLine("- ESC to exit");
            ConsoleKey userChoice;
            do
            {
                Console.Write("\nEnter regular expression: ");
                var regexp = Console.ReadLine();
                do
                {
                    try
                    {
                        var postfix = RegexpHelper.InfixToPostfix(regexp);
                        var nfa = FsmEngine.PostfixToNfa(postfix);

                        Console.Write("Enter input expression: ");
                        var input = Console.ReadLine();

                        //Console.WriteLine();
                        //FsmEngine.DisplayFsm(nfa, "NFA");

                        var dfa = FsmEngine.NfaToDfa(nfa);
                        //FsmEngine.DisplayFsm(dfa, "DFA");

                        var minimizedDfa =
                            FsmEngine.MinimizeFsmByBrzozowski(dfa);
                        //FsmEngine.DisplayFsm(minimizedDfa, "Minimized DFA");

                       // Console.WriteLine("DFA Simulation");
                        var result =
                            FsmEngine.DfaSimulation(minimizedDfa, input);
                        Console.WriteLine("Result: " +
                                          (result ? "Match" : "Not Match"));

                        userChoice = Console.ReadKey().Key;
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        userChoice = ConsoleKey.Home;
                    }
                } while (userChoice != ConsoleKey.Escape &&
                         userChoice != ConsoleKey.Home);

            } while (userChoice != ConsoleKey.Escape);
        }
    }
}
