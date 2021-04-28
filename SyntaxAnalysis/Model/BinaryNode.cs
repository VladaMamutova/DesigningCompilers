using SyntaxAnalysis.Logic;

namespace SyntaxAnalysis.Model
{
    class BinaryNode : AstNode
    {
        public AstNode Left { get; }
        public AstNode Right { get; }

        public Token Token { get; }

        public BinaryNode(string name, AstNode left, Token op, AstNode right) : base(name)
        {
            Left = left;
            Token = op;
            Right = right;
        }
        
        public override object Accept(INodeVisitor visitor)
        {
            return visitor.VisitBinaryNode(this);
        }

        public override string ToString()
        {
            var token = Token.Type == TokenType.None ? " " : $" {Token.Value} ";
            return $"{Left}{token}{Right}";
        }
    }
}
