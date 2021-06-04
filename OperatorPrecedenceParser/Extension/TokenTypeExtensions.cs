using OperatorPrecedenceParser.Model;

namespace OperatorPrecedenceParser.Extension
{
    public static class TokenTypeExtensions
    {
        public static TokenType GetBaseTokenType(this TokenType token)
        {
            //if (TokenType.Sign.HasFlag(token))
            //{
            //    return TokenType.Sign;
            //}

            if (TokenType.ComparisonOperator.HasFlag(token))
            {
                return TokenType.ComparisonOperator;
            }

            if (TokenType.AdditiveOperator.HasFlag(token))
            {
                return TokenType.AdditiveOperator;
            }

            if (TokenType.MultiplicativeOperator.HasFlag(token))
            {
                return TokenType.MultiplicativeOperator;
            }

            return token;
        }
    }
}
