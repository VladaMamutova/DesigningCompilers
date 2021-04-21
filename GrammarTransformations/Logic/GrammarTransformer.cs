using System.Collections.Generic;
using System.Linq;
using GrammarTransformations.Extensions;

namespace GrammarTransformations.Logic
{
    class GrammarTransformer
    {
        private static readonly string[] EpsilonRule =
            {Grammar.Epsilon.ToString()};

        public readonly Grammar Grammar;

        public GrammarTransformer(Grammar grammar)
        {
            Grammar = grammar;
        }

        private HashSet<string> FindEpsilonRuleNonterms()
        {
            HashSet<string> result = new HashSet<string>();
            foreach (var rule in Grammar.Rules)
            {
                if (rule.Value.SequenceEqual(EpsilonRule))
                {
                    result.Add(rule.Key);
                }
            }

            return result;
        }

        private HashSet<string> FindEpsilonGeneratingNonterms(
            HashSet<string> eGeneratingNonterms)
        {
            HashSet<string> resultNonterms =
                new HashSet<string>(eGeneratingNonterms);
            bool modified = false;
            foreach (var rule in Grammar.Rules)
            {
                if (!resultNonterms.Contains(rule.Key))
                {
                    bool eGenerating = true;
                    for (var i = 0; i < rule.Value.Length && eGenerating; i++)
                    {
                        eGenerating =
                            eGeneratingNonterms.Contains(rule.Value[i]);
                    }

                    if (eGenerating)
                    {
                        resultNonterms.Add(rule.Key);
                        modified = true;
                    }
                }
            }

            return modified
                ? FindEpsilonGeneratingNonterms(resultNonterms)
                : eGeneratingNonterms;
        }


        private List<KeyValuePair<string, string[]>> GenerateRules(HashSet<string> eGeneratingNonterms)
        {
            var rules = new List<KeyValuePair<string, string[]>>();
            foreach (var rule in Grammar.Rules)
            {
                int nontermsCount =
                    rule.Value.Count(eGeneratingNonterms.Contains);
                var rightParts = new List<string[]>();
                var rightPart = new List<string>();
                for (var i = 0; i < rule.Value.Length; i++)
                {
                    if (eGeneratingNonterms.Contains(rule.Value[i]))
                    {
                        var rightPartEnd = rule.Value.SubArray(i);
                        var newRightPartEnds = GenerateCombinations(rightPartEnd,
                            eGeneratingNonterms, nontermsCount);
                        foreach (var partEnd in newRightPartEnds)
                        {
                            var newRightPart = rightPart.Concat(partEnd);
                            if (!rightParts.Exists(part =>
                                part.SequenceEqual(newRightPart)))
                            {
                                rightParts.Add(newRightPart.ToArray());
                            }
                        }
                    }
                    else
                    {
                        rightPart.Add(rule.Value[i]);
                    }
                }

                if (rightPart.Count > 0 && !rightPart.SequenceEqual(EpsilonRule))
                {
                    rightParts.Add(rightPart.ToArray());
                }

                foreach (var part in rightParts)
                {
                    rules.Add(new KeyValuePair<string, string[]>(rule.Key, part));
                }
            }

            return rules;
        }

        private List<string[]> GenerateCombinations(string[] chain,
            HashSet<string> symbols, int resultSymbolsCount)
        {
            var combinations = new List<string[]>();
            if (resultSymbolsCount < 1)
            {
                return combinations;
            }

            if (resultSymbolsCount == 1)
            {
                var combination = new List<string> {chain[0]};
                combination.AddRange(
                    GetSubChainWithoutSymbols(chain, symbols, 1));

                combinations.Add(combination.ToArray());
                return combinations;
            }

            var combinationStart =
                GetSubChainWithSymbols(chain, symbols, resultSymbolsCount - 1)
                    .ToList();
            for (var i = combinationStart.Count; i < chain.Length; i++)
            {
                if (symbols.Contains(chain[i]))
                {
                    var combination = new List<string>();
                    combination.AddRange(combinationStart);
                    combination.Add(chain[i]);
                    combination.AddRange(
                        GetSubChainWithoutSymbols(chain, symbols, i + 1));
                    combinations.Add(combination.ToArray());
                }
                else
                {
                    combinationStart.Add(chain[i]);
                }
            }

            combinations.AddRange(GenerateCombinations(chain, symbols, resultSymbolsCount - 1));

            return combinations;
        }

        private string[] GetSubChainWithSymbols(string[] chain, HashSet<string> symbols, int symbolsCount)
        {
            var subChain = new List<string>();
            var count = 0;
            for (var i = 0; i < chain.Length && count < symbolsCount; i++)
            {
                if (symbols.Contains(chain[i]))
                {
                    subChain.Add(chain[i]);
                    count++;
                }
                else
                {
                    subChain.Add(chain[i]);
                }
            }

            return subChain.ToArray();
        }

        private string[] GetSubChainWithoutSymbols(string[] chain,
            HashSet<string> symbols, int start)
        {
            var subChain = new List<string>();
            for (var i = start; i < chain.Length; i++)
            {
                if (!symbols.Contains(chain[i]))
                {
                    subChain.Add(chain[i]);
                }
            }

            return subChain.ToArray();
        }

        /// <summary>
        /// Преобразует КС-грамматику в эквивалентную ей грамматику без є-правил.
        /// Алгоритм 2.10 из книги Ахо, Ульмана "Теория синтаксического анализа,
        /// перевода и компиляции: В 2-х томах. Т.1.: Синтаксический анализ".
        /// </summary>
        /// <returns>Эквивалентная КС-грамматика без є-правил
        /// (может присутствовать правило S -> є, но в этом случае
        /// S не встречается в правых частях правил)</returns>
        public Grammar RemoveEpsilonRules()
        {
            HashSet<string> eRuleNonterms = FindEpsilonRuleNonterms();
            HashSet<string> eGeneratingNonterms = FindEpsilonGeneratingNonterms(eRuleNonterms);

            var newNonterms = new List<string>();
            var newRules = new List<KeyValuePair<string, string[]>>();
            var newStart = Grammar.Start;

            if (eGeneratingNonterms.Contains(Grammar.Start))
            {
                newStart += '\'';
                newNonterms.Add(newStart);
                newRules.AddRange(new[]
                {
                    new KeyValuePair<string, string[]>(newStart,
                        new[] {Grammar.Start}),
                    new KeyValuePair<string, string[]>(newStart, EpsilonRule)
                });
            }

            newNonterms.AddRange(Grammar.Nonterms);
            newRules.AddRange(GenerateRules(eGeneratingNonterms));
            return new Grammar(newNonterms.ToArray(), Grammar.Terms,
                newRules.ToArray(), newStart);
        }
    }
}
