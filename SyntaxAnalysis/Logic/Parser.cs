using SyntaxAnalysis.Exception;
using SyntaxAnalysis.Model;

namespace SyntaxAnalysis.Logic
{
    // <блок> -> { <список операторов> }
    // <список операторов> -> <оператор> <хвост> | <оператор>
    // <оператор> -> <идентификатор> = <выражение>
    // <хвост> -> ; <оператор> <хвост> | ; <оператор>

    // <выражение> ->
    // <простое выражение> |
    // <простое выражение> <операция отношения> <простое выражение>

    // <простое выражение> ->
    // <терм> | <знак> <терм> |
    // <простое выражение> <операция типа сложения> <терм>

    // <терм> -> <фактор> | <терм> <операция типа умножения> <фактор>

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
                throw new ParserException(_lexer.Position, tokenType, _currentToken);
            }
        }
        
        // <блок> ->
        // { <список операторов> }
        private AstNode ParseBlock()
        {
            EatToken(TokenType.LeftBrace);
            var node = ParseOperatorList();
            EatToken(TokenType.RightBrace);

            return new UnaryNode("Block", Token.None, node); // для полного вывода
            //return node;
        }

        // <список операторов> ->
        // <оператор> <хвост> | <оператор>
        private AstNode ParseOperatorList()
        {
            var node = ParseOperator();

            var secondNode = TryParseTail();
            if (secondNode != null)
            {
                node = new BinaryNode("OperatorList", node, Token.None,
                    secondNode);
            }

            return node;
        }

        // <оператор> ->
        // <идентификатор> = <выражение>
        private AstNode ParseOperator()
        {
            var token = _currentToken;
            EatToken(TokenType.Identifier);
            var node = new ValueNode("Identifier", token);

            token = _currentToken;
            EatToken(TokenType.Equal);

            var secondNode = ParseExpression();

            return new BinaryNode("Operator", node, token, secondNode);
        }

        // null if can't parse tail
        private AstNode TryParseTail()
        {
            return _currentToken.Type == TokenType.SemiColon
                ? ParseTail()
                : null;
        }

        // <хвост> ->
        // ; <оператор> <хвост> | ; <оператор>
        private AstNode ParseTail()
        {
            EatToken(TokenType.SemiColon);

            var node = ParseOperator();
            node = new UnaryNode("Tail", Token.None, node);

            var secondNode = TryParseTail();
            if (secondNode != null)
            {
                node = new BinaryNode("Tail", node, Token.None, secondNode);
            }

            return node;
        }

        // <выражение> ->
        // <простое выражение> <операция отношения> <простое выражение> |
        // <простое выражение>
        private AstNode ParseExpression()
        {
            var node = ParseSimpleExpression(); // <простое выражение>

            if (TokenType.ComparisonOperator.HasFlag(_currentToken.Type)
            ) // <простое выражение> <операция отношения> <простое выражение>
            {
                var token = new Token(TokenType.ComparisonOperator,
                    _currentToken.Value);
                EatToken(TokenType.ComparisonOperator);
                var secondNode = ParseSimpleExpression();
                node = new BinaryNode("Expression", node, token, secondNode);
            }
            else
            {
                node = new UnaryNode("Expression", Token.None, node); // для полного вывода
            }

            return node;
        }

        // <простое выражение> ->
        // <знак> <терм> | <терм> |
        // <простое выражение> <операция типа сложения> <терм>
        private AstNode ParseSimpleExpression()
        {
            AstNode node;

            if (TokenType.Sign.HasFlag(_currentToken.Type)) // <знак> <терм>
            {
                var token = new Token(TokenType.Sign, _currentToken.Value);
                EatToken(TokenType.Sign);
                node = ParseTerm();
                node = new UnaryNode("SimpleExpression", token, node);
            }
            else
            {
                node = ParseTerm(); // <терм>
                node = new UnaryNode("SimpleExpression", Token.None, node);  // для полного вывода
            }

            // node = <терм> | <знак> <терм> = <простое выражение>

            if (TokenType.AdditiveOperator.HasFlag(_currentToken.Type)
            ) // <простое выражение> <операция типа сложения> <терм>
            {
                var token = new Token(TokenType.AdditiveOperator, _currentToken.Value);
                EatToken(TokenType.AdditiveOperator);
                var secondNode = ParseTerm();
                node = new BinaryNode("SimpleExpression", node, token, 
                    secondNode);
            }

            return node;
        }

        // <терм> ->
        // <фактор> | <терм> <операция типа умножения> <фактор>
        private AstNode ParseTerm()
        {
            AstNode node = ParseFactor(); // <фактор> = <терм>

            // node = <фактор> = <терм>

            if (TokenType.MultiplicativeOperator.HasFlag(_currentToken.Type)
            ) // <терм> <операция типа умножения> <фактор>
            {
                var token = new Token(TokenType.MultiplicativeOperator, _currentToken.Value);
                EatToken(TokenType.MultiplicativeOperator);
                var secondNode = ParseFactor();
                node = new BinaryNode("Term", node, token, secondNode);
            }
            else
            {
                node = new UnaryNode("Term", Token.None, node); // для полного вывода
            }

            return node;
        }

        // <фактор> ->
        // <идентификатор> | <константа> | ( < простое выражение > ) | not<фактор>
        private AstNode ParseFactor()
        {
            AstNode node;

            var token = _currentToken;
            switch (token.Type)
            {
                case TokenType.Identifier:
                {
                    EatToken(TokenType.Identifier);
                    node = new ValueNode("Factor", token);
                    break;
                }
                case TokenType.Const:
                {
                    EatToken(TokenType.Const);
                    node = new ValueNode("Factor", token);
                    break;
                }
                case TokenType.LeftParenthesis:
                {
                    EatToken(TokenType.LeftParenthesis);
                    node = ParseSimpleExpression();
                    EatToken(TokenType.RightParenthesis);
                    node = new UnaryNode("Factor", Token.None, node);  // для полного вывода
                        break;
                }
                case TokenType.Not:
                {
                    EatToken(TokenType.Not);
                    node = ParseFactor();
                    node = new UnaryNode("Factor", token, node);
                    break;
                }
                default:
                {
                    throw new ParserException(_lexer.Position,
                        new[]
                        {
                            TokenType.Identifier, TokenType.Const,
                            TokenType.LeftParenthesis, TokenType.Not
                        },
                        token);
                }
            }

            return node;
        }

        public AstNode Parse()
        {
            return ParseBlock();
        }
    }
}
