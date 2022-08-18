using System;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace EmailLicenceFile
{
    public class EmailLicenceFile
    {
        [FunctionName("EmailLicenceFile")]
        public void Run([BlobTrigger("licences/{name}",
            Connection = "AzureWebJobsStorage")]string licencesFileContents,
            [SendGrid(ApiKey ="SendGridApiKey")] out SendGridMessage message,
            string name,
            ILogger log)
        {
            var email = Regex.Match(licencesFileContents, pattern: @"^Email\:\ (.+)$", RegexOptions.Multiline).Groups[1].Value;
            log.LogInformation($"Got order from {email}\n Licence file Name:{name}");

            message = new SendGridMessage();
            message.From = new EmailAddress(Environment.GetEnvironmentVariable("EmailSender"));
            message.AddTo(email);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(licencesFileContents);
            var base64 = Convert.ToBase64String(plainTextBytes);
            message.AddAttachment(filename: name, base64Content: base64, type: "text/plain");
            message.Subject = "Your licence file";
            message.HtmlContent = "Thank you for your order";
        }
    }
}
