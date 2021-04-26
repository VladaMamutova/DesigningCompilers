using System;
using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.SyntaxAnalyzer
{
    // <выражение> ->
    // <простое выражение> |
    // <простое выражение> <операция отношения> <простое выражение>

    // <простое выражение> ->
    // <терм> | <знак> <терм> |
    // <простое выражение> <операция типа сложения> <терм>

    // <терм> ->
    // <фактор> | <терм> <операция типа умножения> <фактор>

    // <фактор> -> <идентификатор> | <константа> |
    // ( < простое выражение > ) | not<фактор>

    // <операция отношения> -> = | <> | < | <= | > | >=

    // <знак> -> + | -

    // <операция типа сложения> -> + | - | or
    
    // <операция типа умножения> -> * | / | div | mod | and

    class Parser
    {
        private readonly Lexer _lexer;
        private Token _currentToken;

        public Parser(string input)
        {
            _lexer = new Lexer(input);
            _currentToken = _lexer.GetNextToken();
        }

        private void EatToken(TokenType tokenType)
        {
            if (tokenType.HasFlag(_currentToken.Type))
            {
                _currentToken = _lexer.GetNextToken();
            }
            else
            {
                RaiseException(tokenType, _currentToken);
            }
        }

        private void RaiseException(TokenType expected, Token foundToken)
        {
            throw new Exception($" Position: {_lexer.Position}\n" +
                                $"Error: expected '{expected}', found '{foundToken}'");
        }

        // <выражение> ->
        // <простое выражение> |
        // <простое выражение> <операция отношения> <простое выражение>
        private AstNode ParseExpression()
        {
            var node = ParseSimpleExpression(); // <простое выражение>

            if (node != null) // <простое выражение> <операция отношения> <простое выражение>
            {
                if (TokenType.ComparisonOperator.HasFlag(_currentToken.Type))
                {
                    var token = _currentToken;
                    EatToken(TokenType.ComparisonOperator);
                    var secondNode = ParseSimpleExpression();
                    node = new BinaryNode("Expression", node, token,
                        secondNode);
                }
            }

            return node;
        }

        // <простое выражение> ->
        // <терм> | <знак> <терм> |
        // <простое выражение> <операция типа сложения> <терм>
        private AstNode ParseSimpleExpression()
        {
            AstNode node;

            if (TokenType.Sign.HasFlag(_currentToken.Type)) // <знак> <терм>
            {
                var token = _currentToken;
                EatToken(TokenType.Sign);
                node = ParseTerm();
                node = new UnaryNode("SimpleExpression", token, node);
            }
            else
            {
                node = ParseTerm(); // <терм>
            }

            if (node != null) // node = <терм> | <знак> <терм> = <простое выражение>
            {
                // <простое выражение> <операция типа сложения> <терм>
                if (TokenType.AdditiveOperator.HasFlag(_currentToken.Type))
                {
                    var token = _currentToken;
                    EatToken(TokenType.AdditiveOperator);
                    var secondNode = ParseTerm();
                    node = new BinaryNode("SimpleExpression", node, token,
                        secondNode);
                }
            }

            return node;
        }

        // <терм> ->
        // <фактор> | <терм> <операция типа умножения> <фактор>
        private AstNode ParseTerm()
        {
            AstNode node = ParseFactor(); // <фактор>

            if (node != null) // node = <фактор> = <терм>
            {
                // <терм> <операция типа умножения> <фактор>
                if (TokenType.MultiplicativeOperator.HasFlag(_currentToken.Type))
                {
                    var token = _currentToken;
                    EatToken(TokenType.MultiplicativeOperator);
                    var secondNode = ParseFactor();
                    node = new BinaryNode("Term", node, token, secondNode);
                }
            }

            return node;
        }

        // <фактор> -> <идентификатор> | <константа> |
        // ( < простое выражение > ) | not<фактор>
        private AstNode ParseFactor()
        {
            AstNode node;

            var token = _currentToken;
            switch (token.Type)
            {
                case TokenType.Identifier:
                {
                    EatToken(TokenType.Identifier);
                    return new ValueNode("Factor", token);
                }
                case TokenType.Const:
                {
                    EatToken(TokenType.Const);
                    return new ValueNode("Factor", token);
                    }
                case TokenType.LeftParenthesis:
                {
                    EatToken(TokenType.LeftParenthesis);
                    node = ParseSimpleExpression();
                    EatToken(TokenType.RightParenthesis);
                    return node;
                }
                case TokenType.Not:
                    EatToken(TokenType.Not);
                    node = ParseFactor();
                    return new UnaryNode("Factor", token, node);
            }

            return null;
        }

        public AstNode Parse()
        {
            return ParseExpression();
        }
    }
}
