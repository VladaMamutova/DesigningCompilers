namespace SyntaxAnalysis.Model
{
    class ValueNode : AstNode
    {
        public Token Token { get; }
        public string Value => Token.Value;

        public ValueNode(string name, Token token) : base(name)
        {
            Token = token;
        }

        public override string Visit()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
