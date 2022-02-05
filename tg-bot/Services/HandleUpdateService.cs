using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace fiitobot.Services;

public class HandleUpdateService
{
    private readonly ILogger<HandleUpdateService> logger;
    private readonly ContactsRepository contactsRepository;
    private readonly DetailsRepository detailsRepository;
    private readonly IPresenter presenter;

    public HandleUpdateService(ILogger<HandleUpdateService> logger, ContactsRepository contactsRepository, DetailsRepository detailsRepository, IPresenter presenter)
    {
        this.logger = logger;
        this.contactsRepository = contactsRepository;
        this.detailsRepository = detailsRepository;
        this.presenter = presenter;
    }

    public async Task Handle(Update update)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            UpdateType.InlineQuery => BotOnInlineQuery(update.InlineQuery!),
            UpdateType.EditedMessage => BotOnMessageReceived(update.EditedMessage!),
            _                             => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(update, exception);
        }
    }

    private async Task BotOnInlineQuery(InlineQuery inlineQuery)
    {
        if (inlineQuery.Query.Length < 2) return;
        var foundContacts = SearchContacts(inlineQuery.Query);
        if (foundContacts.Length > 10) return;
        await presenter.InlineSearchResults(inlineQuery.Id, foundContacts);
    }

    private async Task BotOnMessageReceived(Message message)
    {
        logger.LogInformation("Receive message type {messageType}: {text} from {message.From} charId {message.Chat.Id}", message.Type, message.Text, message.From, message.Chat.Id);
        if (!await EnsureHasRights(message.From, message.Chat.Id)) return;
        if (message.Type == MessageType.Text)
            await HandlePlainText(message.Text!, message.Chat.Id);
    }

    private async Task<bool> EnsureHasRights(User? user, long chatId)
    {
        if (user?.Username != null && contactsRepository.IsAdmin(user.Username)) return true;
        await presenter.SayNoRights(chatId);
        return false;
    }

    public async Task HandlePlainText(string text, long fromChatId)
    {
        if (text == "/reload")
        {
            contactsRepository.ForceReload();
            detailsRepository.ForceReload();
            await presenter.SayReloaded(fromChatId);
            return;
        }
        var contacts = SearchContacts(text);
        const int maxResultsCount = 3;
        foreach (var contact in contacts.Take(maxResultsCount))
        {
            await presenter.ShowContact(contact, fromChatId);
        }
        if (contacts.Length > maxResultsCount)
            await presenter.SayHasMoreResults(contacts.Length-maxResultsCount, fromChatId);
        if (contacts.Length == 0)
            await presenter.SayNoResults(fromChatId);
        if (contacts.Length == 1)
        {
            var contact = contacts[0];
            var details = detailsRepository.GetPersonDetails(contact);
            await presenter.ShowDetails(details, fromChatId);
        }
    }

    private Contact[] SearchContacts(string text)
    {
        var res = contactsRepository.FindContacts(text);
        if (res.Length > 0) return res;
        var parts = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Select(part => contactsRepository.FindContacts(part))
            .Where(g => g.Length > 0)
            .MinBy(g => g.Length) 
               ?? Array.Empty<Contact>();
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        logger.LogInformation("Unknown update type: {updateType}", update.Type);
        return Task.CompletedTask;
    }

    public async Task HandleErrorAsync(Update incomingUpdate, Exception exception)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        logger.LogInformation("HandleError: {ErrorMessage}", errorMessage);
        await presenter.ShowErrorToDevops(incomingUpdate, errorMessage);
        logger.LogInformation("Send error to devops");
    }
}