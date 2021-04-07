using System.Collections.Generic;
using RegexpLexer.Logic;
using RegexpLexer.Utils;
using Xunit;

namespace RegexpLexer.Tests
{
    public class FsmEngineTests
    {
        public class FsmEngineTestData
        {
            public static Fsm InitFsmWithOneFinal()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var state2 = new State(2);
                var state3 = new State(3);
                var fsm = new Fsm(state0, state3);
                state0.AddMove('a', state1);
                state0.AddMove('a', state2);
                state0.AddMove('b', state2);
                state1.AddMove('a', state2);
                state1.AddMove('b', state3);
                state2.AddMove('a', state1);
                state2.AddMove('a', state2);
                state2.AddMove('b', state3);
                return fsm;
            }

            public static Fsm InitReversedFsmWithOneFinal()
            {
                var state3 = new State(3);
                var state1 = new State(1);
                var state2 = new State(2);
                var state0 = new State(0);
                var fsm = new Fsm(state3, state0);
                state3.AddMove('b', state1);
                state3.AddMove('b', state2);
                state1.AddMove('a', state0);
                state1.AddMove('a', state2);
                state2.AddMove('a', state1);
                state2.AddMove('a', state2);
                state2.AddMove('a', state0);
                state2.AddMove('b', state0);
                return fsm;
            }
            
            public static Fsm InitDeterministicFsmWithOneFinal()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var state2 = new State(2);
                var state3 = new State(3);
                var fsm = new Fsm(state0, state3);
                state0.AddMove('a', state1);
                state0.AddMove('b', state2);
                state1.AddMove('a', state1);
                state1.AddMove('b', state3);
                state2.AddMove('a', state1);
                state2.AddMove('b', state3);
                return fsm;
            }

            public static Fsm InitFsmWithTwoFinals()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var state2 = new State(2);
                var state3 = new State(3);
                var fsm = new Fsm(state0, new List<State> {state2, state3});
                state0.AddMove('b', state1);
                state1.AddMove('a', state2);
                state1.AddMove('b', state3);
                state2.AddMove('a', state2);
                state2.AddMove('b', state3);
                return fsm;
            }

            public static Fsm InitReversedFsmWithStartFromEpsilon()
            {
                var state4 = new State(4);
                var state2 = new State(2);
                var state3 = new State(3);
                var state1 = new State(1);
                var state0 = new State(0);
                var fsm = new Fsm(state4, state0);
                state4.AddMove(FsmEngine.Epsilon, state2);
                state4.AddMove(FsmEngine.Epsilon, state3);
                state2.AddMove('a', state2);
                state2.AddMove('a', state1);
                state3.AddMove('b', state1);
                state3.AddMove('b', state2);
                state1.AddMove('b', state0);
                return fsm;
            }

            public static Fsm InitFsmTwoWay()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var fsm = new Fsm(state0, state0);
                state0.AddMove('0', state0);
                state0.AddMove('1', state1);
                state1.AddMove('1', state0);
                return fsm;
            }

            public static Fsm InitDeterministicFsmWithStartFromEpsilon()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var state2 = new State(2);
                var fsm = new Fsm(state0, state2);
                state0.AddMove('a', state1);
                state0.AddMove('b', state1);
                state1.AddMove('a', state1);
                state1.AddMove('b', state2);
                return fsm;
            }
        }

        [Fact]
        public void ReverseFsm_OneFinal()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmWithOneFinal();
            var expected = FsmEngineTestData.InitReversedFsmWithOneFinal();

            // Act
            Fsm actual = FsmEngine.ReverseFsm(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Fact]
        public void ReverseFsm_TwoFinals()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmWithTwoFinals();
            var expected = FsmEngineTestData.InitReversedFsmWithStartFromEpsilon();

            // Act
            Fsm actual = FsmEngine.ReverseFsm(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Fact]
        public void ReverseFsm_TwoWay()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmTwoWay();
            var expected = FsmEngineTestData.InitFsmTwoWay();

            // Act
            Fsm actual = FsmEngine.ReverseFsm(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Fact]
        public void DeterminizeFsm_OneFinal()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmWithOneFinal();
            var expected = FsmEngineTestData.InitDeterministicFsmWithOneFinal();

            // Act
            Fsm actual = FsmEngine.NfaToDfa(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Fact]
        public void DeterminizeFsm_TwoFinals()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitReversedFsmWithStartFromEpsilon();
            var expected = FsmEngineTestData.InitDeterministicFsmWithStartFromEpsilon();

            // Act
            Fsm actual = FsmEngine.NfaToDfa(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Fact]
        public void MinimizeFsmByBrzozowski()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmWithOneFinal();
            var expected = FsmEngineTestData.InitDeterministicFsmWithStartFromEpsilon();

            // Act
            Fsm actual = FsmEngine.MinimizeFsmByBrzozowski(fsm);

            // Assert
            Assert.Equal(expected, actual, new FsmEqualityComparer());
        }

        [Theory]
        [InlineData("a*", "")]
        [InlineData("a*", "a")]
        [InlineData("a*", "aaaa")]
        [InlineData("(a)*", "aaaa")]

        [InlineData("ab*a", "aa")]
        [InlineData("ab*a", "aba")]
        [InlineData("ab*a", "abbba")]

        [InlineData("(abc)*(defg)*", "")]
        [InlineData("(abc)*(defg)*", "abc")]
        [InlineData("(abc)*(defg)*", "defg")]
        [InlineData("(abc)*(defg)*", "abcdefg")]
        [InlineData("(abc)*(defg)*", "abcdefgdefg")]
        public void CheckZeroOrMoreInRegularExpression_True(string regexp, string input)
        {
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, input);

            Assert.True(actual);
        }

        [Theory]
        [InlineData("a*", "b")]
        [InlineData("a*", "ab")]
        
        [InlineData("ab*a", "ababa")]
        [InlineData("ab*a", "abb")]

        [InlineData("(abc)*(defg)*", "defgg")]
        [InlineData("(abc)*(defg)*", "abcde")]
        public void CheckZeroOrMoreInRegularExpression_False(string regexp, string input)
        {
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, input);

            Assert.False(actual);
        }

        [Theory]
        [InlineData("a+", "a")]
        [InlineData("a+", "aaaa")]

        [InlineData("a+b+ab+", "abab")]
        [InlineData("a+b+ab+", "aababbbb")]
        [InlineData("a+b+ab+", "aaabbabbbb")]

        [InlineData("(abc)+", "abc")]
        [InlineData("(abc)+", "abcabcabc")]

        [InlineData("(aa)+", "aa")]
        [InlineData("(aa)+", "aaaa")]
        [InlineData("(aa)+(bb)+", "aaaabb")]
        public void CheckOneOrMoreInRegularExpression_True(string regexp, string input)
        {
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, input);

            Assert.True(actual);
        }

        [Theory]
        [InlineData("a+", "")]
        [InlineData("a+", "b")]

        [InlineData("a+b+ab+", "abb")]
        [InlineData("a+b+ab+", "ababab")]

        [InlineData("(aa)+", "aaa")]
        [InlineData("(aa)+", "aaaaa")]
        [InlineData("(aa)+(bb)+", "aaaab")]
        public void CheckOneOrMoreInRegularExpression_False(string regexp, string input)
        {
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, input);

            Assert.False(actual);
        }

        [Theory]
        [InlineData("abb")]
        [InlineData("aabb")]
        [InlineData("babb")]
        [InlineData("aaaaaabb")]
        [InlineData("abbaaababb")]
        [InlineData("aabbabb")]
        public void CheckInputByRegularExpression1_True(string expression)
        {
            // Arrange
            var regexp = "(a|b)*abb";

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("bb")]
        [InlineData("ab")]
        [InlineData("aabbb")]
        [InlineData("aabbaabba")]
        [InlineData("aabbbb")]
        public void CheckInputByRegularExpression1_False(string expression)
        {
            // Arrange
            var regexp = "(a|b)*abb";

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [InlineData("abb")]
        [InlineData("aadd")]
        [InlineData("abbaddbb")]
        [InlineData("abbaccdddbbbb")]
        [InlineData("aacddaccdddb")]
        [InlineData("abbacddbb")]
        public void CheckInputByRegularExpression2_True(string expression)
        {
            // Arrange
            var regexp = "a(bb|ac*dd+)+b*";

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("ab")]
        [InlineData("aad")]
        [InlineData("aacdb")]
        public void CheckInputByRegularExpression2_False(string expression)
        {
            // Arrange
            var regexp = "a(bb|ac*dd+)+b*";

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.False(actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("00")]
        [InlineData("11")]
        [InlineData("000")]
        [InlineData("011")]
        [InlineData("110")]
        [InlineData("0000")]
        [InlineData("0011")]
        [InlineData("0110")]
        [InlineData("1001")]
        [InlineData("1100")]
        [InlineData("1111")]
        [InlineData("00000")]
        public void CheckInputByRegularExpression3_True(string expression)
        {
            // Arrange
            var regexp = "(0|(1(01*(00)*0)*1)*)*";  // ( 0 | ( 1 (0 1* (00)* 0)* 1)* )*

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.True(actual);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("10")]
        [InlineData("01")]
        [InlineData("101")]
        [InlineData("111")]
        [InlineData("0001")]
        [InlineData("0111")]
        [InlineData("1000")]
        [InlineData("00001")]
        public void CheckInputByRegularExpression3_False(string expression)
        {
            // Arrange
            var regexp = "(0|(1(01*(00)*0)*1)*)*"; // ( 0 | ( 1 (0 1* (00)* 0)* 1)* )*

            // Act
            var postfix = RegexpHelper.InfixToPostfix(regexp);
            var nfa = FsmEngine.PostfixToNfa(postfix);
            var dfa = FsmEngine.NfaToDfa(nfa);
            var minimizedDfa = FsmEngine.MinimizeFsmByBrzozowski(dfa);
            bool actual = FsmEngine.DfaSimulation(minimizedDfa, expression);

            // Assert
            Assert.False(actual);
        }
    }
}
