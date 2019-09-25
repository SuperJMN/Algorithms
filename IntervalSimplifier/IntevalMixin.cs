using System.Linq;

namespace IntervalSimplifier
{
    public static class IntevalMixin
    {
        public static Interval Intersect(this Interval a, Interval b)
        {
            if (b.Start > a.End || a.Start > b.End)
            {
                return Interval.Empty;
            }

            var intersect = new Interval(new[] { a.Start, b.Start}.Max(), new[]{ a.End, b.End}.Min());
            return intersect;
        }

        public static Interval Left(this Interval a, Interval b)
        {
            if (a.End < b.Start)
            {
                return Interval.Empty;
            }

            return new Interval(a.Start, b.Start);
        }

        public static Interval Right(this Interval a, Interval b)
        {
            if (a.End < b.Start)
            {
                return Interval.Empty;
            }

            return new Interval(b.End, a.End);
        }
    }
}