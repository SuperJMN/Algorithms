using System;
using NodaTime;

namespace IntervalSimplifier
{
    public static class InstantBuilder
    {
        public static Instant Create(int year, int month, int day)
        {
            return Instant.FromDateTimeOffset(new DateTimeOffset(year, month, day, 0, 0, 0,0, TimeSpan.Zero));
        }
    }
}