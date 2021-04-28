using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.Logic
{
    class NodeStringInterpreter : INodeVisitor
    {
        public const string DefaultIntent = "  ";
        public const string DefaultMarker = "*";

        public AstNode RootNode { get; }
        public string Intent { get; }

        public NodeStringInterpreter(AstNode rootNode, string intent = "")
        {
            RootNode = rootNode;
            Intent = intent;
        }

        public string Interpret()
        {
            return RootNode.Accept(this).ToString();
        }

        public object VisitValueNode(ValueNode node)
        {
            return $"{DefaultMarker} {node.Name}  {node}";
        }

        public object VisitUnaryNode(UnaryNode node)
        {
            var token = node.Token.Type == TokenType.None
                ? ""
                : $"{Intent}  {DefaultMarker} {node.Token.Type}  {node.Token}\n";
            return $"{DefaultMarker} {node.Name}  {node}\n" +
                   $"{Intent} \\\n" +
                   token +
                   $"{Intent}  " +
                   new NodeStringInterpreter(node.Node, Intent + DefaultIntent)
                       .Interpret();
        }

        public object VisitBinaryNode(BinaryNode node)
        {
            var token = node.Token.Type == TokenType.None
                ? ""
                : $"{Intent}  {DefaultMarker} {node.Token.Type}  {node.Token}\n";
            return
                $"{DefaultMarker} {node.Name}  {node}\n" +
                $"{Intent} \\\n" +
                $"{Intent}  " +
                new NodeStringInterpreter(node.Left, Intent + DefaultIntent + "|")
                    .Interpret() + "\n" +
               token +
                $"{Intent}  " +
                new NodeStringInterpreter(node.Right, Intent + DefaultIntent)
                    .Interpret();
        }
    }
}
