using System;
using System.Globalization;

namespace SyntaxAnalysis.Model
{
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

        public void Advance(int length)
        {
            Position += length;
            _currentChar = IsEnd ? EndOfInput : _input[Position];
        }

        private char GetNextChar()
        {
            return Position + 1 >= _input.Length ? EndOfInput : _input[Position + 1];
        }

        private bool MatchValue(string value)
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
                if (MatchValue(key))
                {
                    keyword = key;
                    return true;
                }
            }

            return false;
        }

        private bool TryMatchConstant(out string constant)
        {
            constant = "";

            var pos = Position;
            while (_input[pos] >= '0' && _input[pos] <= '9')
            {
                constant += _input[pos];
                pos++;
            }

            return !string.IsNullOrEmpty(constant);
        }

        private bool TryMatchIdentifier(out string identifier)
        {
            identifier = "";

            var pos = Position;
            while (_input[pos] >= 'a' && _input[pos] <= 'z' ||
                   _input[pos] >= 'A' && _input[pos] <= 'Z')
            {
                identifier += _input[pos];
                pos++;
            }

            return !string.IsNullOrEmpty(identifier);
        }

        public Token GetNextToken()
        {
            TokenType type;
            string value;

            type = MatchSingleCharacterToken(_currentChar);
            if (type != TokenType.None)
            {
                value = _currentChar.ToString();

                var nextChar = GetNextChar();
                var newType = MatchTwoCharactersToken(_currentChar, nextChar);
                if (newType != TokenType.None)
                {
                    type = newType;
                    value += nextChar;
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

            Advance(value.Length);
            return new Token(type, value);
        }
    }
}
