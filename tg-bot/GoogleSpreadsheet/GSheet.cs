using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using lib.db;

namespace fiitobot.GoogleSpreadsheet
{
    public record GSheet(string SpreadsheetId, string SheetName, int SheetId, SheetsService SheetsService)
    {
        public string ReadCell(ValueTuple<int, int> cellCoords)
        {
            return Read(cellCoords);
        }

        public List<List<string>> ReadRange((int top, int left) rangeStart, (int top, int left) rangeEnd)
        {
            var (top, left) = rangeStart;
            var (bottom, right) = rangeEnd;
            left++;
            top++;
            right++;
            bottom++;
            var range = $"R{top}C{left}:R{bottom}C{right}";
            return ReadRange(range);
        }

        public List<List<string>> ReadRange(string range)
        {
            var fullRange = $"{SheetName}!{range}";
            var request = SheetsService.Spreadsheets.Values.Get(SpreadsheetId, fullRange);
            var response = request.Execute();
            var values = response.Values ?? new List<IList<object>>();
            var res = values.Select(l => l?.Select(o => o?.ToString() ?? "").ToList() ?? new List<string>()).ToList();
            return res;
        }

        public GSheetEditsBuilder Edit()
        {
            return new(SheetsService, SpreadsheetId, SheetId);
        }

        public void ClearRange(string sheetName, (int top, int left) rangeStart, (int top, int left) rangeEnd)
        {
            var (top, left) = rangeStart;
            var (bottom, right) = rangeEnd;
            left++;
            top++;
            right++;
            bottom++;
            var range = $"R{top}C{left}:R{bottom}C{right}";
            var fullRange = $"{sheetName}!{range}";
            var requestBody = new ClearValuesRequest();
            var deleteRequest = SheetsService.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, fullRange);
            var deleteResponse = deleteRequest.Execute();
        }

        private string Read((int top, int left) rangeStart)
        {
            var (top, left) = rangeStart;
            left++;
            top++;
            var range = $"R{top}C{left}";
            var values = ReadRange(range);
            return values.First().First();
        }
    }
}
