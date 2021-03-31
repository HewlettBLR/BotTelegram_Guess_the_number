using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace BotTelegram
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string token = File.ReadAllText("token");
            TelegramBotClient botClient = new TelegramBotClient(token);
            Console.WriteLine($"@ {botClient.GetMeAsync().Result.Username} start");

            int max = 5;
            Random rand = new Random();
            Dictionary<long, int> db = new Dictionary<long, int>();
            botClient.OnMessage += (s, arg) =>
            {
                #region var

                string msgText = arg.Message.Text;
                string firstname = arg.Message.Chat.FirstName;
                string replyMsg = string.Empty;
                int msgId = arg.Message.MessageId;
                long chatId = arg.Message.Chat.Id;
                int user = 0;
                string path = $"id_{chatId.ToString().Substring(0, 5).Substring(0, 5)}";
                bool skip = false;
                Console.WriteLine($"{firstname}:{msgText}");

                #endregion var

                if (!db.ContainsKey(chatId)
                || msgText == "/restart"
                || msgText.StartsWith("start")
                || msgText.ToLower().IndexOf("start") != -1
                )
                {
                    int startGame = rand.Next(20, 30);
                    db[chatId] = startGame;
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    skip = true;
                    replyMsg = $"Загадано число: {db[chatId]}";
                }
                else
                {
                    if (db[chatId] <= 0) return;
                    int.TryParse(msgText, out user);
                    if (!(user >= 1 && user <= max))
                    {
                        skip = true;
                        replyMsg = $@"Данные неверны! Число: {db[chatId]}";
                    }
                    if (!skip)
                    {
                        db[chatId] -= user;
                        replyMsg = $"Ход {firstname}:{user} Число: {db[chatId]}";
                        if (db[chatId] <= 0)
                        {
                            replyMsg = $"Победил игрок под ником {firstname}!";
                            skip = true;
                        }
                    }
                }
                if (!skip)
                {
                    int temp = rand.Next(max) + 1;
                    db[chatId] -= temp;
                    replyMsg += $"\nХод Бота: { temp} Число: {db[chatId]}";
                    if (db[chatId] <= 0) replyMsg = $"Победа Бота!";
                }
                Bitmap image = new Bitmap(400, 400);
                Graphics graphics = Graphics.FromImage(image);
                graphics.DrawImage(
                    Image.FromFile("img.bmp"),
                    x: 100,
                    y: 10);
                graphics.DrawString(
                    s: replyMsg,
                    font: new Font("Consolas", 16),
                    brush: Brushes.Blue,
                    x: 10,
                    y: 200);
                path += $@"\file_{DateTime.Now.Ticks}.bmp";
                image.Save(path);
                Console.WriteLine($" >>>{replyMsg}");
                botClient.SendPhotoAsync(
                    chatId: chatId,
                    caption: "LoremIpsum",
                    photo: new InputOnlineFile(new FileStream(path, FileMode.Open)),
                    replyToMessageId: msgId
                    );
            };
            botClient.StartReceiving();
            Console.ReadLine();
        }
    }
}