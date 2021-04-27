namespace SyntaxAnalysis.Model
{
    class Token
    {
        public static readonly Token None = new Token(TokenType.None, "");

        public TokenType Type { get; }
        public object Value { get; }

        public Token(TokenType type, object value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
