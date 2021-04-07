using Xunit;

namespace RegexpLexer.Tests
{
    public class RegexpHelperTests
    {
        [Fact]
        public void AddConcatOperators_One_Letter()
        {
            // Arrange
            string regexp = "a";

            // Act
            string actual = Logic.RegexpHelper.AddConcatOperators(regexp);

            // Assert
            Assert.Equal(regexp, actual);
        }

        [Fact]
        public void AddConcatOperators_Or()
        {
            string regexp = "ab|b";
            string actual = Logic.RegexpHelper.AddConcatOperators(regexp);
            Assert.Equal("a.b|b", actual);
        }


        [Fact]
        public void AddConcatOperators_Parenthesis()
        {
            string regexp = "a(abc)a";
            string actual = Logic.RegexpHelper.AddConcatOperators(regexp);
            Assert.Equal("a.(a.b.c).a", actual);
        }
        [Fact]
        public void AddConcatOperators_Complex1()
        {
            string regexp = "(a|b)*abb";
            string actual = Logic.RegexpHelper.AddConcatOperators(regexp);
            Assert.Equal("(a|b)*.a.b.b", actual);
        }

        [Fact]
        public void AddConcatOperators_Complex2()
        {
            string regexp = "a(bb)+a";
            string actual = Logic.RegexpHelper.AddConcatOperators(regexp);
            Assert.Equal("a.(b.b)+.a", actual);
        }

        [Fact]
        public void InfixToPostfix_OneLetter()
        {
            // Arrange
            string regexp = "a";

            // Act
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);

            // Assert
            Assert.Equal(regexp, actual);
        }

        [Fact]
        public void InfixToPostfix_TwoLetters()
        {
            // Arrange
            string regexp = "ab";

            // Act
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);

            // Assert
            Assert.Equal("ab.", actual);
        }

        [Fact]
        public void InfixToPostfix_Brackets1()
        {
            string regexp = "(a|b)";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("ab|", actual);
        }

        [Fact]
        public void InfixToPostfix_Brackets2()
        {
            string regexp = "(a|b)aa";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("ab|a.a.", actual);
        }

        [Fact]
        public void InfixToPostfix_Brackets3()
        {
            string regexp = "a(a|b)a";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("aab|.a.", actual);
        }

        [Fact]
        public void InfixToPostfix_OneOrMore1()
        {
            string regexp = "a*";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("a*", actual);
        }

        [Fact]
        public void InfixToPostfix_OneOrMore2()
        {
            string regexp = "ba*";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("ba*.", actual);
        }

        [Fact]
        public void InfixToPostfix_OneOrMore3()
        {
            string regexp = "(ba)*";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("ba.*", actual);
        }

        [Fact]
        public void InfixToPostfix_OneOrMore4()
        {
            string regexp = "a(ba)*";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("aba.*.", actual);
        }

        [Fact]
        public void InfixToPostfix_Complex1()
        {
            string regexp = "(a|b)*abb";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("ab|*a.b.b.", actual);
        }

        [Fact]
        public void InfixToPostfix_Complex2()
        {
            string regexp = "a(bb)+a";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("abb.+.a.", actual);
        }
        
        [Fact]
        public void InfixToPostfix_Complex3()
        {
            string regexp = "(a+|ab*)b";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("a+ab*.|b.", actual);
        }

        [Fact]
        public void InfixToPostfix_Complex4()
        {
            string regexp = "b+(ab+a|c*)+b";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("b+ab+.a.c*|+.b.", actual);
        }

        [Fact]
        public void InfixToPostfix_Binary()
        {
            string regexp = "(0|(1(01*(00)*0)*1)*)*";
            string actual = Logic.RegexpHelper.InfixToPostfix(regexp);
            Assert.Equal("0101*.00.*.0.*.1.*|*", actual);
        }
    }
}
