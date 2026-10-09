namespace FinanzasApi.Helpers;

/// <summary>
/// Período de corte de un usuario. Con día de corte C, cada período va del día C+1 del mes
/// anterior hasta el día C del mes siguiente (ambos inclusive). Se identifica con el año y mes
/// en que termina. Ej.: corte día 15 → el período "octubre" va del 16-sep al 15-oct.
/// </summary>
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
