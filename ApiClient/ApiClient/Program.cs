using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavis.UriTemplates;

namespace ApiClient
{
    class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult(); // to avoid exceptions being wrapped into AggregateException
        }

        static async Task MainAsync()
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
                    Console.WriteLine("Loading files...");

                    var htmlBody = item.htmlFile != null ? File.ReadAllText(item.htmlFile) : commonHtml;
                    var textBody = item.textFile != null ? File.ReadAllText(item.textFile) : commonText;

                    var fileNames = data.common.attachments.Union(item.attachments);
                    var attachments = fileNames.Select(x => new
                    {
                        filename = x,
                        base64_content = Convert.ToBase64String(File.ReadAllBytes(x)),
                        type = GetContentTypeByExtension(x)
                    })
                    .ToArray();

                    Console.WriteLine("Preparing data and client...");

                    var template = new UriTemplate(data.server.sendMessageUrlTemplate);
                    template.AddParameters(new
                    {
                        accountId = data.server.accountId,
                        accountName = data.server.accountName
                    });

                    var client = new FlurlClient(template.Resolve()).WithOAuthBearerToken(data.server.apikey);

                    var requestBody = new
                    {
                        from_name = item.fromName ?? data.common.fromName,
                        from_email = item.fromEmail ?? data.common.fromEmail,
                        recipients = new[]
                        {
                            new
                            {
                                email = item.toEmail ?? data.common.toEmail,
                                name = item.toName ?? data.common.toName,
                                type = "to"
                            }
                        },
                        subject = item.subject ?? data.common.subject,
                        html = htmlBody,
                        text = textBody,
                        attachments = attachments
                    };

                    Console.WriteLine("Sending...");

                    var stopwatch = Stopwatch.StartNew();

                    await client.PostJsonAsync(requestBody);

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

        private static string GetContentTypeByExtension(string filename)
        {
            // It is not complete, if you already knows what the content type is, avoid auto-detection
            switch (Path.GetExtension(filename))
            {
                case ".zip": return "application/x-zip-compressed";
                case ".mp3": return "audio/mp3";
                case ".gif": return "image/gif";
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".htm": return "text/html";
                case ".html": return "text/html";
                case ".txt": return "text/plain";
                case ".xml": return "text/xml";
                case ".pdf": return "application/pdf";
                default: return "application/octet-stream";
            }
        }
    }
}
