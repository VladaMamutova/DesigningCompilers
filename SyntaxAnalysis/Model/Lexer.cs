namespace SyntaxAnalysis.Model
{
    class Lexer
    {
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
            Position++;
            _currentChar = IsEnd ? EndOfInput : _input[Position];
        }

        public Token GetNextToken()
        {
            // Запоминаем текущий символ и продвигаемся по входной цепочке.
            var c = _currentChar;
            Advance();
            switch (c)
            {
                case '+': return new Token(TokenType.Plus, c);
                case '-': return new Token(TokenType.Minus, c);
                case '*': return new Token(TokenType.Multiply, c);
                case '/': return new Token(TokenType.Divide, c);
                case '=': return new Token(TokenType.Equal, c);
                case '<': return new Token(TokenType.Less, c);
                case '>': return new Token(TokenType.Greater, c);
                case '(': return new Token(TokenType.LeftParenthesis, c);
                case ')': return new Token(TokenType.RightParenthesis, c);
                case '{': return new Token(TokenType.LeftBrace, c);
                case '}': return new Token(TokenType.RightBrace, c);
                case ';': return new Token(TokenType.SemiColon, c);
                case EndOfInput: return new Token(TokenType.EndOfInput, c);

                default: return new Token(TokenType.Identifier, c);
                    //throw new ArgumentOutOfRangeException(nameof(c), c,
                    //    "Undefined Token");
            }
        }
    }
}
