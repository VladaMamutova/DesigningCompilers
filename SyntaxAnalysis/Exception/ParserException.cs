using System.Linq;
using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.Exception
{
    class ParserException : System.Exception
    {
        public int Position { get; }
        public TokenType[] ExpectedTokens { get; }
        public Token FoundToken { get; }

        public ParserException(int position, TokenType[] expectedTokens,
            Token foundToken) : base(
            CreateDefaultMessage(position, expectedTokens, foundToken))
        {
            Position = position;
            ExpectedTokens = expectedTokens;
            FoundToken = foundToken;
        }

        public ParserException(int position, TokenType expectedToken,
            Token foundToken) : this(position, new[] {expectedToken},
            foundToken)
        {
        }

        private static string CreateDefaultMessage(int position,
            TokenType[] expectedTokens,
            Token foundToken)
        {
            var tokens = expectedTokens.Select(token => $"'{token}'");
            return $"Position: {position}\n" +
                   $"Error: expected {string.Join(", ", tokens)}, found '{foundToken}'";
        }
    }
}
