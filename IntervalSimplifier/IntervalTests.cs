using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using Tests;
using Xunit;

namespace IntervalSimplifier
{
    public class IntervalTests
    {

        public static string format = "dd/MM/yyyy";

        [Fact]
        public void Complex()
        {
            var items = new List<Interval>
            {
                new Interval(DateTime.Parse("2019/01/1"), DateTime.Parse("2019/01/15")),
                new Interval(DateTime.Parse("2019/01/08"), DateTime.Parse("2019/01/10")),
                new Interval(DateTime.Parse("2019/01/16"), DateTime.Parse("2019/01/31")),
                new Interval(DateTime.Parse("2019/01/20"), DateTime.Parse("2019/01/20"))
            };

            var joines = SetUtils.Merge(items);

            joines.Select(x => x.ToString()).Should().BeEquivalentTo(new[]
            {
                "01/01/2019 => 08/01/2019",
                "08/01/2019 => 10/01/2019",
                "10/01/2019 => 15/01/2019",
                "16/01/2019 => 20/01/2019",
                "20/01/2019 => 20/01/2019",
                "20/01/2019 => 31/01/2019",
            });
        }

        [Fact]
        public void EternityInterrupted()
        {
            var items = new List<Interval>
            {
                new Interval(DateTimeOffset.MinValue, DateTimeOffset.MaxValue),
                new Interval(DateTime.Parse("2019/01/08"), DateTime.Parse("2019/01/10")),
            };

            var joines = SetUtils.Merge(items);

            joines.Select(x => x.ToString()).Should().BeEquivalentTo(

                "01/01/0001 => 08/01/2019", 
                "08/01/2019 => 10/01/2019", 
                "10/01/2019 => 31/12/9999");
        }

        [Fact]
        public void Breaking()
        {
            var input = 
@"01/01/2017 +00:00	31/01/2017 +00:00
01/02/2017 +00:00	28/02/2017 +00:00
";
            var intervals = from line in input.Lines()
                let split = line.Split("\t")
                let start = split[0]
                let end = split[1]
                select new Interval(DateTimeOffset.Parse(start), DateTimeOffset.Parse(end));

            var joines = SetUtils.Merge(intervals);

            joines.Select(x => x.ToString()).Should().BeEquivalentTo(
                "01/01/2017 => 31/01/2017", 
                "01/02/2017 => 28/02/2017");
        }

        public static DateTimeOffset Parse(string s)
        {

            return DateTimeOffset.ParseExact(s, format, CultureInfo.InvariantCulture);
        }
        
        [Theory]
        [InlineData("03-01-2019 => 08-01-2019", "05-01-2019 => 09-01-2019", "03-01-2019 => 05-01-2019")]
        [InlineData("03-01-2019 => 08-01-2019", "09-01-2019 => 12-01-2019", "{empty}")]
        public void LeftTest(string a, string b, string c)
        {
            var result = Interval.Parse(a).Left(Interval.Parse(b));

            var expected = Interval.Parse(c);

            result.Should().Be(expected);
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<string> Lines(this string str)
        {
            using var reader = new StringReader(str);
            string line;

            while((line = reader.ReadLine()) != null) 
            {
                yield return line;
            }
        }
    }

    public class Interval
    {
        public static string format = "dd-MM-yyyy";

        public static Interval Parse(string s)
        {
            if (s == "{empty}")
            {
                return Empty;
            }

            var splitted = s.Split("=>");
            var left = splitted[0].Trim();
            var right = splitted[1].Trim();
            var one = DateTimeOffset.ParseExact(right, format, CultureInfo.InvariantCulture);
            var two = DateTimeOffset.ParseExact(left, format, CultureInfo.InvariantCulture);
            return new Interval(two, one);
        }

        public static Interval Empty => new Interval(DateTimeOffset.MinValue, DateTimeOffset.MinValue);

        public Interval(DateTimeOffset start, DateTimeOffset end)
        {
            Start = start;
            End = end;
        }

        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }

        public override string ToString()
        {
            if (Equals(Empty))
            {
                return "{empty}";
            }

            var format = "dd/MM/yyyy";
            return $"{Start.ToString(format)} => {End.ToString(format)}";
        }

        protected bool Equals(Interval other)
        {
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Interval) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public bool Contains(Interval interval)
        {
            var contains = Start == interval.Start && interval.End < End || End == interval.End && interval.Start > Start;
            return contains;
        }
    }
}

