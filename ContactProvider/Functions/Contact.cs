using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using ContactProvider.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ContactProvider.Services
{
    public class Contact(IServiceProvider serviceProvider, ServiceBusClient client)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ServiceBusClient _client = client;


        [Function("CreateContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] [FromBody] ContactEntity entity)
        {
            try
            {
                //var body = await new StreamReader(req.Body).ReadToEndAsync();
                //var entity = JsonConvert.DeserializeObject<ContactEntity>(body);

                if (entity == null || entity.FullName == null || entity.Email == null || entity.Message == null)
                    return new BadRequestResult();
                else
                {
                    using var context = _serviceProvider.GetRequiredService<DataContext>();
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
