using System;
using SyntaxAnalysis.SyntaxAnalyzer;

namespace SyntaxAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser("i>(-a*2+b)");
            var ast = parser.Parse();
            Console.WriteLine(ast);
            Console.ReadLine();
        }
    }
}
