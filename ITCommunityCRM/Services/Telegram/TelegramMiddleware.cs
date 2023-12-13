using ITCommunityCRM.Models.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ITCommunityCRM.Services.Telegram;

public static class RegisterTelegramMiddlewareExtensions
{
    public static IApplicationBuilder UseIncomingTelegramMessage(this IApplicationBuilder applicationBuilder) =>
        applicationBuilder.UseMiddleware<TelegramMiddleware>();
}

public class TelegramMiddleware
{
    public static string TelegramEndpoint => "/TelegramUpdates";

    private readonly RequestDelegate next;
    private readonly ITelegramBotClient botClient;
    private readonly IOptions<AppSettings> config;

    public TelegramMiddleware(RequestDelegate requestDelegate, ITelegramBotClient botClient, IOptions<AppSettings> config)
    {
        this.next = requestDelegate;
        this.botClient = botClient;
        this.config = config;

        this.SetWebHookAsync().Wait();
    }

    private async Task SetWebHookAsync()
    {
        await this.botClient.DeleteWebhookAsync(true);
        var url = config.Value.DomainLink + TelegramEndpoint;
        await this.botClient.SetWebhookAsync(url);
    }

    public async Task InvokeAsync(HttpContext httpContext, TelegramIncomingMessageService handleUpdateService)
    {
        if (httpContext.Request.Path.Value != TelegramEndpoint || httpContext.Request.Method != "POST")
        {
            await next(httpContext);
            return;
        }

        var token = CancellationToken.None;

        var update = await this.GetUpdateMessageFromResponseAsync(httpContext.Request.Body, token);
        await handleUpdateService.HandleUpdateAsync(update ?? throw new InvalidDataException(), token);

        httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
        await httpContext.Response.CompleteAsync();
    }

    private async Task<Update?> GetUpdateMessageFromResponseAsync(Stream body, CancellationToken token)
    {
        using var streamReader = new StreamReader(body);
        var content = await streamReader.ReadToEndAsync(token);
        return JsonConvert.DeserializeObject<Update>(content);
    }
}
