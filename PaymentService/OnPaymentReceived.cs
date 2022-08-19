using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentService.Models;

namespace PaymentService
{
    public static class OnPaymentReceived
    {
        [FunctionName("OnPaymentReceived")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Queue("orders")] IAsyncCollector<Order> orderQueue,
            [Table("orders")] IAsyncCollector<Order> orderTable,
            ILogger log
            )
        {
            log.LogInformation("We received a payment.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject<Order>(requestBody);

            await orderQueue.AddAsync(order);

            order.PartitionKey = "orders";
            order.RowKey = order.OrderId;
            await orderTable.AddAsync(order);

            log.LogInformation($"Order {order.OrderId} received from {order.Email} for product {order.ProductId}");

            var responseMessage = $"Thank you for you payment";

            return new OkObjectResult(responseMessage);
        }
    }
}
