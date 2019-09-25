using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NodaTime.Extensions;
using Xunit;

namespace IntervalSimplifier
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var merge = IntervalJoiner.Merge(new List<TaggedInterval<string>>
            {
                new TaggedInterval<string>
                {
                    Value = "Code",
                    Interval = new NodaTime.Interval(InstantBuilder.Create(1,1,1), InstantBuilder.Create(2,1,1)),
                },
                new TaggedInterval<string>
                {
                    Value = "Code",
                    Interval = new NodaTime.Interval(InstantBuilder.Create(2,1,1), InstantBuilder.Create(3,1,1)),
                }
            });

            merge.Single().ToString().Should().Be("Code: Code, Intervals: 0001-01-01T00:00:00Z/0003-01-01T00:00:00Z");
        }

        [Fact]
        public void Test2()
        {
            var merge = IntervalJoiner.Merge(new List<TaggedInterval<int>>
            {
                new TaggedInterval<int>
                {
                    Value = 1,
                    Interval = new NodaTime.Interval(InstantBuilder.Create(1, 1, 1), InstantBuilder.Create(1, 1, 15)),
                },
                new TaggedInterval<int>
                {
                    Value = 2,
                    Interval = new NodaTime.Interval(InstantBuilder.Create(1, 1, 7), InstantBuilder.Create(1, 1, 8)),
                }
            });

        }
    }

    public static class IntervalJoiner
    {
        public static IEnumerable<SimplifiedIntervals<T>> Merge<T>(IEnumerable<TaggedInterval<T>> codes)
        {
            var query = from code in codes
                group code by code.Value
                into codesByText
                let intervals = codesByText.Select(x => x.Interval)
                select new SimplifiedIntervals<T>(codesByText.Key, IntervalFixer.Simplify(intervals.ToList()).ToList());

            return query;
        }
    }

    public class TaggedInterval<T>
    {
        public T Value { get; set; }
        public NodaTime.Interval Interval { get; set; }

        public override string ToString()
        {
            return $"{Value}: {Interval}";
        }
    }

    public static class IntervalFixer
    {
        public static IEnumerable<NodaTime.Interval> Simplify(IList<NodaTime.Interval> list)
        {
            var sorted = list.OrderBy(x => x.Start).ToList();
            var initial = new List<NodaTime.Interval> { sorted.First() };

            var seq = sorted
                .Aggregate(initial, (previous, candidate) =>
                {
                    var last = previous.Last();
                    if (candidate.Start <= last.End)
                    {
                        var start = new[] { last.Start, candidate.Start}.Min();
                        var end = new[] { last.End, candidate.End}.Max();

                        var toAdd = new NodaTime.Interval(start, end);
                        return previous.Take(previous.Count - 1).Concat(new[] { toAdd }).ToList();
                    }

                    return previous.Concat(new[] { candidate }).ToList();
                });

            return seq;
        }
    }

    public class SimplifiedIntervals<T>
    {
        public T Code { get; }
        public IList<NodaTime.Interval> Intervals { get; }

        public SimplifiedIntervals(T code, IList<NodaTime.Interval> intervals)
        {
            Code = code;
            Intervals = intervals;
        }

        public override string ToString()
        {
            return $"{nameof(Code)}: {Code}, {nameof(Intervals)}: {string.Join(";", Intervals)}";
        }
    }
}
