namespace FinanzasApi.Helpers;

// "Today" according to Dominican Republic time (UTC-4, no daylight saving time).
// This way a day's cut does not run before that day has ended for the user.
public static class AppClock
{
    public static DateTime Today =>
        DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-4).Date, DateTimeKind.Utc);
}
