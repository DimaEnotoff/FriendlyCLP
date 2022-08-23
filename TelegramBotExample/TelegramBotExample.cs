using CommandSetExample;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

// This simple telegram bot shows how the Friendly CLP library can be used to process a user input.
// For the sake of simplicity this bot is synchronously processing messages with one instance of
// CommandProcessor. Note that CommandProcessor is not thread safe and in a real world scenarios
// there should be a pool of CommandProcessors.

string APIToken;

if (args.Length == 1)
{
    APIToken = args[0];
}
else {
    Console.Write("Please provide a Telegram Bot API Token: ");
    APIToken = Console.ReadLine();
}

var bot = new TelegramBotClient(APIToken);

var receiverOptions = new ReceiverOptions { AllowedUpdates = new UpdateType[] { UpdateType.Message } };

var tokenSource = new CancellationTokenSource();

// Creating an instance of a Friendly CLP command set.
var myCommandSet = new CommandSet();

void handleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message.Type == MessageType.Text)
    {
        string request = update.Message.Text;
        Console.WriteLine(update.Message.From + ">>> " + request);
        string response;

        if (request == "/start")
            response = "Friendly command line processor test bot, type \"help\", to show command list!";
        else
            // Processing an incoming user command with a Friendly CLP command processor.
            response = myCommandSet.CommandProcessor.ProcessLine(request);

        Console.WriteLine(response);
        botClient.SendTextMessageAsync(update.Message.Chat, "`" + response + "`", ParseMode.MarkdownV2);
    }
}

void handlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine("Poll failed: " + exception.Message);
    tokenSource.Cancel();
}

bot.StartReceiving(handleUpdateAsync, handlePollingErrorAsync, receiverOptions, tokenSource.Token);

Console.WriteLine("Bot started, press any key to stop.");
Console.ReadKey();

tokenSource.Cancel();
tokenSource.Token.WaitHandle.WaitOne();
Console.WriteLine("Bot stopped.");