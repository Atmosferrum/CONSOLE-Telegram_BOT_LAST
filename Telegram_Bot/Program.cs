using System;
using System.Net;
using System.IO;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;
using System.Collections.ObjectModel;
using System.Diagnostics;

using static System.Console;

namespace Telegram_Bot
{
    public class Program
    {
        static TelegramBotClient bot;

        static void Main(string[] args)
        {
            string token = File.ReadAllText(@"TelegramBotToken.txt");

            #region Proxy;

            var proxy = new WebProxy()
            {
                Address = new Uri($"http://109.173.26.83:1080"),
                UseDefaultCredentials = false,
            };

            var httpClientHandler = new HttpClientHandler() { Proxy = proxy };

            HttpClient httpClient = new HttpClient(httpClientHandler);

            bot = new TelegramBotClient(token, httpClient);

            #endregion Proxy

            bot = new TelegramBotClient(token);

            bot.OnMessage += MessageListener;
            bot.StartReceiving();
            ReadKey();
        }

        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs args)
        {
            string text = $"\n{DateTime.Now.ToShortDateString()}" +
                          $"\n{args.Message.Chat.FirstName}" +
                          $"\n{args.Message.Chat.Id}" +
                          $"\n{args.Message.Text}";

            WriteLine($"{text} " +
                      $"\nMessage type : {args.Message.Type.ToString()}");

            if (args.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                WriteLine($"\n{args.Message.Document.FileName}" +                         
                          $"\n{args.Message.Document.FileSize}" +
                          $"\n{args.Message.Document.MimeType}");

                DownloadDocument(args.Message.Chat.Id,
                                 args.Message.Document.FileId,
                                 args.Message.Document.FileName,
                                 args.Message.Document.MimeType);  
            }
        }

        static async void DownloadDocument(long chatID, string fileID, string fileName, string extention)
        {
            var file = await bot.GetFileAsync(fileID);

            FileStream fileStream = new FileStream($"{fileName}", 
                                                    FileMode.Create);

            await bot.DownloadFileAsync(file.FilePath,
                                        fileStream);

            fileStream.Close();
            fileStream.Dispose();

            UploadDocument(chatID,
                           $@"{fileName}");
        }

        static string Extentioneer(string extention)
        {
            int indexOfFullStop = extention.LastIndexOf("/");
            return $".{extention.Substring(extention.Length - indexOfFullStop + 2)}";
        }

        static async void UploadDocument(long chatID, string filePath)
        {
            FileStream fileStream = File.OpenRead(filePath);

            InputOnlineFile inputOnlineFile = new InputOnlineFile(fileStream, filePath);
            await bot.SendDocumentAsync(chatID, inputOnlineFile);

            fileStream.Close();
            fileStream.Dispose();
        }
    }
}
