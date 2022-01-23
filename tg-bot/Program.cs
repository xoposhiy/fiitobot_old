using fiitobot;
using fiitobot.Services;
using lib.db;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddHostedService<ConfigureWebhook>();
services.AddHttpClient("tgwebhook")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var config = sp.GetService<IConfiguration>()!;
        var token = config.GetValue<string>("BotToken");
        return new TelegramBotClient(token, httpClient);
    });
services.AddScoped<HandleUpdateService>();
services.AddSingleton<IPresenter, Presenter>();
services.AddSingleton<ContactsRepository>();
services.AddSingleton<GSheetClient>();
var app = builder.Build();
var token = app.Configuration.GetValue<string>("BotToken");
app.UseRouting();
app.MapPost($"/bot/{token}", async (HttpContext context, [FromServices] HandleUpdateService handleUpdateService) =>
{
    using var sr = new StreamReader(context.Request.Body);
    var str = await sr.ReadToEndAsync();
    var update = JsonConvert.DeserializeObject<Update>(str)!;
    await handleUpdateService.Handle(update);
    return Results.Ok();
});
app.Run();