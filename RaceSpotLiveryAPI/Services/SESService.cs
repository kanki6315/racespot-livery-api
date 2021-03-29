using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using HandlebarsDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RaceSpotLiveryAPI.Entities;
using RaceSpotLiveryAPI.Utils;

namespace RaceSpotLiveryAPI.Services
{
    public class SESService : ISESService
    {
        private readonly string _senderAddress;
        private readonly IAmazonSimpleEmailService _client;

        private ILogger<SESService> _logger;

        public SESService(IConfiguration config, ILogger<SESService> logger)
        {
            var accessKey = config["S3.AccessKey"];
            var secretKey = config["S3.SecretKey"];
            _senderAddress = config["SES.SenderEmail"];
            
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(config["S3.BucketRegion"]));
            _logger = logger;
        }
        
        public async Task SendUpdateEmailSettingsNotification(ApplicationUser user)
        {
            var htmlTemplateObj = Handlebars.Compile(EmailTemplates.emailUpdateHTMLTemplate);
            var textTemplateObj = Handlebars.Compile(EmailTemplates.emailUpdateTextTemplate);
            var data = new {
                actionTitle = user.IsAgreedToEmails ? "Enabled" : "Disabled",
                name = user.FirstName + ' ' + user.LastName,
                actionBody = user.IsAgreedToEmails ? "enabled" : "disabled",
                receiveText = user.IsAgreedToEmails ? "receive" : "not receive",
                restartActionOne = user.IsAgreedToEmails ? "disable" : "re-enable",
                restartActionTwo = user.IsAgreedToEmails ? "disable" : "enable",
            };
            var htmlBody = htmlTemplateObj(data);
            var textBody = textTemplateObj(data);
            SendEmail(user.Email, "RaceSpot.Media Email Notification Change", htmlBody, textBody);
        }
        
        public async Task SendRejectionEmail(Livery livery)
        {
            var htmlTemplateObj = Handlebars.Compile(EmailTemplates.paintUpdateHTMLTemplate);
            var textTemplateObj = Handlebars.Compile(EmailTemplates.paintUpdateTextTemplate);
            var data = new {
                seriesName = livery.Series.Name,
                name = livery.User.FirstName + ' ' + livery.User.LastName
            };
            var htmlBody = htmlTemplateObj(data);
            var textBody = textTemplateObj(data);
            SendEmail(livery.User.Email, "RaceSpot.Media Paint Rejection", htmlBody, textBody);
        }

        private async void SendEmail(string receiverAddress, string subject, string htmlBody, string textBody)
        {
            var sendRequest = new SendEmailRequest
            {
                Source = _senderAddress,
                Destination = new Destination
                {
                    ToAddresses =
                        new List<string> {receiverAddress}
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = htmlBody
                        },
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = textBody
                        }
                    }
                }
            };
            try
            {
                _logger.LogInformation("Sending email using Amazon SES to " + receiverAddress);
                var response = await _client.SendEmailAsync(sendRequest);
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("The email was sent successfully.");
                }
                else
                {
                    throw new Exception("There was an error sending update email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("The email was not sent.");
                _logger.LogError("Error message: " + ex.Message);
                throw;
            }
        }

    }
}