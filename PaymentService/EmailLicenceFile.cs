using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using SendGrid.Helpers.Mail;
using System;

namespace PaymentService
{
    public class EmailLicenceFile
    {
        [FunctionName("EmailLicenceFile")]
        public void Run([BlobTrigger("licences/{orderId}.lic",
            Connection = "AzureWebJobsStorage")]string licencesFileContents,
            [SendGrid(ApiKey = "SendGridApiKey")] ICollector<SendGridMessage> sender,
            [Table("orders", "orders", "{orderId}")] Order order,
            string orderId,
            ILogger log)
        {
            //var email = Regex.Match(licencesFileContents, pattern: @"^Email\:\ (.+)$", RegexOptions.Multiline).Groups[1].Value;
            var email = order.Email;
            log.LogInformation($"Got order from {email}\n Licence file Name:{orderId}");

            var message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(email);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(licencesFileContents);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment(filename: orderId, base64Content: base64, type: "text/plain");
            message.Subject = "Your licence file";
            message.HtmlContent = "Thank you for your order";

            if (!email.EndsWith("@test.com"))
            {
                sender.Add(message);
            }
        }
    }
}
