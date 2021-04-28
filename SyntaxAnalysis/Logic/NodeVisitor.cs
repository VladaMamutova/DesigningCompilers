using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.Logic
{
    interface INodeVisitor
    {
        object VisitValueNode(ValueNode node);
        object VisitUnaryNode(UnaryNode node);
        object VisitBinaryNode(BinaryNode node);
    }
}
