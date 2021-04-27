namespace SyntaxAnalysis.Model
{
    class Token
    {
        public static readonly Token None = new Token(TokenType.None, "");

        public TokenType Type { get; }
        public string Value { get; }

        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Token(TokenType type, char value) : this(type, value.ToString()) { }

        public override string ToString()
        {
            return Value;
        }
    }
}
