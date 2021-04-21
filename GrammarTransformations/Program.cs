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

                Console.WriteLine("G = < NT, T, P, S >");
                grammar.Print(true);

                GrammarTransformer transformer =
                    new GrammarTransformer(grammar);
                var transformingGrammar = transformer.RemoveEpsilonRules();

                Console.WriteLine(
                    "\nG without є-rules = G' = < NT', T, P', S' >");
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
