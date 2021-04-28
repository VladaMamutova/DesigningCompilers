using System;
using SyntaxAnalysis.Logic;

namespace SyntaxAnalysis
{
    class Program
    {
        static void Main()
        {
            string input =
                "{cat =  880div max<>(-a*206+b); i=var *10-30;var=notb;g=1and8  }";
            Console.WriteLine($"Input: {input}\n");

            try
            {
                Parser parser = new Parser(input);
                var ast = parser.Parse();
                var stringInterpreter = new NodeStringInterpreter(ast);

                Console.WriteLine("Abstract Syntax Tree:\n");
                Console.WriteLine(stringInterpreter.Interpret());
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
