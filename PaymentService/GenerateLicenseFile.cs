using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PaymentService.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PaymentService
{
    public class GenerateLicenseFile
    {
        [FunctionName("GenerateLicenseFile")]
        public async Task Run(
            [QueueTrigger("orders", Connection = "AzureWebJobsStorage")] Order order,
            IBinder binder,
            //[Blob("licences/{rand-guid}.lic")] TextWriter outputBlob,
            ILogger log)
        {
            var outputBlob = await binder.BindAsync<TextWriter>(new BlobAttribute(blobPath: $"licences/{order.OrderId}.lic")
            {
                Connection = "AzureWebJobsStorage",
            });

            outputBlob.WriteLine($"OrderId: {order.OrderId}");
            outputBlob.WriteLine($"Email: {order.Email}");
            outputBlob.WriteLine($"ProductId: {order.ProductId}");
            outputBlob.WriteLine($"PurchaseDate: {DateTime.Now}");

            var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(order.Email + "secret"));

            outputBlob.WriteLine($"SecretCode: {BitConverter.ToString(hash).Replace("-", "")}");

        }
    }
}
