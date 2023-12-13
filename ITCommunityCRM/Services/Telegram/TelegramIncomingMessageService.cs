using ITCommunityCRM.Data;
using ITCommunityCRM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ITCommunityCRM.Services.Telegram;

// https://github.com/TelegramBots/Telegram.Bot.Examples/blob/master/Telegram.Bot.Examples.WebHook/Services/UpdateHandlers.cs
public class TelegramIncomingMessageService
{
    private readonly ITelegramBotClient telegramBotClient;
    private readonly ITCommunityCRMDbContext context;

    public TelegramIncomingMessageService(ITelegramBotClient telegramBotClient, ITCommunityCRMDbContext context)
    {
        this.telegramBotClient = telegramBotClient;
        this.context = context;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        var handlerTask = update switch
        {
            { MyChatMember: { } myChatMember } => OnChatJoinAsync(myChatMember, cancellationToken),
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:// chat member
            // UpdateType.Poll:
            //{ ChatJoinRequest: { } chatJoin } => OnChatJoin(chatJoin)
            //{ Message: { } message } => BotOnMessageReceived(message, cancellationToken),
            //{ EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
            //{ CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
            //{ InlineQuery: { } inlineQuery } => BotOnInlineQueryReceived(inlineQuery, cancellationToken),
            //{ ChosenInlineResult: { } chosenInlineResult } => BotOnChosenInlineResultReceived(chosenInlineResult, cancellationToken),
            _ => UnknownUpdateHandlerAsync(update, cancellationToken)
        };

        await handlerTask;
    }

    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken) =>
        Task.CompletedTask;


    private async Task OnChatJoinAsync(ChatMemberUpdated myChatMember, CancellationToken cancellationToken)
    {
        if (myChatMember.OldChatMember.User.Id != this.telegramBotClient.BotId)
        {
            return;
        }
        var chatId = myChatMember.Chat.Id;
        var chatName = myChatMember.Chat.Title;

        var chat = new TelegramChat() { Id = chatId, Title = chatName };

        if (myChatMember.NewChatMember.Status is ChatMemberStatus.Left)
        {
            chat.IsBotJoined = false;
        }
        else if (myChatMember.NewChatMember.Status is ChatMemberStatus.Member)
        {
            chat.IsBotJoined = true;
        }

        if (await this.context.TelegramChats.AnyAsync(x => x.Id == chatId, cancellationToken))
        {
            this.context.Update(chat);
        }
        else
        {
            this.context.Add(chat);
        }

        await this.context.SaveChangesAsync(cancellationToken);
    }
}

