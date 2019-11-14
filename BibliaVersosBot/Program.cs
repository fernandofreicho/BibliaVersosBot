using BibliaVersosBot.Models;
using Dapper;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BibliaVersosBot
{
    class Program
    {
        static ITelegramBotClient botClient;
        public static string Versao { get; set; }

        static void Main()
        {
            Versao = "nvi";
            //dev key
            botClient = new TelegramBotClient("752423713:AAEFZ_l0L14yP5MRJQ3aAXdYVEgiSj7Bh5Q");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.OnCallbackQuery += BotOnCallbackQueryReceived;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type == ChatType.Group)
                if (!e.Message.Text.Contains("@BibliaVersos"))
                    return;


            
            if (e.Message.Text != null)
            {
                if (e.Message.Text.StartsWith('/'))
                {
                    Console.WriteLine($"Command:{e.Message.Text}");
                    switch (e.Message.Text.Split('@').First())
                    {
                        // send inline keyboard
                        case "/biblia":
                            await botClient.SendChatActionAsync(e.Message.Chat.Id, ChatAction.Typing);

                            await Task.Delay(500); // simulate longer running task

                            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                            {
                                new [] // first row
                                {
                                    InlineKeyboardButton.WithCallbackData("NVI"),
                                    InlineKeyboardButton.WithCallbackData("ACF"),
                                }
                            });

                            await botClient.SendTextMessageAsync(
                                e.Message.Chat.Id,
                                "Escolha a versão",
                                replyMarkup: inlineKeyboard);
                            break;

                        default:
                            break;
                    }

                }
                else
                {
                    string receivedMessage = e.Message.Text;
                    if (e.Message.Chat.Type == ChatType.Group)
                        receivedMessage = string.Join(" ", receivedMessage.Split(' ').Skip(1).ToArray());

                    Console.WriteLine($"Message:{receivedMessage}");
                    Random random = new Random();
                    var id = random.Next(1, 31062).ToString();
                    if (Versao == "acf")
                        id = random.Next(31063, 62126).ToString();

                    Verse result = null;
                    bool aleatorio = false;
                    using (var cnn = new SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "/db/biblia.db"))
                    {
                        cnn.Open();
                        string sql = $"SELECT * FROM Verses v INNER JOIN Books b on v.BookId = b.id  WHERE v.version = '{Versao}' AND v.Text like @Texto ORDER BY RANDOM()";
                        string sql2 = $"SELECT * FROM Verses v INNER JOIN Books b on v.BookId = b.id  WHERE v.version = '{Versao}' AND v.Id = @Id";
    
                        result = cnn.Query<Verse, Book, Verse>(sql, (v, b) => { v.Book = b; return v; }, new { @Texto = "% " + receivedMessage + " %" }).FirstOrDefault();
                        if (result == null)
                        {
                            result = cnn.Query<Verse, Book, Verse>(sql2, (v, b) => { v.Book = b; return v; }, new { @Id = id }).FirstOrDefault();
                            aleatorio = true;
                        }

                    }

                    string message = result.Text.Replace(receivedMessage, "</i><b> " + receivedMessage + "</b><i>");

                    Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");

                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.Chat.Id,
                      text: string.Format("{0}\n<i>{1} ({2} {3}:{4} ({5}) </i>)", aleatorio ? "Não encontrato, retornado aleatório:" : "", message, result.Book.Abbrev, result.Chapter, result.VerseNumber, Versao),
                      parseMode: ParseMode.Html);
                }
            }
        }
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id,$"Selecionado {callbackQuery.Data}");
            Versao = callbackQuery.Data.ToLower();

        }
    }
}

