using System;
using System.Globalization;
using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.Logic
{
    // Lexical analyzer (also known as scanner or tokenizer)
    class Lexer
    {
        public static readonly string[] Keywords =
            {"mod", "div", "and", "or", "not"};

        public const char EndOfInput = '\n';

        public int Position { get; private set; }
        private readonly string _input;
        private char _currentChar;

        private bool IsEnd => Position >= _input.Length;

        public Lexer(string input)
        {
            Position = 0;
            _input = input;
            _currentChar = input?.Length > 0 ? _input[Position] : EndOfInput;
        }

        public void Advance()
        {
            Position ++;
            _currentChar = IsEnd ? EndOfInput : _input[Position];
        }

        private bool CompareFollowingValue(string value)
        {
            return Position + value.Length <= _input.Length &&
                   string.Equals(value,
                       _input.Substring(Position, value.Length),
                       StringComparison.CurrentCultureIgnoreCase);
        }

        private TokenType MatchSingleCharacterToken(char c)
        {
            switch (c)
            {
                case '+': return TokenType.Plus;
                case '-': return TokenType.Minus;
                case '*': return TokenType.Multiply;
                case '/': return TokenType.Divide;
                case '=': return TokenType.Equal;
                case '<': return TokenType.Less;
                case '>': return TokenType.Greater;
                case '(': return TokenType.LeftParenthesis;
                case ')': return TokenType.RightParenthesis;
                case '{': return TokenType.LeftBrace;
                case '}': return TokenType.RightBrace;
                case ';': return TokenType.SemiColon;
                case EndOfInput: return TokenType.EndOfInput;
                default: return TokenType.None;
            }
        }

        private TokenType MatchTwoCharactersToken(char firstCharacter,
            char secondCharacter)
        {
            switch (firstCharacter)
            {
                case '<':
                {
                    switch (secondCharacter)
                    {
                        case '>': return TokenType.NotEqual;
                        case '=': return TokenType.LessOrEqual;
                        default: return TokenType.None;
                    }
                }
                case '>':
                    switch (secondCharacter)
                    {
                        case '=': return TokenType.GreaterOrEqual;
                        default: return TokenType.None;
                    }
                default: return TokenType.None;
            }
        }

        private bool TryMatchKeyword(out string keyword)
        {
            keyword = "";
            foreach (var key in Keywords)
            {
                if (CompareFollowingValue(key))
                {
                    keyword = key;
                    foreach (var _ in key)
                    {
                        Advance();
                    }
                    return true;
                }
            }

            return false;
        }

        private bool TryMatchConstant(out int constant)
        {
            constant = 0;
            var startPosition = Position;

            while (_currentChar >= '0' && _currentChar <= '9')
            {
                constant *= 10;
                constant += _currentChar - '0';
                Advance();
            }

            return Position > startPosition;
        }

        private bool TryMatchIdentifier(out string identifier)
        {
            identifier = "";

            while (_currentChar >= 'a' && _currentChar <= 'z' ||
                   _currentChar >= 'A' && _currentChar <= 'Z')
            {
                identifier += _currentChar;
                Advance();
            }

            return !string.IsNullOrEmpty(identifier);
        }

        public Token GetNextToken()
        {
            object value;

            var type = MatchSingleCharacterToken(_currentChar);
            if (type != TokenType.None)
            {
                value = _currentChar;
                Advance();

                var newType = MatchTwoCharactersToken((char)value, _currentChar);
                if (newType != TokenType.None)
                {
                    type = newType;
                    value += _currentChar.ToString();
                    Advance();
                }
            }
            else
            {
                if (TryMatchKeyword(out var keyword))
                {
                    type = (TokenType) Enum.Parse(typeof(TokenType),
                        CultureInfo.CurrentCulture.TextInfo
                            .ToTitleCase(keyword));
                    value = keyword;
                }
                else if (TryMatchConstant(out var constant))
                {
                    type = TokenType.Const;
                    value = constant;
                }
                else if (TryMatchIdentifier(out var identifier))
                {
                    type = TokenType.Identifier;
                    value = identifier;
                }
                else
                {
                    throw new ArgumentException(
                        $"Undefined Token: {_currentChar}");
                }
            }

            return new Token(type, value);
        }
    }
}
