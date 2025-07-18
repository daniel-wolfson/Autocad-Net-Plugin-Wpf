using ID.Api.Enums;
using ID.Api.Extensions;
using ID.Api.Interfaces;
using ID.Api.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ID.Services
{
    public class SendService : ISendService
    {
        private readonly IEmailConfiguration _emailConfig;
        private readonly ISmsConfiguration _smsConfig;
        private readonly ILogger _logger;

        public SendService(IEmailConfiguration emailConfiguration, ISmsConfiguration smsConfiguration, ILogger logger)
        {
            _emailConfig = emailConfiguration;
            _smsConfig = smsConfiguration;
            _logger = logger;
        }

        public async Task<ObjectResult> SendSmsAsync(string name, string mobile, string message)
        {
            ObjectResult actionResult = new ObjectResult(new { statusCode = HttpStatusCode.NotImplemented });

            // send sms detailed async
            //var SendSMS = new SendMessageSoapClient(EndpointConfiguration.SendMessageSoap12);
            try
            {
                //    var result = await SendSMS.SendSmsDetailedAsync(
                //        _smsConfig.UserName,
                //        _smsConfig.Password,
                //        message ?? "Test send to candidate",
                //        "", // message Peleohone 
                //        "", // message Cellcom
                //        "", // message Partner
                //        "", // message Mirs
                //        string.IsNullOrEmpty(_smsConfig.AdminMobile) ? mobile : _smsConfig.AdminMobile,
                //        "", // custom parameter
                //        "", // custom message id
                //        0, // message interval
                //        DateTime.Now,
                //        _smsConfig.Company,
                //        _smsConfig.Sender, //Sender number
                //        0
                //    );

                //    string displayMessage = "sent email successfuly";
                //    actionResult = new ObjectResult(displayMessage) { StatusCode = (int?)HttpStatusCode.Accepted };
                //    _logger.Information($"{displayMessage}; {result.ToString()}");
            }
            catch (Exception ex)
            {
                string displayMessage = ex?.InnerException?.Message;
                actionResult = new ObjectResult(displayMessage) { StatusCode = (int?)HttpStatusCode.Conflict };
                _logger.Error(displayMessage);
            }
            finally
            {
                //await SendSMS.CloseAsync();
            }

            return actionResult;
        }

        public async Task<ObjectResult> SendEmailAsync(eXmlEmailTemplates sendTemplate, SendMessageData data)
        {
            _logger.Information($"{nameof(SendEmailAsync)} method called!");
            string xml = GetServiceCenterXmlTemplate(sendTemplate, data);
            return await SendEmailAsync(xml);
        }

        public async Task<ObjectResult> SendEmailAsync(eXmlEmailTemplates sendTemplate, List<PsygateCandidate> candidates)
        {
            _logger.Information($"{nameof(SendEmailAsync)} method called!");

            if (_emailConfig.InfoMailSendingTimeInterval > 0)
            {
                Dictionary<int, ObjectResult> sendListResults = new Dictionary<int, ObjectResult>();
                foreach (var candidate in candidates)
                {
                    string xml = GetCandidateXmlTemplate(sendTemplate, new List<PsygateCandidate>() { candidate });
                    Thread.Sleep(1000 * _emailConfig.InfoMailSendingTimeInterval);
                    var result = await SendEmailAsync(xml);
                    sendListResults[(int)candidate.ID_NUMBER] = result;
                }

                var errorList = sendListResults.Where(x => !x.Value.IsSuccessStatusCode())
                .Select(result => $"{result.Key}: {result.Value.Value.ToString()}").ToArray();

                var successList = sendListResults.Where(x => x.Value.IsSuccessStatusCode())
                    .Select(result => $"{result.Key}: {result.Value.Value.ToString()}").ToArray();

                var logErrorResultMessages = (errorList.Length > 1) ? string.Join("; ", errorList) : errorList.FirstOrDefault();
                var logSuccessResultMessages = (successList.Length > 1) ? string.Join("; ", successList) : successList.FirstOrDefault();

                var interval = $"with interval:{_emailConfig.InfoMailSendingTimeInterval}";
                _logger.Error($"{nameof(SendEmailAsync)}({interval}) error list: {(string.IsNullOrEmpty(logErrorResultMessages) ? "none" : logErrorResultMessages)}");
                _logger.Information($"{nameof(SendEmailAsync)}({interval}) success list: {(string.IsNullOrEmpty(logSuccessResultMessages) ? "none" : logSuccessResultMessages)}");

                return new ObjectResult((logSuccessResultMessages ?? "") + (logErrorResultMessages ?? ""))
                {
                    StatusCode = (int)HttpStatusCode.MultiStatus
                };
            }
            else
            {
                string xml = GetCandidateXmlTemplate(sendTemplate, candidates);
                return await SendEmailAsync(xml);
            }
        }

        private async Task<ObjectResult> SendEmailAsync(string xml)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    // get to mail service
                    var urlString = _emailConfig.InfoMailServiceUrl;

                    var parameters = new Dictionary<string, string> { { "xml", xml } };
                    var encodedContent = new FormUrlEncodedContent(parameters);

                    HttpResponseMessage response = await httpClient.PostAsync(urlString, encodedContent);
                    if (response.IsSuccessStatusCode)
                    {
                        // read data as string and get deserialized result
                        string xmlResult = await response.Content.ReadAsStringAsync();
                        XDocument xdocResponse = XDocument.Parse(xmlResult);
                        XElement status = xdocResponse.XPathSelectElement("//InfoMailResponse//SendEmails//Status");

                        if (!status.Value.Contains("Success"))
                        {
                            var displayMessage = $"{nameof(SendEmailAsync)}; {_emailConfig.InfoMailServiceUrl} response status {status}";
                            _logger.Error($"{nameof(SendEmailAsync)} action: {displayMessage}");
                            return new ObjectResult(displayMessage);
                        }

                        return new ObjectResult("sent successfully") { StatusCode = (int)HttpStatusCode.Accepted };
                    }
                    else
                    {
                        var displayMessage = $"StatusCode {Enum.GetName(typeof(HttpStatusCode), response.StatusCode)} is not valid";
                        _logger.Error($"{nameof(SendEmailAsync)} Candidates action: {displayMessage}");
                        return new ObjectResult(displayMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"{nameof(SendEmailAsync)} Candidates action: {ex.InnerException?.Message}");
                    return new ObjectResult(ex.InnerException?.Message);
                }
            }
        }

        private string GetServiceCenterXmlTemplate(eXmlEmailTemplates sendTemplate, SendMessageData data)
        {
            XDocument xdoc = GetXmlTemplate(sendTemplate, data.Name, data.Email);

            // add recipients
            var recipients = xdoc.XPathSelectElement("//InfoMailClient//Recipients");
            XElement root = new XElement("Email");
            root.Add(new XAttribute("address", _emailConfig.AdminEmailAddressServiceCenter));
            root.Add(new XAttribute("var1", data.Message));
            root.Add(new XAttribute("var2", data.Name));
            recipients.Add(root);

            return xdoc.ToString();
        }

        private string GetCandidateXmlTemplate(eXmlEmailTemplates sendTemplate, List<PsygateCandidate> candidates)
        {
            XDocument xdoc = GetXmlTemplate(sendTemplate);

            // recipients
            var recipients = xdoc.XPathSelectElement("//InfoMailClient//Recipients");
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate.EMAIL))
                {
                    XElement root = new XElement("Email");
                    root.Add(new XAttribute("address", string.IsNullOrEmpty(_emailConfig.AdminEmailAddressTo) ? candidate.EMAIL : _emailConfig.AdminEmailAddressTo));
                    root.Add(new XAttribute("var1", candidate.FIRST_NAME));
                    root.Add(new XAttribute("var2", candidate.TEMPPASS));
                    recipients.Add(root);
                }
            }

            return xdoc.ToString();
        }

        private XDocument GetXmlTemplate(eXmlEmailTemplates xmlEmailtemplate, string name = null, string addressFrom = null, string subject = null)
        {
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                $@"Resources/{xmlEmailtemplate.ToString() + "Emailtemplate"}.xml");

            XDocument xdoc = XDocument.Load(fileName);

            // user
            XElement username = xdoc.XPathSelectElement("//SendEmails//User//Username");
            username.Value = _emailConfig.InfoMailUsername;
            XElement token = xdoc.XPathSelectElement("//SendEmails//User//Token");
            token.Value = _emailConfig.InfoMailToken;

            // email message
            XElement campaignName = xdoc.XPathSelectElement("//SendEmails//Message//CampaignName");
            campaignName.Value = _emailConfig.InfoMailCampaignName;
            XElement messageSubject = xdoc.XPathSelectElement("//SendEmails//Message//Subject");
            messageSubject.Value = subject ?? messageSubject.Value;
            XElement fromAddress = xdoc.XPathSelectElement("//SendEmails//Message//FromAddress");

            fromAddress.Value = addressFrom ??
                (!string.IsNullOrEmpty(_emailConfig.InfoMailFromAddress)
                    ? _emailConfig.InfoMailFromAddress
                    : fromAddress.Value);

            XElement fromName = xdoc.XPathSelectElement("//SendEmails//Message//FromName");
            fromName.Value = name ?? _emailConfig.InfoMailFromName;

            // send datetime
            XElement scheduledSending = xdoc.XPathSelectElement("//SendEmails//Message//ScheduledSending");
            if (scheduledSending != null && !string.IsNullOrEmpty(_emailConfig.InfoMailScheduledSendingTime))
            {
                DateTime dateTime = DateTime.Now;
                int[] hoursMinutesSeconds = _emailConfig.InfoMailScheduledSendingTime.Split(":").Select(x => int.Parse(x)).ToArray();
                dateTime = new DateTime(
                    dateTime.Year,
                    dateTime.Month,
                    dateTime.Day,
                    hoursMinutesSeconds[0],
                    hoursMinutesSeconds[1],
                    hoursMinutesSeconds[2],
                    0, dateTime.Kind);
                scheduledSending.Value = dateTime.ToString("yyy-MM-dd HH:mm:ss");
            }
            return xdoc;
        }

        // TODO: delete?
        // Send email async
        public async Task<ObjectResult> SendEmailSmtpAsync(string name, string subject, string addressFrom, string addressTo, string message)
        {
            // return result
            ObjectResult actionResult = new ObjectResult("NotImplemented") { StatusCode = (int?)HttpStatusCode.NotImplemented };

            // create email message
            var emailMessage = new MimeMessage
            {
                Sender = new MailboxAddress(subject, _emailConfig.SmtpUsername),
                Subject = subject,
                Body = new TextPart(TextFormat.Html) { Text = message }
            };

            emailMessage.From.Add(new MailboxAddress(subject, _emailConfig.AdminEmailAddressFrom));
            emailMessage.To.Add(new MailboxAddress(name, addressTo));

            // send email async
            using (var emailClient = new SmtpClient())
            {
                var timeout = TimeSpan.FromMinutes(_emailConfig.AdminTimeout);
                using (var cancellationTokenSource = new CancellationTokenSource(timeout))
                {
                    CancellationToken cancellationToken = cancellationTokenSource.Token;
                    try
                    {
                        emailClient.SslProtocols = System.Security.Authentication.SslProtocols.Tls;
                        await emailClient.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.SmtpPort, false);
                        emailClient.AuthenticationMechanisms.Remove("LOGIN");
                        await emailClient.AuthenticateAsync(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword);
                        await emailClient.SendAsync(emailMessage, cancellationToken);

                        string displayMessage;
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            displayMessage = "sent email successfuly";
                            actionResult = new ObjectResult(displayMessage) { StatusCode = (int?)HttpStatusCode.Accepted };
                            _logger.Information($"{nameof(SendService)} {displayMessage} to email: {addressTo}");
                        }
                        else
                        {
                            displayMessage = $"sent email error to: {addressTo}";
                            actionResult = new ObjectResult(displayMessage) { StatusCode = (int?)HttpStatusCode.Conflict };
                            _logger.Error($"{nameof(SendService)} {displayMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        HttpStatusCode httpStatusCode = !string.IsNullOrEmpty(ex.Message) && ex.Message.Contains("timeout")
                            ? HttpStatusCode.RequestTimeout : HttpStatusCode.InternalServerError;

                        string displayMessage = "sent email error";
                        actionResult = new ObjectResult(displayMessage) { StatusCode = (int?)HttpStatusCode.Conflict }; ;
                        _logger.Error($"{nameof(SendService)} {displayMessage} to email: {addressTo}");
                    }
                    finally
                    {
                        await emailClient.DisconnectAsync(true);
                    }
                }
                return actionResult;
            }
        }

    }
}
