namespace FinanzasApi.Helpers;

// Validates Dominican cédula (11 digits) and RNC (9 digits) using the official
// check-digit algorithm. It does not query DGII/JCE (there is no official public API).
public static class CedulaRncValidator
{
    public static string Normalize(string? value) =>
        new string((value ?? "").Where(char.IsDigit).ToArray());

    public static bool IsValidCedula(string? value)
    {
        var d = Normalize(value);
        if (d.Length != 11) return false;

        int sum = 0;
        for (int i = 0; i < 10; i++)
        {
            int n = (d[i] - '0') * (i % 2 == 0 ? 1 : 2);
            if (n >= 10) n = n / 10 + n % 10;
            sum += n;
        }
        int check = (10 - sum % 10) % 10;
        return check == d[10] - '0';
    }

    public static bool IsValidRnc(string? value)
    {
        var d = Normalize(value);
        if (d.Length != 9) return false;

        int[] weights = { 7, 9, 8, 6, 5, 4, 3, 2 };
        int sum = 0;
        for (int i = 0; i < 8; i++) sum += (d[i] - '0') * weights[i];

        int rest = sum % 11;
        int check = rest switch { 0 => 2, 1 => 1, _ => 11 - rest };
        return check == d[8] - '0';
    }

    // Individual → cédula; legal entity → RNC. Returns null when valid.
    public static string? Validate(string personType, string? value)
    {
        if (personType == Models.PersonTypes.Fisica)
            return IsValidCedula(value) ? null : "La cédula no es válida (11 dígitos con dígito verificador correcto).";
        return IsValidRnc(value) ? null : "El RNC no es válido (9 dígitos con dígito verificador correcto).";
    }
}
