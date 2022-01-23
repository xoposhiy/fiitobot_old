using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace lib.db
{
    public class GSheetClient
    {
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
    }
}
