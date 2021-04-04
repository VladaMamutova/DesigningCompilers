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

            public static Fsm InitDeterministicFsmWithStartFromEpsilon()
            {
                var state0 = new State(0);
                var state1 = new State(1);
                var state2 = new State(2);
                var state3 = new State(3);
                var fsm = new Fsm(state0, state3);
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
        public void DeterminizeFsm_OneFinal()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitFsmWithOneFinal();
            var expected = FsmEngineTestData.InitDeterministicFsmWithOneFinal();

            // Act
            Fsm actual = FsmEngine.DeterminizeFsm(fsm);

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
            Fsm actual = FsmEngine.DeterminizeFsm(fsm);

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
    }
}
