using fiitobot.Services;
using lib.db;
using Microsoft.Extensions.Configuration;

namespace tests;

public class ContactsRepoBuilder
{
    public ContactsRepository Build()
    {
        var config = new ConfigurationManager();
        config.AddJsonFile("appsettings.Development.json");
        return new ContactsRepository(new GSheetClient(config), config);
    }
}