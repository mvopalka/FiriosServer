using FiriosServer.Data;
using FiriosServer.Entity;
using FiriosServer.Models;
using Microsoft.EntityFrameworkCore;

namespace FiriosServer.Services;

public class UserHelperService
{
    private readonly FiriosSuperLightContext _context;

    public UserHelperService(FiriosSuperLightContext context)
    {
        _context = context;
    }
    public async Task<IncidentModel> SaveUserToIncident(UserToIncidentInputModel from)
    {
        var userData = await _context.UserBrowserDatas.Include(i => i.UserEntity)
            .FirstOrDefaultAsync(i => i.Session == from.Session);
        if (userData == null)
        {
            return null;
        }

        var userId = userData.UserEntity.Id;

        var incidentEntity = _context.IncidentEntity
            .Include(i => i.Users)
            .ThenInclude(i => i.UserEntity)
            .FirstOrDefault(i => i.Id.Equals(from.IncidentId));

        if (incidentEntity is { IsActive: false })
        {
            return null;
        }

        var userEntity = _context.UserEntity
            .Include(i => i.Incidents)
            .ThenInclude(i => i.IncidentEntity)
            .FirstOrDefault(i => i.Id.Equals(userId));

        //var incidentEntity = _context.IncidentEntity
        //    .Include(i => i.Users)
        //    .ThenInclude(i => i.UserEntity)
        //    .FirstOrDefault(i => i.Id.Equals(from.IncidentId));

        //var userEntity = _context.UserEntity
        //    .Include(i => i.Incidents)
        //    .ThenInclude(i => i.IncidentEntity)
        //    .FirstOrDefault(i => i.Id.Equals(from.UserId));

        if (userEntity == null || incidentEntity == null)
        {
            return null;
        }

        var userIncidentEntity = incidentEntity.Users.FirstOrDefault(i => i.UserId.Equals(userId));
        var existingUserIncidentEntity =
            await _context.UserIncidentEntity.FirstOrDefaultAsync(i =>
                i.IncidentId == incidentEntity.Id && i.UserId == userEntity.Id);
        if (userIncidentEntity == null && existingUserIncidentEntity == null)
        {
            userIncidentEntity = new UserIncidentEntity
            {
                IncidentEntity = incidentEntity,
                UserEntity = userEntity,
                State = from.State
            };
            incidentEntity.Users.Add(userIncidentEntity);
            userEntity.Incidents.Add(userIncidentEntity);
            //incidentEntity.Users = new List<UserIncidentEntity>();
        }
        else
        {
            userIncidentEntity.State = from.State;
            //_context.Entry(UserIncidentEntity).State = EntityState.Modified;
        }

        await _context.SaveChangesAsync();

        var incidentModel = new IncidentModel
        {
            Id = incidentEntity.Id,
            Type = incidentEntity.Type,
            ObjectName = incidentEntity.ObjectName,
            AdditionalInformation = incidentEntity.AdditionalInformation,
            Level = incidentEntity.Level,
            IsActive = incidentEntity.IsActive,
            Date = incidentEntity.Date,
            Address = incidentEntity.Address,
            Mpd = incidentEntity.Mpd,
            Region = incidentEntity.Region,
            SubType = incidentEntity.SubType,
            Users = incidentEntity.Users.OrderBy(i => i.UserEntity.FirstName).Select(i => new IncidentModel.User
            {
                Id = i.UserId,
                FirstName = i.UserEntity.FirstName,
                SecondName = i.UserEntity.SecondName,
                Position = i.UserEntity.Position,
                State = i.State
            }).ToList()
        };
        return incidentModel;
    }
    public async Task<IncidentModel> GetIncidentModelById(Guid id)
    {
        var incidentEntity = _context.IncidentEntity
            .Include(i => i.Users)
            .ThenInclude(i => i.UserEntity)
            .FirstOrDefault(i => i.Id.Equals(id));
        if (incidentEntity == null)
        {
            return null;
        }

        var users = incidentEntity.Users.Select(i => new IncidentModel.User
        {
            Id = i.UserId,
            FirstName = i.UserEntity.FirstName,
            SecondName = i.UserEntity.SecondName,
            Position = i.UserEntity.Position,
            State = i.State
        }).ToList();

        var incidentModel = new IncidentModel
        {
            Id = incidentEntity.Id,
            Type = incidentEntity.Type,
            ObjectName = incidentEntity.ObjectName,
            AdditionalInformation = incidentEntity.AdditionalInformation,
            Level = incidentEntity.Level,
            IsActive = incidentEntity.IsActive,
            Date = incidentEntity.Date,
            Address = incidentEntity.Address,
            Mpd = incidentEntity.Mpd,
            Region = incidentEntity.Region,
            SubType = incidentEntity.SubType,
            Users = users
        };
        return incidentModel;
    }
}