using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using Firios.Data;
using Firios.Entity;
using Firios.Models;
using Firios.Models.WebSocketsModels;
using Firios.Models.WithoutList;
using Firios.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using WebPush;

namespace Firios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentEntitiesController : ControllerBase
    {
        private readonly FiriosSuperLightContext _context;
        private readonly UserHelperService _userHelperService;
        private readonly WebSocketFiriosManagerService _managerService;
        private readonly ILogger<IncidentEntitiesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly FiriosUserAuthenticationService _userAuthenticationService;
        private readonly IncidentIdService _lastIncident;
        private readonly FiriosSourceAuthentificationService _sourceAuthentificationService;

        public IncidentEntitiesController(FiriosSuperLightContext context,
            UserHelperService userHelperService,
            WebSocketFiriosManagerService managerService,
            ILogger<IncidentEntitiesController> logger,
            IConfiguration configuration,
            FiriosUserAuthenticationService userAuthenticationService,
            IncidentIdService lastIncident,
            FiriosSourceAuthentificationService sourceAuthentificationService)
        {
            _context = context;
            _userHelperService = userHelperService;
            _managerService = managerService;
            _logger = logger;
            _configuration = configuration;
            _userAuthenticationService = userAuthenticationService;
            _lastIncident = lastIncident;
            _sourceAuthentificationService = sourceAuthentificationService;
        }

        // GET: api/IncidentEntities
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<IncidentEntity>>> GetIncidentEntity()
        //{
        //    return await _context.IncidentEntity.ToListAsync();
        //}

        // GET: api/IncidentEntities/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<IncidentModel>> GetIncidentEntity(Guid id)
        //{
        //    var incidentModel = await _userHelperService.GetIncidentModelById(id);

        //    if (incidentModel == null)
        //    {
        //        return NotFound();
        //    }

        //    return incidentModel;
        //}
        // POST: api/IncidentEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> IncidentRegistration(IncidentWithoutList incident)
        {
            string validation = "";
            var regex = @"^([0-9A-Fa-f]{2})+$";
            var match = Regex.Match(incident.ValidationId, regex, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return Ok(new { Status = "bad" });
            }
            if (!_sourceAuthentificationService.Validate(incident.ValidationId, incident.SignatureId))
            {
                validation = _sourceAuthentificationService.GetValidationString(incident.ValidationId);
                if (validation == "")
                {
                    return Ok(new { Status = "bad" });
                }
            }

            var incidentEntity = new IncidentEntity
            {
                Id = new Guid(),
                Type = incident.Type,
                ObjectName = incident.ObjectName,
                AdditionalInformation = incident.AdditionalInformation,
                Level = incident.Level,
                IsActive = incident.IsActive,
                Date = incident.Date,
                Address = incident.Address,
                Mpd = incident.Mpd,
                Region = incident.Region,
                SubType = incident.SubType,
                Users = new List<UserIncidentEntity>()

            };
            _context.IncidentEntity.Add(incidentEntity);
            await _context.SaveChangesAsync();

            _lastIncident.Id = incidentEntity.Id;

            // Call push service
            var incidentPushData = new ServerPushData
            {
                id = incidentEntity.Id.ToString(),
                message = incidentEntity.Mpd + " " + incidentEntity.Type + " " + incidentEntity.SubType,
                smsSentAt = incidentEntity.Date.ToString(),
                serverReceiveDate = DateTime.Now.ToString(),

            };
            var subject = _configuration["Vapid:subject"];
            var publicKey = _configuration["Vapid:publicKey"];
            var privateKey = _configuration["Vapid:privateKey"];
            var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

            foreach (var userBrowserData in await _context.UserBrowserDatas.ToListAsync())
            {
                if (!string.IsNullOrEmpty(userBrowserData.Auth) && !string.IsNullOrEmpty(userBrowserData.Endpoint) && !string.IsNullOrEmpty(userBrowserData.P256dh))
                {
                    var subscription = new PushSubscription(userBrowserData.Endpoint, userBrowserData.P256dh, userBrowserData.Auth);

                    var webPushClient = new WebPushClient();
                    incidentPushData.session = userBrowserData.Session;
                    try
                    {
                        await webPushClient.SendNotificationAsync(subscription, incidentPushData.ToJson(), vapidDetails);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception.Message);
                    }
                }
            }

            // Call websockets

            var serverMsg = Encoding.UTF8.GetBytes((new WebSocketModel { IncidentWithoutList = incident }).ToJson());
            foreach (var webSocket in _managerService.GetAll())
            {
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length),
                    WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
            }

            return Ok(new { incidentEntity, Status = "ok", Sig = validation });
        }
        [HttpPost("registration")]
        public async Task<StatusCodeResult> Registration(UserToIncidentInputModel data)
        {
            if (ModelState.IsValid && data.State == "yes" || data.State == "no" || data.State == "on_place")
            {

                if (!_userAuthenticationService.ValidateUser(data.Session,
                        new List<string>()
                        {
                            FiriosConstants.HASIC,
                            FiriosConstants.STROJNIK,
                            FiriosConstants.VELITEL,
                            FiriosConstants.VELITEL_JEDNOTKY
                        }))
                {
                    return StatusCode(400);
                }

                var userBrowser = await _context.UserBrowserDatas.Include(i => i.UserEntity)
                    .FirstOrDefaultAsync(i => i.Session == data.Session);
                if (userBrowser == null || userBrowser.UserEntity == null)
                {
                    return StatusCode(400);
                }
                var incident = await _userHelperService.SaveUserToIncident(data);
                if (incident == null)
                    return StatusCode(400);

                if (data.IncidentId != _lastIncident.Id)
                    return Ok();

                // Web sockety

                var user = userBrowser.UserEntity;

                var serverMsg = Encoding.UTF8.GetBytes((new WebSocketModel
                {
                    Id = user.Id.ToString(),
                    Action = data.State,
                    Name = user.FirstName,
                    Surname = user.SecondName,
                    Position = user.Position
                }).ToJson());

                foreach (var webSocket in _managerService.GetAll())
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length),
                        WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
                }

                return Ok();
            }

            return StatusCode(400);
        }

        [HttpGet("/UserRegistration")]
        public async Task Registration()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Register(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Register(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            var userBrowserData = await _context.UserBrowserDatas.FirstOrDefaultAsync(i => i.Session == Encoding.UTF8.GetString(buffer));
            if (userBrowserData == null)
            {
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                return;
            }

            var id = new Guid();
            _managerService.AddNew(id, webSocket);
            var serverMsg = Encoding.UTF8.GetBytes((new WebSocketModel { Status = "ok" }).ToJson());

            await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

            // If last incident have users

            var lastIncident = await _context.IncidentEntity
                .Include(i => i.Users)
                .ThenInclude(i => i.UserEntity)
                .FirstOrDefaultAsync(i => i.Id == _lastIncident.Id);

            if (lastIncident != null)
            {
                foreach (var userIncident in lastIncident.Users)
                {
                    var user = userIncident.UserEntity;
                    var webSocketMsg = Encoding.UTF8.GetBytes((new WebSocketModel
                    {
                        Id = user.Id.ToString(),
                        Action = userIncident.State,
                        Name = user.FirstName,
                        Surname = user.SecondName,
                        Position = user.Position
                    }).ToJson());
                    await webSocket.SendAsync(new ArraySegment<byte>(webSocketMsg, 0, webSocketMsg.Length),
                        WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
                }
            }

            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                // TODO What to do if client send something //UPDATE he propably 

            }

            _managerService.RemoveById(id);
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        [HttpPost("PushRegistration")]
        public async Task<IActionResult> PushNotificationRegistration(UserPushData userPushData)
        {
            if (ModelState.IsValid)
            {
                var isSaved = false;
                var browserDatas = _context.UserBrowserDatas.Where(i => i.Auth == userPushData.Auth && i.Endpoint == userPushData.Endpoint && i.P256dh == userPushData.P256dh).ToList();
                foreach (var browserData in browserDatas)
                {
                    if (browserData.Session != userPushData.Session)
                    {
                        _context.Remove(browserData);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        isSaved = true;
                    }
                }

                if (isSaved)
                {
                    return Ok();
                }

                var userBrowserData = await _context.UserBrowserDatas.FirstOrDefaultAsync(i => i.Session == userPushData.Session);
                if (userBrowserData == null)
                {
                    return BadRequest();
                }

                userBrowserData.Auth = userPushData.Auth;
                userBrowserData.Endpoint = userPushData.Endpoint;
                userBrowserData.P256dh = userPushData.P256dh;

                _context.Update(userBrowserData);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }
        [HttpPost("NotifierRegistration")]
        public string NotifierRegistration(NotifierModel model)
        {
            var regex = @"^([0-9A-Fa-f]{2})+$";
            var match = Regex.Match(model.ValidationId, regex, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return "bad";
            }
            if (_sourceAuthentificationService.Validate(model.ValidationId, model.SignatureId))
            {
                return "ok";
            }

            var validation = _sourceAuthentificationService.GetValidationString(model.ValidationId);

            return validation == "" ? "bad" : validation;
        }
    }
}
