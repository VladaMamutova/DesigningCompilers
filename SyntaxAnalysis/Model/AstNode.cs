namespace SyntaxAnalysis.Model
{
    abstract class AstNode
    {
        public string Name { get; }

        protected AstNode(string name)
        {
            Name = name;
        }

        public abstract string Visit();

        public override string ToString()
        {
            return Name;
        }
    }
}
