using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace General.Infrastructure.WebApi.Core.Controllers
{
    [Route("/api/")]
    [ApiController]
    [Authorize]
    public class CandidatesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly MapitContext _context;
        private readonly MapitSettings _appSettings;
        private readonly IUserService _userService;
        private readonly IEmailConfiguration _emailConfig;

        public CandidatesController(MapitContext context,
            IUserService userService, ILogger logger,
            IOptions<MapitSettings> appSettings,
            IEmailConfiguration emailConfig)
        {
            _context = context;
            _logger = logger;
            _appSettings = appSettings.Value;
            _userService = userService;
            _emailConfig = emailConfig;
        }

        [HttpPut("candidates")] //(Policy = "EmployeeOnly")
        public async Task<IActionResult> Candidates()
        {
            _logger.Information($"{nameof(CandidatesController)} method GetCandidates called!");

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    string authHeader = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authHeader);

                    // get to psypass service
                    _logger.Information($"{nameof(Candidates)} method httpClient.GetAsync rerquest...");
                    HttpResponseMessage response = await httpClient.GetAsync(_appSettings.MapitServiceUrl);
                    _logger.Information($"{nameof(Candidates)} method httpClient.GetAsync response: {response.RequestMessage}; {response.StatusCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        // read data as string and get deserialized result
                        string jsonStringResult = await response.Content.ReadAsStringAsync();
                        List<PsygateCandidate> candidateList = JsonConvert.DeserializeObject<List<PsygateCandidate>>(jsonStringResult);

                        if (candidateList == null || !candidateList.Any())
                        {
                            var displayMessage = "Psygate candidates is null or []";
                            _logger.Error($"{nameof(CandidatesController)} Candidates action: {displayMessage}");
                            return BadRequest(displayMessage);
#if DEBUG
                            //candidates = CandidateExtensions.LoadJson(); 
#endif
                        }

                        // Remove all candidates
                        _context.Candidates.RemoveRange(_context.Candidates);
                        _context.SaveChanges();

                        // add candidates
                        candidateList = candidateList.Select(pc =>
                        {
                            pc.TEMPPASS = Encoding.UTF8.ToBase64(_userService.PasswordGenerate());
                            pc.EMAIL = string.IsNullOrEmpty(pc.EMAIL) ? _emailConfig.AdminEmailAddressTo : pc.EMAIL;
                            return pc;
                        }).ToList();

                        _context.Candidates.AddRange(candidateList);
                        _context.SaveChanges();

                        candidateList = candidateList.Select(pc =>
                        {
                            string decodedTempPass;
                            bool isDecodeded = Encoding.UTF8.TryParseBase64(pc.TEMPPASS, out decodedTempPass);
                            pc.TEMPPASS = isDecodeded ? decodedTempPass : "error";
                            return pc;
                        }).ToList();

                        return Ok(candidateList);
                    }
                    else
                    {
                        var message = $"StatusCode {Enum.GetName(typeof(HttpStatusCode), response.StatusCode)} is not valid";
                        _logger.Error($"{nameof(CandidatesController)} Candidates action: {message}");
                        return BadRequest(message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"{nameof(CandidatesController)} Candidates action: {ex.InnerException?.Message}");
                    return BadRequest(ex.InnerException?.Message);
                }
            }
        }

        [HttpGet("candidates/{id?}")] // [Authorize] //(Policy = "EmployeeOnly")
        public IActionResult GetCandidates(string id = null)
        {
            // read candidates
            var resultCandidates = ReadCandidates(id);

            if (resultCandidates.Any())
            {
                if (!string.IsNullOrEmpty(id))
                {

                    // return single candidate
                    return Ok(resultCandidates.First());
                }

                // return all candidates
                return Ok(resultCandidates);
            }

            return BadRequest("Psygate candidates is null or []");
        }

        // read candidates 
        public IEnumerable<PsygateCandidate> ReadCandidates(string id = null)
        {
            IEnumerable<PsygateCandidate> candidates = _context.Candidates;

            // id for single candidate
            if (!string.IsNullOrEmpty(id))
            {
                candidates = _context.Candidates.Where(x => x.ID_NUMBER == double.Parse(id));
            }

            // display result is candidates with decoded TEMPPASS
            candidates = candidates.Select(c =>
                {
                    string decodedTempPass;
                    bool convertSuccess = Encoding.UTF8.TryParseBase64(c.TEMPPASS, out decodedTempPass);
                    c.TEMPPASS = convertSuccess ? decodedTempPass : "error";
                    // c.TEMPPASS = Encoding.UTF8.ToBase64(c.TEMPPASS);
                    return c;
                })
                .ToList();

            return candidates;
        }
    }
}
