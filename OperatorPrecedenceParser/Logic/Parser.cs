using System.Collections.Generic;
using OperatorPrecedenceParser.Exception;
using OperatorPrecedenceParser.Extension;
using OperatorPrecedenceParser.Model;
using static OperatorPrecedenceParser.Model.TokenType;

namespace OperatorPrecedenceParser.Logic
{
    class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        private readonly Stack<Token> _stack;
        private readonly Queue<Token> _result;
        private static readonly Dictionary<TokenType, Dictionary<TokenType, char>> Matrix;

        private readonly Dictionary<char, string> _errors =
            new Dictionary<char, string>
            {
                {'0', "expected Expression"},
                {'1', "expected Identifier"},
                {'2', "unbalanced ')'"},
                {'3', "expected Operator"},
                {'4', "expected ')'"},
                {'5', "expected '{'"},
                {'6', "expected '}'"},
                {'7', "expected EndOfInput"},
                {'8', "expected OperatorList"},
                {'9', "expected ';' or '}'"}
            };

        public static readonly Token End = new Token(EndOfInput, Lexer.EndOfInput);

        // <блок> -> { <список операторов> }
        // <список операторов> -> <оператор> <хвост> | <оператор>
        // <оператор> -> <идентификатор> = <выражение>
        // <хвост> -> ; <оператор> <хвост> | ; <оператор>

        // <выражение> ->
        // <простое выражение> |
        // <простое выражение> <операция отношения> <простое выражение>

        // <простое выражение> ->
        // <терм> | <знак> <терм> |
        // <простое выражение> <операция типа сложения> <терм>

        // <терм> -> <фактор> | <терм> <операция типа умножения> <фактор>

        // <фактор> -> <идентификатор> | <константа> |
        // ( <простое выражение> ) | not<фактор>

        // <операция отношения> -> = | <> | < | <= | > | >=
        // <знак> -> + | -
        // <операция типа сложения> -> + | - | or
        // <операция типа умножения> -> * | / | div | mod | and

        static Parser()
        {
            Matrix = new Dictionary<TokenType, Dictionary<TokenType, char>>();

            // Токены в матрице:
            // Identifier, Const, Not,
            // AdditiveOperator, MultiplicativeOperator,
            // ComparisonOperator, Assignment,
            // LeftParenthesis, RightParenthesis,
            // LeftBrace, RightBrace,
            // SemiColon, EndOfInput // добавить токены для унарного и бинарного минуса

            Matrix.Add(Identifier, new Dictionary<TokenType, char>
            {
                {Identifier, '3'}, {Const, '3'}, {Not, '>'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '>'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '3'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '6'}
            });

            Matrix.Add(Const, new Dictionary<TokenType, char>
            {
                {Identifier, '3'}, {Const, '3'}, {Not, '>'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '>'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '3'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '6'}
            });

            Matrix.Add(Not, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '>'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '<'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(AdditiveOperator, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '<'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(MultiplicativeOperator, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '>'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '<'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(ComparisonOperator, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '<'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '>'}, {Assignment, '>'},
                {LeftParenthesis, '<'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(Assignment, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '<'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '<'}, {Assignment, '>'},
                {LeftParenthesis, '<'}, {RightParenthesis, '>'},
                {LeftBrace, '>'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(LeftParenthesis, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '<'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '<'}, {Assignment, '<'},
                {LeftParenthesis, '<'}, {RightParenthesis, '='},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '4'}
            });

            Matrix.Add(RightParenthesis, new Dictionary<TokenType, char>
            {
                {Identifier, '3'}, {Const, '3'}, {Not, '>'},
                {AdditiveOperator, '>'}, {MultiplicativeOperator, '>'},
                {ComparisonOperator, '>'},  {Assignment, '>'},
                {LeftParenthesis, '3'}, {RightParenthesis, '>'},
                {LeftBrace, '5'}, {RightBrace, '>'},
                {SemiColon, '>'}, {EndOfInput, '>'}
            });

            Matrix.Add(LeftBrace, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '<'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '<'}, {Assignment, '<'},
                {LeftParenthesis, '<'}, {RightParenthesis, '2'},
                {LeftBrace, '8'}, {RightBrace, '='},
                {SemiColon, '3'}, {EndOfInput, '6'}
            });

            Matrix.Add(RightBrace, new Dictionary<TokenType, char>
            {
                {Identifier, '7'}, {Const, '7'}, {Not, '7'},
                {AdditiveOperator, '7'}, {MultiplicativeOperator, '7'},
                {ComparisonOperator, '7'}, {Assignment, '3'},
                {LeftParenthesis, '7'}, {RightParenthesis, '7'},
                {LeftBrace, '7'}, {RightBrace, '7'},
                {SemiColon, '7'}, {EndOfInput, '>'}
            });

            Matrix.Add(SemiColon, new Dictionary<TokenType, char>
            {
                {Identifier, '<'}, {Const, '<'}, {Not, '<'},
                {AdditiveOperator, '<'}, {MultiplicativeOperator, '<'},
                {ComparisonOperator, '<'}, {Assignment, '1'},
                {LeftParenthesis, '<'}, {RightParenthesis, '3'},
                {LeftBrace, '3'}, {RightBrace, '3'},
                {SemiColon, '='}, {EndOfInput, '3'}
            });

            Matrix.Add(EndOfInput, new Dictionary<TokenType, char>
            {
                {Identifier, '5'}, {Const, '5'}, {Not, '5'},
                {AdditiveOperator, '5'}, {MultiplicativeOperator, '5'},
                {ComparisonOperator, '5'},  {Assignment, '5'},
                {LeftParenthesis, '5'}, {RightParenthesis, '5'},
                {LeftBrace, '<'}, {RightBrace, '5'},
                {SemiColon, '5'}, {EndOfInput, '8'}
            });
        }

        public Parser(string input)
        {
            _lexer = new Lexer(input);
            _currentToken = _lexer.GetNextToken();

            _stack = new Stack<Token>();
            _result = new Queue<Token>();
        }

        private char GetMatrixOperator(Token token1, Token token2)
        {
            var type1 = token1.Type.GetBaseTokenType();
            var type2 = token2.Type.GetBaseTokenType();
            return Matrix[type1][type2];
        }

        private void Shift()
        {
            if (_currentToken.Type != SemiColon)
            {
                _stack.Push(_currentToken);
            }

            _currentToken = _lexer.GetNextToken();
        }

        private void Reduce(Token token)
        {
            if (token.Type != LeftParenthesis &&
                token.Type != RightParenthesis &&
                token.Type != LeftBrace &&
                token.Type != RightBrace)
            {
                _result.Enqueue(token);
            }
        }

        public Queue<Token> Parse()
        {
            _stack.Push(new Token(EndOfInput, Lexer.EndOfInput));

            while (_stack.Count > 1 || !_currentToken.Equals(End))
            {
                switch (GetMatrixOperator(_stack.Peek(), _currentToken))
                {
                    case '<':
                    case '=':
                    {
                        Shift();
                        break;
                    }
                    case '>':
                    {
                        Token token;
                        do
                        {
                            token = _stack.Pop();
                            Reduce(token);
                        } while (GetMatrixOperator(_stack.Peek(), token) != '<');

                        if (_stack.Peek().Type == LeftBrace && _currentToken.Type == SemiColon)
                        {
                            Shift();
                        }

                        break;
                    }
                    default:
                    {
                        throw new ParserException(_lexer.Position, _currentToken,
                            _errors[
                                GetMatrixOperator(_stack.Peek(),
                                    _currentToken)]);
                    }
                }
            }

            return _result;
        }
    }
}
