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
            public static Fsm InitOneStartOneEndFsm()
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

            public static Fsm InitOneStartOneEndReversedFsm()
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

            public static Fsm InitOneStartTwoEndsFsm()
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

            public static Fsm InitOneStartTwoEndsReversedFsm()
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
        }

        [Fact]
        public void ReverseFsm_OneStartOneFinal()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitOneStartOneEndFsm();
            var fsmExpected = FsmEngineTestData.InitOneStartOneEndReversedFsm();

            // Act
            Fsm reversedFsm = FsmEngine.ReverseFsm(fsm);

            // Assert
            Assert.Equal(fsmExpected, reversedFsm, new FsmEqualityComparer());
        }

        [Fact]
        public void ReverseFsm_OneStartTwoFinals()
        {
            // Arrange
            var fsm = FsmEngineTestData.InitOneStartTwoEndsFsm();
            var fsmExpected = FsmEngineTestData.InitOneStartTwoEndsReversedFsm();

            // Act
            Fsm reversedFsm = FsmEngine.ReverseFsm(fsm);

            // Assert
            Assert.Equal(fsmExpected, reversedFsm, new FsmEqualityComparer());
        }

    }
}
