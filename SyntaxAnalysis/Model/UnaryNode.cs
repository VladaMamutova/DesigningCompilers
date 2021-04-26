namespace SyntaxAnalysis.Model
{
    class UnaryNode : AstNode
    {
        public Token Token { get; }
        public AstNode Node { get; }

        public UnaryNode(string name, Token token, AstNode node) : base(name)
        {
            Token = token;
            Node = node;
        }
        
        public override string Visit()
        {
            throw new System.NotImplementedException();
        }

        public override string ToString()
        {
            return $"{Token}{Node}";
        }
    }
}
