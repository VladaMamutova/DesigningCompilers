using OperatorPrecedenceParser.Model;

namespace OperatorPrecedenceParser.Exception
{
    class ParserException : System.Exception
    {
        public int Position { get; }
        public Token FoundToken { get; }
        public string Cause { get; }

        public override string Message =>
            $"Position: {Position}\n" + $"Error: {Cause}, found '{FoundToken}'";

        public ParserException(int position, Token foundToken, string message)
        {
            Position = position;
            FoundToken = foundToken;
            Cause = message;
        }
    }
}
