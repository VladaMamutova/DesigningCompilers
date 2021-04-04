using System.Collections.Generic;
using System.Linq;
using RegexpLexer.Logic;

namespace RegexpLexer.Utils
{
    public class FsmEqualityComparer : IEqualityComparer<Fsm>
    {
        public bool Equals(Fsm fsm1, Fsm fsm2)
        {
            if (ReferenceEquals(fsm1, fsm2))
            {
                return true;
            }

            if (fsm1 == null || fsm2 == null)
            {
                return false;
            }

            if (fsm1.Final.Count != fsm2.Final.Count)
            {
                return false;
            }

            if (!fsm1.Final.OrderBy(final => final.Id)
                .SequenceEqual(fsm2.Final.OrderBy(final => final.Id)))
            {
                return false;
            }

            var states1 = fsm1.GetAllStates();
            var states2 = fsm2.GetAllStates();

            if (states1.Count != states2.Count)
            {
                return false;
            }

            foreach (var state1 in states1)
            {
                var state2 = states2.FirstOrDefault(s => s.Id == state1.Id);
                if (state2 == null || state1.Moves.Count != state2.Moves.Count)
                {
                    return false;
                }

                foreach (var move1 in state1.Moves)
                {
                    // По логике не может быть дублирующихся переходов
                    // (по одному и тому же символу в одно и то же состояние).
                    // Также по логике есть набор уникальных состояний автомата,
                    // а в переходах - только ссылки на них, поэтому
                    // проверяем только символ и номер состояния.
                    if (!state2.Moves.Exists(move2 =>
                        move2.Key == move1.Key &&
                        move2.Value.Id == move1.Value.Id))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public int GetHashCode(Fsm fsm)
        {
            int hCode = fsm.Final.Aggregate(fsm.Start.Id, (current, final) => current ^ final.Id);
            return hCode.GetHashCode();
        }
    }
}
