using System.Collections.Generic;
using System.Linq;

namespace IntervalSimplifier
{
    public static class SetUtils
    {
        public static IEnumerable<Interval> Intervals(IEnumerable<Interval> items)
        {
            var ordered = items.OrderBy(x => x.Start).ToList();
            var intersections = ordered.Zip(ordered.Skip(1), (a, b) => a.Intersect(b)).ToList();
            var leftParts = ordered.Zip(ordered.Skip(1), (a, b) => a.Left(b)).ToList();
            var rightParts = ordered.Zip(ordered.Skip(1), (a, b) => a.Right(b)).ToList();

            var concat = intersections.Concat(leftParts).Concat(rightParts);

            var result = concat
                .OrderBy(x => x.Start)
                .Where(x => !Equals(Interval.Empty, x));

            return result;
        }
    }
}