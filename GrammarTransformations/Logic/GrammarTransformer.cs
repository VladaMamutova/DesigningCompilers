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

        private KeyValuePair<string, string[]> CreateRule(string nonterm,
            IEnumerable<string> rightPart)
        {
            return new KeyValuePair<string, string[]>(nonterm,
                rightPart.ToArray());
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


        private List<KeyValuePair<string, string[]>> GenerateRules(
            HashSet<string> eGeneratingNonterms)
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
                        var newRightPartEnds =
                            rightPartEnd.GenerateCombinations(
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

                if (rightPart.Count > 0 &&
                    !rightPart.SequenceEqual(EpsilonRule))
                {
                    rightParts.Add(rightPart.ToArray());
                }

                foreach (var part in rightParts)
                {
                    rules.Add(CreateRule(rule.Key, part));
                }
            }

            return rules;
        }

        /// <summary>
        /// Устраняет непосредственную левую рекурсию для правил заданного нетерминала.
        /// </summary>
        /// <param name="nonterm">Нетерминал, порождающий левую рекурсию.</param>
        /// <param name="rightParts">Правые части правил для нетерминала.</param>
        /// <param name="newNonterm">Наименование нового нетерминала,
        /// который будет введён после устранения левой рекурсии.</param>
        /// <returns>Массив правил без левой рекурсии для заданного нетерминала
        /// и нового.</returns>
        private KeyValuePair<string, string[]>[]
            EliminateImmediateLeftRecursion(string nonterm,
                List<string[]> rightParts, string newNonterm)
        {
            var newRules = new List<KeyValuePair<string, string[]>>();

            var startsWithNontermIndices = new List<int>();
            var otherIndices = new List<int>();
            for (var i = 0; i < rightParts.Count; i++)
            {
                if (rightParts[i][0] == nonterm)
                {
                    startsWithNontermIndices.Add(i);
                }
                else
                {
                    otherIndices.Add(i);
                }
            }

            if (startsWithNontermIndices.Count == 0)
            {
                return newRules.ToArray();
            }

            foreach (var i in otherIndices)
            {
                var newRightPart = new List<string>();
                if (!rightParts[i].SequenceEqual(EpsilonRule))
                {
                    newRightPart.AddRange(rightParts[i]);
                }

                newRightPart.Add(newNonterm);
                newRules.Add(CreateRule(nonterm, newRightPart));
            }

            foreach (var i in startsWithNontermIndices)
            {
                var newRightPart =
                    rightParts[i].SubArray(1).Concat(new[] {newNonterm})
                        .ToArray();
                newRules.Add(CreateRule(newNonterm, newRightPart));
            }

            newRules.Add(CreateRule(newNonterm, EpsilonRule));

            return newRules.ToArray();
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
            HashSet<string> eGeneratingNonterms =
                FindEpsilonGeneratingNonterms(eRuleNonterms);

            var newNonterms = new List<string>();
            var newRules = new List<KeyValuePair<string, string[]>>();
            var newStart = Grammar.Start;

            if (eGeneratingNonterms.Contains(Grammar.Start))
            {
                newStart += '\'';
                newNonterms.Add(newStart);
                newRules.AddRange(new[]
                {
                    CreateRule(newStart, new[] {Grammar.Start}),
                    CreateRule(newStart, EpsilonRule)
                });
            }

            newNonterms.AddRange(Grammar.Nonterms);
            newRules.AddRange(GenerateRules(eGeneratingNonterms));
            return new Grammar(newNonterms.ToArray(), Grammar.Terms,
                newRules.ToArray(), newStart);
        }

        /// <summary>
        /// Устраняет левую рекурсию в КС-грамматике без циклов и є-продукций
        /// (алгоритм 4.11. Ахо А.В, Лам М.С., Сети Р., Ульман Дж.Д.
        /// Компиляторы: принципы, технологии и инструменты).
        /// </summary>
        /// <returns>Эквивалентная грамматика без левой рекурсии.</returns>
        public Grammar EliminateLeftRecursion()
        {
            var newNonterms = new List<string>(Grammar.Nonterms);
            var newRules =
                new List<KeyValuePair<string, string[]>>(Grammar.Rules);
            for (int i = 0; i < Grammar.Nonterms.Length; i++)
            {
                var nontermI = Grammar.Nonterms[i];
                var rightPartsI = newRules
                    .Where(rule => rule.Key == nontermI)
                    .Select(rule => rule.Value).ToList();
                for (int j = 0; j < i; j++)
                {
                    var nontermJ = Grammar.Nonterms[j];
                    var rightPartsIWithJ = rightPartsI
                        .Where(rightPartI => rightPartI[0] == nontermJ)
                        .ToList();

                    if (rightPartsIWithJ.Count > 0)
                    {
                        var rightPartsJ = newRules
                            .Where(rule => rule.Key == nontermJ)
                            .Select(rule => rule.Value).ToList();

                        rightPartsI.RemoveAll(rightPartI =>
                            rightPartI[0] == nontermJ);
                        foreach (var rightPartI in rightPartsIWithJ)
                        {
                            var rightPartIWithoutJ = rightPartI.SubArray(1);
                            foreach (var rightPartJ in rightPartsJ)
                            {
                                rightPartsI.Add(rightPartJ
                                    .Concat(rightPartIWithoutJ).ToArray());
                            }
                        }
                    }
                }

                var newNonterm = nontermI + '\'';
                var resultRules = EliminateImmediateLeftRecursion(nontermI,
                    rightPartsI, newNonterm);
                if (resultRules.Length > 0)
                {
                    newRules.RemoveAll(rule => rule.Key == nontermI);
                    newRules.AddRange(resultRules);
                    newNonterms.Add(newNonterm);
                }
            }

            return new Grammar(newNonterms.ToArray(), Grammar.Terms,
                newRules.ToArray(), Grammar.Start);
        }

        /// <summary>
        /// Получает эквивалентную левофакторизованную КС-грамматику
        /// (алгоритм 4.11. Ахо А.В, Лам М.С., Сети Р., Ульман Дж.Д.
        /// Компиляторы: принципы, технологии и инструменты).
        /// </summary>
        /// <returns>Эквивалентная левофакторизованная грамматика.</returns>
        public Grammar ApplyLeftFactoring()
        {
            var newNonterms = new List<string>(Grammar.Nonterms);
            var newRules =
                new List<KeyValuePair<string, string[]>>(Grammar.Rules);

            foreach (var nonterm in Grammar.Nonterms)
            {
                var rules = newRules
                    .Where(rule => rule.Key == nonterm)
                    .Select(rule => rule.Value).ToArray();

                var rulesAsStrings = rules.Select(rule => string.Join("", rule))
                    .ToArray();

                string prefix = rulesAsStrings.FindPrefix();
                if (prefix.Length > 0)
                {
                    string newNonterm = nonterm + '\'';
                    newNonterms.Add(newNonterm);
                    newRules.RemoveAll(rule => rule.Key == nonterm);

                    string[] prefixVariables = null;
                    for (int i = 0; i < rulesAsStrings.Length; i++)
                    {
                        if (rulesAsStrings[i].StartsWith(prefix))
                        {
                            if (prefixVariables == null)
                            {
                                prefixVariables = rules[i]
                                    .GetItemsByTotalLength(prefix.Length);
                                newRules.Add(CreateRule(nonterm,
                                    prefixVariables.Concat(new[]
                                        {newNonterm})));
                            }

                            var suffix =
                                rules[i].SubArray(prefixVariables.Length);
                            if (suffix.Length == 0)
                            {
                                suffix = suffix.Concat(EpsilonRule).ToArray();
                            }

                            newRules.Add(CreateRule(newNonterm, suffix));
                        }
                        else
                        {
                            newRules.Add(CreateRule(nonterm, rules[i]));
                        }
                    }
                }
            }

            return new Grammar(newNonterms.ToArray(), Grammar.Terms,
                newRules.ToArray(), Grammar.Start);
        }
    }
}
