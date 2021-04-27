using System;
using SyntaxAnalysis.Logic;

namespace SyntaxAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser parser = new Parser("{cat=78div878787<>(-a*2898+b);bj=50;c=notb}");
            var ast = parser.Parse();
            Console.WriteLine(ast);
            Console.ReadLine();
        }
    }
}
