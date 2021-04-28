using SyntaxAnalysis.Logic;

namespace SyntaxAnalysis.Model
{
    class ValueNode : AstNode
    {
        public Token Token { get; }
        public object Value => Token.Value;

        public ValueNode(string name, Token token) : base(name)
        {
            Token = token;
        }

        public override object Accept(INodeVisitor visitor)
        {
            return visitor.VisitValueNode(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
