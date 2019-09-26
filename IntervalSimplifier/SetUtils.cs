using System.Collections.Generic;
using System.Linq;
using IntervalSimplifier;

namespace Tests
{
    public static class SetUtils
    {
        public static IEnumerable<Interval> Merge(IEnumerable<Interval> items)
        {
            var ordered = items.OrderBy(x => x.Start).ToList();

            var overlapping = Overlapping(ordered);
            var nonOverlapping = NonOverlapping(ordered);
            
            var concat = overlapping.Concat(nonOverlapping);

            var result = concat.OrderBy(x => x.Start);

            return result;
        }

        private static IEnumerable<Interval> NonOverlapping(IReadOnlyCollection<Interval> ordered)
        {
            return ordered.Where(x => ordered.Except(new [] {x}).All(y => Equals(x.Intersect(y), Interval.Empty))).ToList();
        }

        private static IEnumerable<Interval> Overlapping(IReadOnlyCollection<Interval> ordered)
        {
            var intersections = ordered.Zip(ordered.Skip(1), (a, b) => a.Intersect(b)).ToList();
            var leftParts = ordered.Zip(ordered.Skip(1), (a, b) => a.Left(b)).ToList();
            var rightParts = ordered.Zip(ordered.Skip(1), (a, b) => a.Right(b)).ToList();
            return intersections.Concat(leftParts).Concat(rightParts)
                .Where(x => !Equals(Interval.Empty, x));
        }
    }
}