using MailKit.Net.Smtp;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmptClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Reading data from data.json...");

            var serialized = File.ReadAllText("data.json");

            Console.WriteLine("Parsing data...");

            var data = JsonConvert.DeserializeObject<DataInput>(serialized);

            Console.WriteLine("Loading common HTML and Text files...");

            var commonHtml = data.common.htmlFile != null ? File.ReadAllText(data.common.htmlFile) : null;
            var commonText = data.common.textFile != null ? File.ReadAllText(data.common.textFile) : null;

            foreach (var item in data.items)
            {
                Console.WriteLine("Processing " + item.toEmail);

                try
                {
                    Console.WriteLine("Building body...");

                    var builder = new BodyBuilder();
                    builder.HtmlBody = item.htmlFile != null ? File.ReadAllText(item.htmlFile) : commonHtml;
                    builder.TextBody = item.textFile != null ? File.ReadAllText(item.textFile) : commonText;

                    foreach (var attachmentFile in data.common.attachments.Union(item.attachments))
                    {
                        builder.Attachments.Add(attachmentFile);
                    }

                    Console.WriteLine("Building message...");

                    var message = new MimeMessage(
                        from: new[] { new MailboxAddress(
                            item.fromName ?? data.common.fromName,
                            item.fromEmail ?? data.common.fromEmail)
                        },
                        to: new[] { new MailboxAddress(
                            item.toName ?? data.common.toName,
                            item.toEmail ?? data.common.toEmail)
                        },
                        subject: item.subject ?? data.common.subject,
                        body: builder.ToMessageBody());

                    Console.WriteLine("Sending...");

                    var stopwatch = Stopwatch.StartNew();

                    using (var client = new SmtpClient())
                    {
                        client.Connect(data.server.host, data.server.port, false);
                        Console.WriteLine($"    Connected {stopwatch.ElapsedMilliseconds} ms");
                        client.Authenticate(data.server.username, data.server.apikey);
                        Console.WriteLine($"    Authenticated {stopwatch.ElapsedMilliseconds} ms");
                        client.Send(message);
                        Console.WriteLine($"    Sent {stopwatch.ElapsedMilliseconds} ms");
                        client.Disconnect(true);
                    }

                    Console.WriteLine($"Sent! in {stopwatch.ElapsedMilliseconds} ms");
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine(e);
                    Console.WriteLine();
                }
                System.Threading.Thread.Sleep(20);
            }

            Console.WriteLine("Press ENTER to continue . . .");
            Console.ReadLine();
        }
    }
}
