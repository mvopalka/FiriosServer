using Firios.Data;
using Firios.Entity;
using Firios.Mapper;
using Firios.Model;
using Firios.Model.WithoutList;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;
using System.Text;
using WebPush;

namespace Firios.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncidentEntitiesController : ControllerBase
    {
        private readonly FiriosSuperLightContext _context;
        private readonly Repository _repository;
        private readonly WebSocketFiriosManager _manager;
        private readonly ILogger<IncidentEntitiesController> _logger;
        private readonly IConfiguration _configuration;

        public IncidentEntitiesController(FiriosSuperLightContext context, Repository repository, WebSocketFiriosManager manager, ILogger<IncidentEntitiesController> logger, IConfiguration configuration)
        {
            _context = context;
            _repository = repository;
            _manager = manager;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: api/IncidentEntities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IncidentEntity>>> GetIncidentEntity()
        {
            return await _context.IncidentEntity.ToListAsync();
        }

        // GET: api/IncidentEntities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentModel>> GetIncidentEntity(Guid id)
        {
            var incidentModel = await _repository.GetIncidentModelById(id);

            if (incidentModel == null)
            {
                return NotFound();
            }

            return incidentModel;
        }
        // POST: api/IncidentEntities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IncidentEntity>> IncidentRegistration(IncidentWithoutList incident)
        {
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

            foreach (var userBrowserData in await _context.UserBrowserDatas.ToListAsync())
            {
                if (!string.IsNullOrEmpty(userBrowserData.Auth) && !string.IsNullOrEmpty(userBrowserData.Endpoint) && !string.IsNullOrEmpty(userBrowserData.P256dh))
                {
                    var subscription = new PushSubscription(userBrowserData.Endpoint, userBrowserData.P256dh, userBrowserData.Auth);
                    var subject = _configuration["Vapid:subject"];
                    var publicKey = _configuration["Vapid:publicKey"];
                    var privateKey = _configuration["Vapid:privateKey"];

                    var vapidDetails = new VapidDetails(subject, publicKey, privateKey);

                    var webPushClient = new WebPushClient();
                    try
                    {
                        await webPushClient.SendNotificationAsync(subscription, incidentEntity.Id.ToString(), vapidDetails);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception.Message);
                    }
                }
            }


            var serverMsg = Encoding.UTF8.GetBytes(incidentEntity.Id.ToString());
            foreach (var webSocket in _manager.GetAll())
            {
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length),
                    WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, CancellationToken.None);
            }

            return incidentEntity;
        }
        [HttpPost("registration")]
        public Task<IncidentModel> Registration(UserToIncidentInputModel data)
        {
            // TODO return simplier response (potvrzeno a kolik jede?)
            if (ModelState.IsValid && data.State == "yes" || data.State == "no" || data.State == "on_place")
            {
                var incident = _repository.SaveUserToIncident(data);
                return incident;
            }

            return null;
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

            // TODO authorization

            _manager.AddNew(new Guid(), webSocket);
            var serverMsg = Encoding.UTF8.GetBytes($"Authorization OK");

            await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                // TODO What to do if client send something //UPDATE he propably 

            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
        [HttpPost("PushRegistration")]
        public async Task<IActionResult> PushNotificationRegistration(UserPushData userPushData) //Todo to model and implement
        {
            if (ModelState.IsValid)
            {
                var userBrowserData = _context.UserBrowserDatas.FirstOrDefault(i => i.Session == userPushData.Session);
                if (userBrowserData == null)
                {
                    return BadRequest();
                }

                userBrowserData.Auth = userPushData.Auth;
                userBrowserData.Endpoint = userPushData.Endpoint;
                userBrowserData.P256dh = userPushData.P256dh;

                _context.Update(userBrowserData);
                await _context.SaveChangesAsync();

                userBrowserData = _context.UserBrowserDatas.FirstOrDefault(i => i.Session == userPushData.Session);
                return Ok();
            }
            return BadRequest();
        }

    }
}
