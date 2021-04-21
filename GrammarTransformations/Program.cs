using System;
using GrammarTransformations.Logic;

namespace GrammarTransformations
{
    class Program
    {
        static void Main()
        {
            try
            {
                Grammar grammar = Grammar.Parse("grammar.txt");
                GrammarTransformer transformer =
                    new GrammarTransformer(grammar);
                var transformingGrammar = transformer.RemoveEpsilonRules();

                Console.WriteLine("G = < NT, T, P, S >");
                grammar.Print(true);
                Console.WriteLine(
                    "\nG without є-rules = G' = < NT', T, P', S' >");
                transformingGrammar.Print(true);

                grammar = Grammar.Parse("grammar with left recursion.txt");
                transformer = new GrammarTransformer(grammar);
                transformingGrammar = transformer.EliminateLeftRecursion();

                Console.WriteLine("\nG = < NT, T, P, S >");
                grammar.Print(true);
                Console.WriteLine(
                    "\nG without left recursion = G' = < NT', T, P', S >");
                transformingGrammar.Print(true);

                grammar = Grammar.Parse("grammar for left refactoring.txt");
                transformer = new GrammarTransformer(grammar);
                transformingGrammar = transformer.ApplyLeftFactoring();

                Console.WriteLine("\nG = < NT, T, P, S >");
                grammar.Print(true);
                Console.WriteLine(
                    "\nG after left refactoring = G' = < NT', T, P', S >");
                transformingGrammar.Print(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }
    }
}
