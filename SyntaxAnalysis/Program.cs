using System;
using SyntaxAnalysis.SyntaxAnalyzer;

namespace SyntaxAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser("{c=i+8>(-a*2+b);b=5;c=c+b}");
            var ast = parser.Parse();
            Console.WriteLine(ast);
            Console.ReadLine();
        }
    }
}
