using Azure.Messaging.ServiceBus;
using ContactProvider.Contexts;
using ContactProvider.Entities;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ContactProvider.Services;

public class ContactService(IServiceProvider serviceProvider, ServiceBusClient client)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ServiceBusClient _client = client;

    public async Task<bool> CreateContact(ContactEntity entity)
    {
        try
        {
            if (entity == null || entity.FullName == null || entity.Email == null || entity.Message == null)
                return false;

            using var context = _serviceProvider.GetRequiredService<DataContext>();
            context.Contacts.Add(entity);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR : ContactProvider.ContactService.cs :: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEmail(string email, string fullName)
    {
        try
        {
            var emailRequest = new
            {
                to = email,
                subject = "Contact Request Received",
                htmlBody = $"Hello {fullName},<br/><br/>Your contact request has been received. We will get back to you shortly.<br/><br/>Regards,<br/>Silicon",
                plainText = $"Hello {fullName},\n\nYour contact request has been received. We will get back to you shortly.\n\nRegards,\nSilicon"
            };

            var sender = _client.CreateSender("email_request");
            var message = new ServiceBusMessage(JsonConvert.SerializeObject(emailRequest));
            await sender.SendMessageAsync(message);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR : ContactProvider.ContactService.cs :: {ex.Message}");
            return false;
        }
    }
}
