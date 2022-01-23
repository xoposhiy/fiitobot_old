using Telegram.Bot;
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

}

public class Presenter : IPresenter
{
    private readonly ITelegramBotClient botClient;

    public Presenter(ITelegramBotClient botClient)
    {
        this.botClient = botClient;
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

    public async Task ShowContact(Contact contact, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, FormatContactAsHtml(contact), ParseMode.Html);
    }

    public async Task SayHasMoreResults(int moreResultsCount, long chatId)
    {
        await botClient.SendTextMessageAsync(chatId, $"Есть ещё {moreResultsCount} подходящих людей", ParseMode.Html);
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
{FormatGroup(contact.GroupIndex, contact.SubgroupIndex, contact.AdmissionYear)} (год поступления: {contact.AdmissionYear})
🏫 Школа: {contact.School}
🏙️ Город: {contact.City}
Поступление {FormatConcurs(contact.Concurs)} c рейтингом {contact.Rating}

📧 {contact.Email}
📞 {contact.Phone}
💬 {contact.Telegram}
{contact.Note}";
    }

    private string FormatGroup(int groupIndex, int subgroupIndex, int admissionYear)
    {
        var now = DateTime.Now;
        var delta = now.Month >= 8 ? 0 : 1;
        var course = now.Year - admissionYear + 1 - delta;
        return $"ФТ-{course}0{groupIndex}-{subgroupIndex}";
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