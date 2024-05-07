using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using ContactProvider.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ContactProvider.Services
{
    public class Contact(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;


        [Function("CreateContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contacts")] [FromBody] ContactEntity entity)
        {
            try
            {
                ////var body = await new StreamReader(req.Body).ReadToEndAsync();
                ////var entity = JsonConvert.DeserializeObject<ContactEntity>(body);

                //if (entity == null || entity.FullName == null || entity.Email == null || entity.Message == null)
                //    return new BadRequestResult();
                //else
                //{
                //    using var context = _serviceProvider.GetRequiredService<DataContext>();
                //    context.Contacts.Add(entity);
                //    await context.SaveChangesAsync();

                //    var emailRequest = new
                //    {
                //        to = entity.Email,
                //        subject = "Contact Request Received",
                //        htmlBody = $"Hello {entity.FullName},<br/><br/>Your contact request has been received. We will get back to you shortly.<br/><br/>Regards,<br/>Silicon",
                //        plainText = $"Hello {entity.FullName},\n\nYour contact request has been received. We will get back to you shortly.\n\nRegards,\nSilicon"
                //    };

                //    var sender = _client.CreateSender("email_request");
                //    var message = new ServiceBusMessage(JsonConvert.SerializeObject(emailRequest));
                //    await sender.SendMessageAsync(message);

                var contactService = _serviceProvider.GetRequiredService<ContactService>();

                var contact = await contactService.CreateContact(entity);
                if (!contact)
                    return new BadRequestResult();

                var email = await contactService.SendEmail(entity.Email, entity.FullName);
                if (!email)
                    return new StatusCodeResult(500);


                return new OkObjectResult("Contact request saved successfully.");
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR : ContactProvider.Contact.cs :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
