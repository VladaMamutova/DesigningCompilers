using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GrammarTransformations.Logic
{
    class Grammar
    {
        public const char Epsilon = 'є';

        public readonly string[] Nonterms;
        public readonly string[] Terms;
        public readonly KeyValuePair<string, string[]>[] Rules;
        public readonly string Start;

        public Grammar(string[] nonterms, string[] terms,
            KeyValuePair<string, string[]>[] rules, string start)
        {
            Nonterms = nonterms;
            Terms = terms;
            Rules = rules;
            Start = start;
        }

        /// <summary>
        /// Считывает из файла грамматику и парсит её в соответствии со следующим форматом:
        /// <list type="table">
        /// <item>
        /// <term>1 строка</term>
        /// <description>нетерминалы (НТ; один и больше символов) через пробел</description>
        /// </item>
        /// <item>
        /// <term>2 строка</term>
        /// <description>терминалы (Т, один символ) через пробел</description>
        /// </item>
        /// <item>
        /// <term>3 строка</term>
        /// <description>начальный символ</description>
        /// </item>
        /// <item>
        /// <term>4..N строки</term>
        /// <description>правила в формате
        /// <code>&lt;нетерминал&gt; -> &lt;нетермилы_и_терминалы&gt;</code></description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="fileName">Путь к файлу с грамматикой.</param>
        /// <returns>Грамматика.</returns>
        public static Grammar Parse(string fileName)
        {
            string[] nonterms = null;
            string[] terms = null;
            List<KeyValuePair<string, string[]>> rules =
                new List<KeyValuePair<string, string[]>>();
            string start = null;

            var lines = File.ReadAllLines(fileName);
            try
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    try
                    {
                        switch (i)
                        {
                            case 0:
                            {
                                nonterms = lines[i].Split(new[] {' '},
                                    StringSplitOptions.RemoveEmptyEntries);
                                break;
                            }
                            case 1:
                            {
                                terms = lines[i].Split(new[] {' '},
                                    StringSplitOptions.RemoveEmptyEntries);
                                break;
                            }
                            case 2:
                            {
                                start = lines[i];
                                if (nonterms != null && !nonterms.Contains(start))
                                {
                                    throw new Exception($"Undefined nonterm: \'{start}\'");
                                }

                                break;
                            }

                            default:
                            {
                                var ruleParts = lines[i].Split(new[] {" -> "},
                                    StringSplitOptions.RemoveEmptyEntries);
                                var rightParts = ruleParts[1].Split(new[] {' '},
                                    StringSplitOptions.RemoveEmptyEntries);

                                var newRule =
                                    new KeyValuePair<string, string[]>(
                                        ruleParts[0], rightParts);
                                CheckRule(newRule, nonterms, terms);

                                rules.Add(newRule);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(
                            $"Error in line \'{i + 1}\': {ex.Message}");
                    }
                }

                if (nonterms == null || terms == null || rules.Count == 0 ||
                    start == null)
                {
                    throw new ArgumentException("Too little lines: " +
                                                $"found \'{lines.Length}\', " +
                                                "minimum \'4\' required");
                }
            }
            catch (Exception ex)
            {
                var format =
                    "  1st line - nonterminals (one or more characters) " +
                    "separated by a space\n" +
                    "  2nd line - terminals (one or more characters) " +
                    "separated by a space\n" +
                    "  3rd line - initial character\n" +
                    "  4..N lines - rules in the format: " +
                    "<nonterminal> -> <nonterminals_and_terminals>";

                throw new Exception("The grammar in the file is presented " +
                                    "in the wrong format.\n" + ex.Message +
                                    "\nRequired format:\n" + format);
            }

            return new Grammar(nonterms, terms, rules.ToArray(), start);
        }
    
        public void Print(bool alternativeRuleInLine = false)
        {
            string nonterms = string.Join(", ", Nonterms);
            string terms = string.Join(", ", Terms);
            List<string> rules = RulesToStrings(alternativeRuleInLine);

            int valuesWidth = Math.Max(nonterms.Length, terms.Length);
            valuesWidth = Math.Max(valuesWidth, rules.Max(rule => rule.Length));
            string splitter =
                $"+{new string('-', 4)}+" +
                $"{new string('-', valuesWidth + 2)}+";
            Console.WriteLine(splitter);

            Console.WriteLine($"| NT | {nonterms.PadRight(valuesWidth)} |");
            Console.WriteLine(splitter);

            Console.WriteLine($"| T  | {terms.PadRight(valuesWidth)} |");
            Console.WriteLine(splitter);

            for (int i = 0; i < rules.Count; i++)
            {
                Console.WriteLine(
                    $"| {(i == 0 ? "P " : "  ")} | {rules[i].PadRight(valuesWidth)} |");
            }

            Console.WriteLine(splitter);

            Console.WriteLine($"| S  | {Start.PadRight(valuesWidth)} |");
            Console.WriteLine(splitter);
        }

        public List<string> RulesToStrings(bool alternativeRuleInLine = false)
        {
            List<string> rules = new List<string>();
            if (alternativeRuleInLine)
            {
                var rulesDictionary = new Dictionary<string, List<string[]>>();
                foreach (var rule in Rules)
                {
                    if (!rulesDictionary.ContainsKey(rule.Key))
                    {
                        rulesDictionary.Add(rule.Key, new List<string[]>());
                    }

                    rulesDictionary[rule.Key].Add(rule.Value);
                }

                foreach (var rule in rulesDictionary)
                {
                    string[] rightParts = new string[rule.Value.Count];
                    for (var i = 0; i < rule.Value.Count; i++)
                    {
                        rightParts[i] = string.Join(" ", rule.Value[i]);
                    }

                    rules.Add($"{rule.Key} -> {string.Join(" | ", rightParts)}");
                }
            }
            else
            {
                rules.AddRange(Rules.Select(rule =>
                    $"{rule.Key} -> {string.Join(" ", rule.Value)}"));
            }

            return rules;
        }

        public static void CheckRule(KeyValuePair<string, string[]> rule, string[] nonterms, string[] terms)
        {
            if (!nonterms.Contains(rule.Key))
            {
                throw new Exception($"Undefined nonterm: \'{rule.Key}\'");
            }

            foreach (var variable in rule.Value)
            {
                if (!nonterms.Contains(variable) && !terms.Contains(variable) && variable != Epsilon.ToString())
                {
                    throw new Exception($"Undefined variable: \'{variable}\'");
                }
            }
        }
    }
}
