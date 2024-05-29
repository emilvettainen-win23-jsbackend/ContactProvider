using ContactProvider.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ContactProvider.Services
{
    public class Contact(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;


        [Function("CreateContact")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "contacts")] [FromBody] ContactEntity entity)
        {
            try
            {
                var contactService = _serviceProvider.GetRequiredService<ContactService>();

                var contact = await contactService.CreateContact(entity);
                if (!contact)
                    return new BadRequestResult();

                var email = await contactService.SendEmail(entity.Email, entity.FullName);
                if (!email)
                    return new StatusCodeResult(500);


                return new OkObjectResult("Contact request saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR : ContactProvider.Contact.cs :: {ex.Message}");
                return new StatusCodeResult(500);
            }
        }
    }
}
