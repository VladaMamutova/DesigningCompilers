namespace SyntaxAnalysis.Model
{
    [System.Flags]
    public enum TokenType
    {
        None = 0,

        Plus = 1 << 1,
        Minus = 1 << 2,
        Multiply = 1 << 3,
        Divide = 1 << 4,

        Equal = 1 << 5,
        NotEqual = 1 << 6,
        Less = 1 << 7,
        LessOrEqual = 1 << 8,
        Greater = 1 << 9,
        GreaterOrEqual = 1 << 10,

        Mod = 1 << 11,
        Div = 1 << 12,
        And = 1 << 13,
        Or = 1 << 14,
        Not = 1 << 15,

        LeftParenthesis = 1 << 16,
        RightParenthesis = 1 << 17,
        LeftBrace = 1 << 18,
        RightBrace = 1 << 19,
        SemiColon = 1 << 20,

        Identifier = 1 << 21,
        Const = 1 << 22,

        EndOfInput = 1 << 23,

        ComparisonOperator = Equal | NotEqual | Less | LessOrEqual | Greater |
                               GreaterOrEqual,
        Sign = Plus | Minus,
        AdditiveOperator = Plus | Minus | Or,
        MultiplicativeOperator = Multiply | Divide | Div | Mod | And
    }
}
