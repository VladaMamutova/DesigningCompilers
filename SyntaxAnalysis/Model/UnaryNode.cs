using SyntaxAnalysis.Logic;

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

        public override object Accept(INodeVisitor visitor)
        {
            return visitor.VisitUnaryNode(this);
        }
        
        public override string ToString()
        {
            string nodeInfo;
            if (Name == "Block")
            {
                nodeInfo = $"{{ {Node} }}";
            }
            else if (Name == "Tail")
            {
                nodeInfo = $"; {Node}";
            }
            else
            {
                nodeInfo = Node.ToString();
            }
            var token = Token.Type == TokenType.None ? "" : $"{Token.Value} ";
            return $"{token}{nodeInfo}";
        }
    }
}
