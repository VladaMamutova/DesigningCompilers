namespace OperatorPrecedenceParser.Model
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

        public override bool Equals(object obj)
        {
            return obj is Token token && Type == token.Type;
        }

        public override int GetHashCode() => Type.ToString().GetHashCode();

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
