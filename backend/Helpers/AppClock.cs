namespace FinanzasApi.Helpers;

/// <summary>
/// "Hoy" según la hora de República Dominicana (UTC-4, sin horario de verano).
/// Así el corte de un día no se ejecuta antes de que termine ese día para el usuario.
/// </summary>
public static class AppClock
{
    public static DateTime Today =>
        DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-4).Date, DateTimeKind.Utc);
}
