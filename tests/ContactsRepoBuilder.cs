using fiitobot.GoogleSpreadsheet;
using fiitobot.Services;
using Microsoft.Extensions.Configuration;

namespace tests;

public class ContactsRepoBuilder
{
    public ContactsRepository Build()
    {
        var config = new ConfigBuilder().Build();
        return new ContactsRepository(new GSheetClient(config), config);
    }
}

public class GSheetClientBuilder
{
    public GSheetClient Build()
    {
        return new GSheetClient(new ConfigBuilder().Build());
    }
}

public class ConfigBuilder
{
    public ConfigurationManager Build()
    {
        var config = new ConfigurationManager();
        config.AddJsonFile("appsettings.Development.json");
        return config;
    }
}