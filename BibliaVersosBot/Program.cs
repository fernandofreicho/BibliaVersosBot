using BibliaVersosBot.Models;
using Dapper;
using System;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace BibliaVersosBot
{
    class Program
    {
        static ITelegramBotClient botClient;

        static void Main()
        {
            //dev key
            botClient = new TelegramBotClient("752423713:AAEFZ_l0L14yP5MRJQ3aAXdYVEgiSj7Bh5Q");

            var me = botClient.GetMeAsync().Result;
            Console.WriteLine(
              $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            Thread.Sleep(int.MaxValue);
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                if (e.Message.Text.StartsWith('/'))
                { }
                else
                {
                    Console.WriteLine($"Message:{e.Message.Text}");
                    Random random = new Random();
                    var id = random.Next(1, 31062).ToString();

                    Verse result = null;
                    bool aleatorio = false;
                    using (var cnn = new SQLiteConnection("Data Source=" + Environment.CurrentDirectory + "\\db\\biblia.db"))
                    {
                        cnn.Open();
                        string sql = "SELECT * FROM Verses v INNER JOIN Books b on v.BookId = b.id  WHERE v.version = 'nvi' AND v.Text like @Texto ORDER BY RANDOM()";
                        string sql2 = "SELECT * FROM Verses v INNER JOIN Books b on v.BookId = b.id  WHERE v.version = 'nvi' AND v.Id = @Id";

                        result = cnn.Query<Verse, Book, Verse>(sql, (v, b) => { v.Book = b; return v; }, new { Texto = "% " + e.Message.Text + " %" }).FirstOrDefault();
                        if (result == null)
                        {
                            result = cnn.Query<Verse, Book, Verse>(sql2, (v, b) => { v.Book = b; return v; }, new { Id = id }).FirstOrDefault();
                            aleatorio = true;
                        }

                    }

                    string message = result.Text.Replace(e.Message.Text, "</i><b> " + e.Message.Text + "</b><i>");

                    Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");

                    await botClient.SendTextMessageAsync(
                      chatId: e.Message.Chat.Id,
                      text: string.Format("{0}\n<i>{1} ({2} {3}:{4}</i>)", aleatorio ? "Não encontrato, retornado aleatório:" : "", message, result.Book.Abbrev, result.Chapter, result.VerseNumber),
                      parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }
            }
        }
    }
}

