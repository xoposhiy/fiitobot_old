using System.Collections.Generic;
using System.Linq;
using fiitobot.GoogleSpreadsheet;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace lib.db
{
    public record GSpreadsheet(string SpreadsheetId, SheetsService SheetsService)
    {
        public List<GSheet> GetSheets()
        {
            var metadata = SheetsService.Spreadsheets.Get(SpreadsheetId).Execute();
            var sheets = metadata.Sheets.Select(x => new GSheet(SpreadsheetId, x.Properties.Title, x.Properties.SheetId ?? 0, SheetsService));
            return sheets.ToList();
        }

        public GSheet GetSheetById(int sheetId)
        {
            return GetSheets().First(s => s.SheetId == sheetId);
        }

        public GSheet GetSheetByName(string sheetName)
        {
            return GetSheets().First(s => s.SheetName == sheetName);
        }

        public void CreateNewSheet(string title)
        {
            var requests = new List<Request>
            {
                new()
                {
                    AddSheet = new AddSheetRequest
                    {
                        Properties = new SheetProperties
                        {
                            Title = title,
                            TabColor = new Color {Red = 1}
                        }
                    }
                }
            };
            var requestBody = new BatchUpdateSpreadsheetRequest {Requests = requests};
            var request = SheetsService.Spreadsheets.BatchUpdate(requestBody, SpreadsheetId);
            var response = request.Execute();
        }
    }
}
