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
    string Note);