using ID.Api.Enums;
using ID.Api.Extensions;
using ID.Api.Interfaces;
using ID.Api.Models;
using ID.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ID.Api.Controllers
{
    [Route("/api/")]
    [ApiController]
    [Authorize]
    public class SendController : ControllerBase
    {
        private IMemoryCache _cache;
        private readonly ILogger _logger;
        private readonly MapitContext _context;
        private readonly IOptions<GeneralSettings> _appSettings;
        private readonly IEmailConfiguration _emailConfig;
        private readonly ISmsConfiguration _smsConfig;
        private readonly ISendService _sendService;
        private readonly IUserService _userService;

        public SendController(ILogger logger, IMemoryCache memoryCache,
            ISendService emailService, IUserService userService,
            MapitContext context,
            IOptions<GeneralSettings> appSettings,
            IEmailConfiguration emailConfig, ISmsConfiguration smsConfig
            )
        {
            _logger = logger;
            _cache = memoryCache;
            _context = context;
            _appSettings = appSettings;
            _emailConfig = emailConfig;
            _smsConfig = smsConfig;
            _sendService = emailService;
            _userService = userService;
        }

        [HttpPost("sendemail/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> SendEmail(string id, [FromBody] SendMessageData sendData)
        {
            string displayMessage = "";
            ObjectResult sendObjectResult;
            Dictionary<int, ObjectResult> sendListResults = new Dictionary<int, ObjectResult>();

            try
            {
                if (!Enum.GetValues(typeof(eXmlEmailTemplates)).Cast<int>().Any(x => x == sendData.TemplateId))
                {
                    displayMessage = $"send email error: templateId = {sendData.TemplateId} not found!";
                }
                else
                {
                    eXmlEmailTemplates xmlEmailTemplate = (eXmlEmailTemplates)sendData.TemplateId;
                    switch (xmlEmailTemplate)
                    {
                        case eXmlEmailTemplates.Aman:

                            List<PsygateCandidate> candidates = ReadCandidates(id == "0" ? null : id);
                            if (candidates == null || !candidates.Any())
                            {
                                displayMessage = "email sending error: candidates empty or null";
                                break;
                            }

                            sendListResults = candidates
                                .Select(x => (int)x.ID_NUMBER).Distinct()
                                .ToDictionary(k => k, v => new ObjectResult("NotImplemented") { StatusCode = (int?)HttpStatusCode.NotImplemented });

                            displayMessage = $"{nameof(SendEmail)} sending start...";

                            sendObjectResult = await _sendService.SendEmailAsync(xmlEmailTemplate, candidates);

                            if (sendObjectResult.IsSuccessStatusCode())
                            {
                                displayMessage = $"{_emailConfig.AdminEmailAddressServiceCenter} sent successfully" +
                                    (sendObjectResult.StatusCode == (int)HttpStatusCode.MultiStatus
                                        ? $" with errors ({sendObjectResult.Value})" : "");
                            }
                            else
                            {
                                displayMessage = $"{_emailConfig.AdminEmailAddressServiceCenter} send error, statuscode: {sendObjectResult.StatusCode}";
                            }
                            break;

                        case eXmlEmailTemplates.ServiceCenter:

                            if (!string.IsNullOrEmpty(sendData.Email))
                            {
                                displayMessage = $"{nameof(SendEmail)} sending start...";

                                sendObjectResult = await _sendService.SendEmailAsync(xmlEmailTemplate, sendData);

                                if (sendObjectResult.IsSuccessStatusCode())
                                {
                                    displayMessage = $"{_emailConfig.AdminEmailAddressServiceCenter} sent successfully";
                                    sendObjectResult.StatusCode = (int?)HttpStatusCode.Accepted;
                                }
                                else
                                {
                                    displayMessage = $"{_emailConfig.AdminEmailAddressServiceCenter} send error, statuscode: {sendObjectResult.StatusCode}";
                                }

                                sendObjectResult.Value = displayMessage;
                            }
                            else
                            {
                                displayMessage = "email null or empty";
                                sendObjectResult = new ObjectResult($"{sendData.Name}: {displayMessage}"); // { Value = $"{data.Name}: {displayMessage}" };
                            }

                            var _candidate = ReadCandidates(id).FirstOrDefault();
                            if (_candidate != null)
                            {
                                sendListResults[(int)_candidate.ID_NUMBER] = sendObjectResult;
                            }
                            else
                            {
                                displayMessage = $"{id} not found";
                                sendListResults[0] = sendObjectResult;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                displayMessage = $"{nameof(SendEmail)} error: {ex?.InnerException?.Message}";
                sendListResults[0].Value = displayMessage;
                _logger.Error(displayMessage);
            }

            // Add logs
            var errorList = sendListResults.Where(x => !x.Value.IsSuccessStatusCode())
                .Select(result => $"{result.Key}: {result.Value.Value.ToString()}").ToArray();

            var successList = sendListResults.Where(x => x.Value.IsSuccessStatusCode())
                .Select(result => $"{result.Key}: {result.Value.Value.ToString()}").ToArray();

            var logErrorResultMessages = errorList.Length > 1 ? string.Join("; ", errorList) : errorList.FirstOrDefault();
            var logSuccessResultMessages = successList.Length > 1 ? string.Join("; ", successList) : successList.FirstOrDefault();

            _logger.Error($"{nameof(SendEmail)} error list: {(string.IsNullOrEmpty(logErrorResultMessages) ? "none" : logErrorResultMessages)}");
            _logger.Information($"{nameof(SendEmail)} success list: {(string.IsNullOrEmpty(logSuccessResultMessages) ? "none" : logSuccessResultMessages)}");

            return Ok(new
            {
                success = logSuccessResultMessages,
                error = logErrorResultMessages
            });
        }

        [HttpGet("sendsms/{id}")]
        public async Task<IActionResult> SendSms(string id)
        {
            string displayMessage;
            try
            {
                var candidate = ReadCandidates(id).FirstOrDefault();
                if (candidate != null)
                {
                    // generate code
                    string code = Util.GenerateRandomDigits(6);

                    if (code == null)
                        throw new Exception("generated code error!");

                    // Set cache options.
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        // Keep in cache for this time, reset time if accessed.
                        .SetSlidingExpiration(TimeSpan.FromMinutes(_smsConfig.CodeTimeExpiration));

                    // Save data in cache.
                    _cache.Set(CacheKeys.CodeSms + "_" + id, code, cacheEntryOptions);

                    ObjectResult sendObjectResult = await _sendService.SendSmsAsync(
                        candidate.FIRST_NAME, candidate.PHONE_NUMBER2,
                        $"{code} Is your verification code.The code will be used by you later in the process."
                    );

                    if (sendObjectResult != null && sendObjectResult.IsSuccessStatusCode())
                    {
                        displayMessage = $"{nameof(SendController)} {id}: sms sent successfully";
                        sendObjectResult.StatusCode = (int?)HttpStatusCode.Accepted;

                        displayMessage = "sent successfully";
                        _logger.Information($"sms sent successfully to {id}");

                        return Ok(true); // new { code = Encoding.UTF8.ToBase64(code.ToString()) }
                    }
                    else
                    {
                        displayMessage = $"{id}: sent error";
                        return BadRequest(displayMessage);
                    }
                }
                else
                {
                    displayMessage = $"sms send error: candidate {id} not found";
                    return BadRequest(displayMessage);
                }

            }
            catch (Exception ex)
            {
                displayMessage = $"send sms error: {ex?.InnerException?.Message}";
                return BadRequest($"{nameof(SendController)} " + displayMessage);
            }
        }

        [HttpPost("comparesms/{id}")]
        public IActionResult CompareSms(string id, [FromBody] int value)
        {
            try
            {
                if (_cache.TryGetValue(CacheKeys.CodeSms + "_" + id, out int cacheCodeSms))
                {
                    if (value == cacheCodeSms)
                    {
                        _logger.Information($"success: sms code equal to original code");
                        return Ok(new { appLink = _appSettings.Value.Link, orgId = _appSettings.Value.OrgId });
                    }
                }

                var message = $"sms code not equal to original code";
                _logger.Error($"{nameof(CompareSms)} {message}");
                return BadRequest();
            }
            catch (Exception ex)
            {
                var message = $"{nameof(CompareSms)} {ex?.InnerException?.Message}";
                _logger.Error(message);
                return BadRequest(message);
            }
        }

        [HttpGet("SmsAuth/{id}")]
        [AllowAnonymous]
        public IActionResult IsSmsAuthenticated(string id)
        {
            if (_cache.Get(CacheKeys.CodeSms + "_" + id) != null)
            {
                _cache.Remove(CacheKeys.CodeSms + "_" + id);
                return Ok(new { appLink = _appSettings.Value.Link, orgId = _appSettings.Value.OrgId });
            }
            else
            {
                var message = $"Not sms authenticated";
                return BadRequest(new { errorMessage = message });
            }
        }

        private List<PsygateCandidate> ReadCandidates(string id = null)
        {
            //var candidates = RedirectToAction("ReadCandidates", "Candidates", new { id = id });
            //var candidatesController = new CandidatesController(_context, _userService, _logger, _appSettings, _emailConfig);
            //var candidates = candidatesController.ReadCandidates(id).ToList();
            return null;
        }

        private async Task<ObjectResult> SendEmailsSmtpAsync(List<PsygateCandidate> candidates, eXmlEmailTemplates sendTemplate)
        {
            string displayMessage;
            ObjectResult sendObjectResult = new ObjectResult("") { StatusCode = (int)HttpStatusCode.NotImplemented };

            foreach (PsygateCandidate candidate in candidates)
            {
                if (!string.IsNullOrEmpty(candidate.EMAIL))
                {
                    EmailTemplate templTemplate =
                        new EmailTemplate(sendTemplate, "", _appSettings.Value.MapitMainUrl, candidate);

                    sendObjectResult = await _sendService.SendEmailSmtpAsync(
                        name: candidate.FIRST_NAME,
                        subject: "",
                        addressFrom: _emailConfig.AdminEmailAddressFrom,
                        addressTo: string.IsNullOrEmpty(_emailConfig.AdminEmailAddressTo) ? candidate.EMAIL : _emailConfig.AdminEmailAddressTo,
                        message: templTemplate.Body);

                    if (sendObjectResult.IsSuccessStatusCode())
                    {
                        displayMessage = $"{candidate.EMAIL} sent successfully";
                        sendObjectResult.StatusCode = (int?)HttpStatusCode.Accepted;
                    }
                    else
                    {
                        displayMessage = $"{(sendObjectResult.Value != null ? sendObjectResult.Value.ToString() : "")}; {candidate.EMAIL} sent error";
                    }
                    sendObjectResult.Value = displayMessage;
                    //sendListResults[(int)candidate.ID_NUMBER] = sendObjectResult;
                }
                else
                {
                    displayMessage = "email null or empty";
                    // sendListResults[(int)candidate.ID_NUMBER].Value = displayMessage;
                }
            }
            return sendObjectResult;
        }
    }
}
