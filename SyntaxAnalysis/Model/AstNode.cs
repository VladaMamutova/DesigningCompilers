using SyntaxAnalysis.Logic;

namespace SyntaxAnalysis.Model
{
    abstract class AstNode
    {
        public string Name { get; }

        protected AstNode(string name)
        {
            Name = name;
        }

        public abstract object Accept(INodeVisitor visitor);

        public override string ToString()
        {
            return Name;
        }
    }
}
