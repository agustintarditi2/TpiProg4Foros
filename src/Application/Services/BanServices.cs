using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;
using MyApp.Domain.Entities;
using MyApp.Application.Interfaces;
using MyApp.Application.Models;

namespace MyApp.Application.Services;

public sealed class BanServices
{
    private readonly IBanRepository _bans;
    private readonly IUserRepository _users;

    public BanServices(
        IBanRepository bans,
        IUserRepository users)
    {
        _bans = bans;
        _users = users;
    }


//=========================================================================


    public async Task<BanId> CreateBan(CreateBanDTO banDTO, CancellationToken ct)
    {
        User? targetUser = await _users.GetById((UserId)banDTO.UserId, ct);
        if (targetUser is null)
            throw new NotFoundException("User to ban not found.");

        User? admin = await _users.GetById((UserId)banDTO.AdminId, ct);
        if (admin is null)
            throw new NotFoundException("Admin not found.");

        var ban = new Ban(targetUser, admin, banDTO.Days, banDTO.Reason);

        await _bans.Add(ban, ct);
        return ban.Id;
    }


//=========================================================================


    public async Task<BanDTO> GetById(Guid id, CancellationToken ct)
    {
        Ban? foundBan = await _bans.GetById((BanId)id, ct);
        if (foundBan is null)
            throw new NotFoundException("Requested ban not found");
        return BanDTO.Create(foundBan);
    }


//=========================================================================


    public async Task<List<BanDTO>> Get(CancellationToken ct)
    {
        List<Ban>? banList = await _bans.Get(ct);
        if (banList.Count < 1)
            throw new NotFoundException("No bans found");
        return BanDTO.CreateList(banList);
    }


//=========================================================================


    public async Task ModifyBan(ModifyBanDTO banDTO, CancellationToken ct)
    {
        if (banDTO.Id == Guid.Empty)
            throw new DomainException("ID is required to modify a ban.");

        Ban? ban = await _bans.GetById((BanId)banDTO.Id, ct);
        if (ban is null)
            throw new NotFoundException("Requested ban not found");

        ban.ModifyBan(banDTO.NewDays, banDTO.NewReason);

        _bans.Update(ban, ct);
    }


//=========================================================================


    public async Task Desban(Guid id, CancellationToken ct)
    {
        Ban? ban = await _bans.GetById((BanId)id, ct);
        if (ban is null)
            throw new NotFoundException("Requested ban not found");

        ban.Desban();

        _bans.Update(ban, ct);
    }


//=========================================================================


    public async Task DeleteBan(Guid id, CancellationToken ct)
    {
        Ban? banToDelete = await _bans.GetById((BanId)id, ct);
        if (banToDelete is null)
            throw new NotFoundException("Requested ban not found");
        _bans.Delete(banToDelete, ct);
    }
}