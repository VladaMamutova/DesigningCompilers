using System;
using OperatorPrecedenceParser.Logic;

namespace OperatorPrecedenceParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Operator Precedence Parser");
            char userChoice;
            do
            {
                Console.Write("\nEnter infix expression: ");
                var input = Console.ReadLine();
                // string input =
                //    "{ a = aa + (6 div (1 + 2)); b = 9 == 0; c = 7 > (6 mod 1)}";

                try
                {
                    Parser parser = new Parser(input);
                    var result = parser.Parse();

                    while (result.Count > 0)
                    {
                        Console.Write(result.Dequeue().Value.ToString() + ' ');
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Console.Write("\n\nContinue? (y/n) ");
                userChoice = Console.ReadKey(intercept: true).KeyChar;
                Console.WriteLine();
            } while (userChoice != 'n' && userChoice != 'N');

            Console.ReadLine();
        }
    }
}
