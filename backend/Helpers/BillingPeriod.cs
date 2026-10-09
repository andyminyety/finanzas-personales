namespace FinanzasApi.Helpers;

// A user's cut period. With cut day C, each period runs from day C+1 of the previous month
// through day C of the following month (both inclusive). It is identified by the year and month
// in which it ends. E.g. cut day 15 → the "October" period runs from Sep 16 to Oct 15.
public readonly record struct BillingPeriod(DateTime Start, DateTime End, int Year, int Month)
{
    public static BillingPeriod For(DateTime date, int cutDay)
    {
        var d = date.Date;
        var endMonth = new DateTime(d.Year, d.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        if (d.Day > cutDay) endMonth = endMonth.AddMonths(1);

        var end = new DateTime(endMonth.Year, endMonth.Month, cutDay, 0, 0, 0, DateTimeKind.Utc);
        var prev = endMonth.AddMonths(-1);
        var start = new DateTime(prev.Year, prev.Month, cutDay, 0, 0, 0, DateTimeKind.Utc).AddDays(1);
        return new BillingPeriod(start, end, end.Year, end.Month);
    }
}
