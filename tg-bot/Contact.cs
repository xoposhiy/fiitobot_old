namespace fiitobot;

public record Contact(
    int AdmissionYear,
    string LastName,
    string FirstName,
    string Patronymic,
    int GroupIndex,
    int SubgroupIndex,
    string City,
    string School,
    string Concurs,
    string Rating,
    string Telegram,
    string Phone,
    string Email,
    string Note)
{
    public string FormatMnemonicGroup(DateTime now)
    {
        var delta = now.Month >= 8 ? 0 : 1;
        var course = now.Year - AdmissionYear + 1 - delta;
        return $"ФТ-{course}0{GroupIndex}-{SubgroupIndex}";
    }
    
    public string FormatOfficialGroup(DateTime now)
    {
        var delta = now.Month >= 8 ? 0 : 1;
        var course = now.Year - AdmissionYear + 1 - delta;
        var id = new[] { "0801", "0802", "0809", "0810" }[GroupIndex-1];
        return $"МЕН-{course}{AdmissionYear%10}{id}";
    }

}

public static class PluralizeExtensions
{
    public static string Pluralize(this int count, string oneTwoManyPipeSeparated)
    {
        var parts = oneTwoManyPipeSeparated.Split("|");
        return count.Pluralize(parts[0], parts[1], parts[2]);
    }
    
    public static string Pluralize(this int count, string one, string two, string many)
    {
        if (count <= 0 || (count % 100 >= 10 && count % 100 <= 20) || count%10 > 4)
            return count + " " + many;
        if (count % 10 == 1) return count + " " + one;
        return count + " " + two;
    }
}  