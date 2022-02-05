using System.IO;
using System.Text.RegularExpressions;
using fiitobot.GoogleSpreadsheet;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace fiitobot.GoogleSpreadsheet
{
    public class GSheetClient
    {
        // https://docs.google.com/spreadsheets/d/1XFKrFCScUD5APkZFALQp0XXgAuifHVxmwMSt1J2TqE8/edit#gid=419729714
        public static Regex urlRegex = new Regex("https://docs.google.com/spreadsheets/d/(.+)/edit#gid=(.+)", RegexOptions.Compiled);
        public GSheetClient(IConfiguration configuration)
        {
            SheetsService = new SheetsService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromJson(configuration.GetValue<string>("GOOGLE_AUTH_JSON"))
                        .CreateScoped(SheetsService.Scope.Spreadsheets),
                    ApplicationName = "fiitobot"
                });
        }

        public GSpreadsheet GetSpreadsheet(string spreadsheetId) =>
            new(spreadsheetId, SheetsService);

        private SheetsService SheetsService { get; }

        public GSheet GetSheetByUrl(string url)
        {
            var match = urlRegex.Match(url);
            var spreadsheetId = match.Groups[1].Value;
            var sheetId = int.Parse(match.Groups[2].Value);
            return GetSpreadsheet(spreadsheetId).GetSheetById(sheetId);
        }
    }
}
