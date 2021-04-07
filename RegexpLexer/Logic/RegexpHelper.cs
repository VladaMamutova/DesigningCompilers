using System.Collections.Generic;
using System.Linq;

namespace RegexpLexer.Logic
{
    public static class RegexpHelper
    {
        public const char Concat = '.';
        public static readonly Dictionary<char, int> Operators = new Dictionary<char, int>
        {
            {'(', 4},
            {')', 4},
            {'*', 3},
            {'+', 3},
            {Concat, 2},
            {'|', 1}
        };

        public static string AddConcatOperators(string infix)
        {
            var infixStack = new Stack<char>();
            foreach (var c in infix)
            {
                if (infixStack.Count > 0)
                {
                    var prev = infixStack.Peek();
                    if (c != '+' && c != '*' &&
                        c != '|' && prev != '|' &&
                        c != ')' && prev != '(')
                    {
                        infixStack.Push(Concat);
                    }
                }

                infixStack.Push(c);
            }

            return string.Concat(infixStack.Reverse());
        }

        /// <summary>
        /// Преобразует инфиксную форму регулярного выражения в эквивалентную
        /// форму постфиксную вида
        /// (https://ru.wikipedia.org/wiki/Обратная_польская_запись#Алгоритм,
        /// https://habr.com/ru/post/489744/)
        /// </summary>
        /// <param name="infix">Регулярное выражение в инфиксной нотации.</param>
        /// <returns>Регулярное выражение в постфиксной нотации.</returns>
        public static string InfixToPostfix(string infix)
        {
            var operators = new Stack<char>();
            var postfix = new Stack<char>();

            infix = AddConcatOperators(infix);

            foreach (var c in infix)
            {
                if (!Operators.ContainsKey(c)) // символ
                {
                    postfix.Push(c);
                }
                else // оператор
                {
                    char op;
                    switch (c)
                    {
                        case '(':
                        {
                            operators.Push('(');
                            break;
                        }
                        case ')':
                        {
                            op = operators.Pop();
                            while (op != '(')
                            {
                                postfix.Push(op);
                                op = operators.Pop();
                            }

                            break;
                        }
                        default: // *, +, .
                        {
                            while (operators.Count > 0)
                            {
                                op = operators.Pop();
                                if (op == '(' || Operators[c] > Operators[op])
                                {
                                    operators.Push(op);
                                    break;
                                }

                                postfix.Push(op);
                            }

                            operators.Push(c);
                            break;
                        }
                    }
                }
            }

            while (operators.Count > 0)
            {
                postfix.Push(operators.Pop());
            }

            return string.Concat(postfix.Reverse());
        }
    }
}
