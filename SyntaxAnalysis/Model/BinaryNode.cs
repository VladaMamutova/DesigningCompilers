using System;

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
        
        public override string Visit()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return $"{Left}{Token}{Right}";
        }
    }
}
