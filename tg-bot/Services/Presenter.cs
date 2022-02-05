using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace fiitobot.Services;

public interface IPresenter
{
    Task ShowContact(Contact contact, long chatId);
    Task SayHasMoreResults(int moreResultsCount, long chatId);
    Task SayNoResults(long chatId);
    Task SayNoRights(long chatId);
    Task InlineSearchResults(string inlineQueryId, Contact[] foundContacts);
    Task ShowDetails(Detail[] contactDetails, long fromChatId);
    Task SayReloaded(long chatId);
    Task ShowErrorToDevops(Update incomingUpdate, string errorMessage);
}

public class Presenter : IPresenter
{
    private readonly ITelegramBotClient botClient;
    private readonly long devopsChatId;

    public Presenter(ITelegramBotClient botClient, IConfiguration config)
    {
        this.botClient = botClient;
        devopsChatId = config.GetValue<long>("DevopsChatId");
    }

    public async Task InlineSearchResults(string inlineQueryId, Contact[] foundContacts)
    {
        var results = foundContacts.Select(c => 
            new InlineQueryResultArticle(c.GetHashCode().ToString(), $"{c.LastName} {c.FirstName} {c.Patronymic}", 
                new InputTextMessageContent(FormatContactAsHtml(c))
                {
                    ParseMode = ParseMode.Html
                }));
        await botClient.AnswerInlineQueryAsync(inlineQueryId, results, 60);
    }

    public async Task ShowDetails(Detail[] contactDetails, long chatId)
    {
        var text = new StringBuilder();
        foreach (var rubric in contactDetails.GroupBy(d => d.Rubric))
        {
            text.AppendLine($"<b>{EscapeForHtml(rubric.Key)}</b> (<a href=\"{rubric.First().SourceUrl}\">источник</a>)");
            foreach (var detail in rubric)
                text.AppendLine($" • {EscapeForHtml(detail.Parameter)}: {EscapeForHtml(detail.Value)}");
        }
        await botClient.SendTextMessageAsync(chatId, text.ToString(), ParseMode.Html);
    }

    public async Task SayReloaded(long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, "Перезагрузил!", ParseMode.Html);
    }

    public async Task ShowErrorToDevops(Update incomingUpdate, string errorMessage)
    {
        await botClient.SendTextMessageAsync(devopsChatId, FormatErrorHtml(incomingUpdate, errorMessage), ParseMode.Html);
    }

    private string FormatErrorHtml(Update incomingUpdate, string errorMessage)
    {
        var incoming = incomingUpdate.Type switch
        {
            UpdateType.Message => $"From: {incomingUpdate.Message!.From} Message: {incomingUpdate.Message!.Text}",
            UpdateType.EditedMessage => $"From: {incomingUpdate.EditedMessage!.From} Message: {incomingUpdate.EditedMessage!.Text}",
            UpdateType.InlineQuery => $"From: {incomingUpdate.InlineQuery!.From} Query: {incomingUpdate.InlineQuery!.Query}",
            _ => $"Message with type {incomingUpdate.Type}"
        };

        return $"Error handling message: <pre>{EscapeForHtml(incoming)}</pre>\n\nError:\n<pre>{EscapeForHtml(errorMessage)}</pre>";
    }

    private string EscapeForHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    public async Task ShowContact(Contact contact, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, FormatContactAsHtml(contact), ParseMode.Html);
    }

    public async Task SayHasMoreResults(int moreResultsCount, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, $"Есть ещё {moreResultsCount.Pluralize("подходящий человек", "подходящих человека", "подходящих человек")}", ParseMode.Html);
    }

    public async Task SayNoResults(long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, $"Не нашел никого подходящего", ParseMode.Html);
    }

    public async Task SayNoRights(long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, $"Этот бот только для команды ФИИТ", ParseMode.Html);
    }

    public string FormatContactAsHtml(Contact contact)
    {
        return $@"<b>{contact.LastName} {contact.FirstName} {contact.Patronymic}</b>
{contact.FormatMnemonicGroup(DateTime.Now)} (год поступления: {contact.AdmissionYear})
🏫 Школа: {contact.School}
🏙️ Город: {contact.City}
Поступление {FormatConcurs(contact.Concurs)} c рейтингом {contact.Rating}

📧 {contact.Email}
📞 {contact.Phone}
💬 {contact.Telegram}
{EscapeForHtml(contact.Note)}";
    }

    private string FormatConcurs(string concurs)
    {
        if (concurs == "О") return "по общему конкурсу";
        else if (concurs == "БЭ") return "по олимпиаде";
        else if (concurs == "К") return "по контракту";
        else if (concurs == "КВ") return "по льготной квоте";
        else if (concurs == "Ц") return "по целевой квоте";
        else return "неизвестно как 🤷‍";
    }


}