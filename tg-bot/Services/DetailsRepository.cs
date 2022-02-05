using fiitobot.GoogleSpreadsheet;

namespace fiitobot.Services;

public record SheetData(string SheetName, string SheetUrl, List<List<string>> Values);

public class DetailsRepository
{
    private readonly GSheetClient sheetClient;
    private readonly ContactsRepository contactsRepo;
    private readonly object locker = new();
    private DateTime lastUpdateTime = DateTime.MinValue;
    private volatile List<SheetData> data = new();

    public DetailsRepository(GSheetClient sheetClient, ContactsRepository contactsRepo)
    {
        this.sheetClient = sheetClient;
        this.contactsRepo = contactsRepo;
    }

    public void ReloadIfNeeded(bool force = false)
    {
        lock (locker)
        {
            if (DateTime.Now - lastUpdateTime <= TimeSpan.FromHours(24) && !force) return;
            var otherSpreadsheets = contactsRepo.GetOtherSpreadsheets();
            var newData = 
                from spreadsheet in otherSpreadsheets 
                let sheet = sheetClient.GetSheetByUrl(spreadsheet) 
                let name = sheet.SheetName 
                let values = sheet.ReadRange("A1:ZZ") 
                select new SheetData(name, spreadsheet, values);
            data = newData.ToList();
            lastUpdateTime = DateTime.Now;
        }
    }
    
    public Detail[] GetPersonDetails(Contact contact)
    {
        ReloadIfNeeded();
        var result = new List<Detail>();
        foreach (var (name, url, values) in data)
        {
            try
            {
                if (values.Count <= 1) continue;
                var headerRowsCount = 1;
                var headers = values[0];
                if (string.IsNullOrWhiteSpace(headers[0]))
                {
                    headerRowsCount++;
                    var h0 = headers;
                    headers = values[1];
                    for (int i = 0; i < h0.Count; i++)
                    {
                        if (i < headers.Count && string.IsNullOrWhiteSpace(headers[i]))
                            headers[i] = h0[i];
                    }
                }

                foreach (var row in values.Skip(headerRowsCount))
                {
                    if (RowContains(row, contact))
                    {
                        for (int i = 0; i < row.Count; i++)
                        {
                            var value = row[i];
                            if (string.IsNullOrWhiteSpace(value)) continue;
                            if (i >= headers.Count || string.IsNullOrWhiteSpace(headers[i])) continue;
                            var ignoredValues = GetContactValuesToIgnore(contact);
                            if (ignoredValues.Any(ignoredValue => 
                                    value.StartsWith(ignoredValue, StringComparison.OrdinalIgnoreCase))) continue;
                            if (result.Any(res => res.Parameter.Equals(headers[i], StringComparison.OrdinalIgnoreCase)))
                                continue;
                            var detail = new Detail(name, headers[i].Replace("\n", " ").Replace("\r", " "), value, url);
                            result.Add(detail);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"spreadsheet {name} at {url} error", e);
            }
        }
        return result.ToArray();
    }

    private string[] GetContactValuesToIgnore(Contact contact)
    {
        return new[]
        {
            contact.FirstName,
            contact.LastName,
            contact.Patronymic,
            contact.FirstName + " " + contact.LastName,
            contact.LastName + " " + contact.FirstName + " " + contact.Patronymic,
            contact.City,
            contact.Concurs,
            contact.City,
            contact.Email,
            contact.Phone,
            contact.Email,
            contact.School,
            contact.Telegram,
            contact.FormatOfficialGroup(DateTime.Now),
            contact.FormatOfficialGroup(DateTime.Now.Subtract(TimeSpan.FromDays(365))),
            contact.FormatMnemonicGroup(DateTime.Now),
            contact.FormatMnemonicGroup(DateTime.Now.Subtract(TimeSpan.FromDays(365)))
        }.Where(v => v.Length > 1).ToArray();
    }

    private bool RowContains(List<string> row, Contact contact)
    {
        if (row.Any(v => v.StartsWith(contact.LastName + " " + contact.FirstName, StringComparison.OrdinalIgnoreCase)))
            return true;
        var firstNameIndex = row.IndexOf(contact.FirstName);
        var lastNameIndex = row.IndexOf(contact.LastName);
        if (Math.Abs(firstNameIndex - lastNameIndex) == 1)
            return true;
        return false;
    }
    
    public void ForceReload()
    {
        ReloadIfNeeded(true);
    }
}

public record Detail(string Rubric, string Parameter, string Value, string SourceUrl);