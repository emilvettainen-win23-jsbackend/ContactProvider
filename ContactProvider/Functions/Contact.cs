using Azure;
using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using ContactProvider.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ContactProvider.Services
{
    public class Contact(ServiceBusClient client)
    {
        private readonly ServiceBusClient _client = client;

        //[Function("Contact")]
        //public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] HttpRequest req)
        //{
        //    var body = await new StreamReader(req.Body).ReadToEndAsync();
        //    var entity = JsonConvert.DeserializeObject<ContactEntity>(body);
        //    if (entity == null || entity.FullName == null || entity.Email == null || entity.Message == null)
        //        return new BadRequestResult();
        //    else
        //    {
        //        var sender = _client.CreateSender("contacts_create");
        //        var receiver = _client.CreateReceiver("contacts_create_response");

        //        try
        //        {
        //            var message = new ServiceBusMessage(JsonConvert.SerializeObject(entity))
        //            {
        //                ContentType = "application/json",
        //                CorrelationId = Guid.NewGuid().ToString(),
        //            };

        //            await sender.SendMessageAsync(message);

        //            ServiceBusReceivedMessage response = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));
        //            if (response != null)
        //            {
        //                await receiver.CompleteMessageAsync(response);
        //                return new OkObjectResult(JsonConvert.DeserializeObject<ContactEntity>(response.Body.ToString()));
        //            }
        //            else
        //            {
        //                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //            }
        //        }
        //        catch
        //        {
        //            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        //        }
        //        finally
        //        {
        //            await sender.DisposeAsync();
        //            await receiver.DisposeAsync();
        //        }
        //    }
        //}



        [Function("CreateContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] HttpRequest req,
        DataContext context)
        {
            try
            {
                var body = await new StreamReader(req.Body).ReadToEndAsync();
                var entity = JsonConvert.DeserializeObject<ContactEntity>(body);

                if (entity == null || entity.FullName == null || entity.Email == null || entity.Message == null)
                    return new BadRequestResult();
                else
                {
                    context.Contacts.Add(entity);
                    await context.SaveChangesAsync();

                    var emailRequest = new
                    {
                        to = entity.Email,
                        subject = "Contact Request Received",
                        htmlBody = $"Hello {entity.FullName},<br/><br/>Your contact request has been received. We will get back to you shortly.<br/><br/>Regards,<br/>Silicon",
                        plainText = $"Hello {entity.FullName},\n\nYour contact request has been received. We will get back to you shortly.\n\nRegards,\nSilicon"
                    };

                    var sender = _client.CreateSender("email_request");
                    var message = new ServiceBusMessage(JsonConvert.SerializeObject(emailRequest));
                    await sender.SendMessageAsync(message);

                    return new OkObjectResult("Contact request saved successfully.");
                }
            }
            catch 
            {
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
