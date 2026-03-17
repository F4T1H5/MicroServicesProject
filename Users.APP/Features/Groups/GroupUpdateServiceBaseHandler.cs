using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupUpdateServiceBaseRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(100)]
        public string Title { get; set; }
    }

    public class GroupUpdateServiceBaseHandler : ServiceBase, IRequestHandler<GroupUpdateServiceBaseRequest, CommandResponse>
    {
        private readonly UsersDb _db;

        public GroupUpdateServiceBaseHandler(UsersDb db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(GroupUpdateServiceBaseRequest request, CancellationToken cancellationToken)
        {
            if (await _db.Groups.AnyAsync(groupEntity => groupEntity.Id != request.Id
                && groupEntity.Title == request.Title.Trim(), cancellationToken))
                return Error("Group with the same title exists!");

            var entity = await _db.Groups.FindAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Group not found!");

            entity.Title = request.Title.Trim();

            _db.Groups.Update(entity);

            await _db.SaveChangesAsync(cancellationToken);

            return Success("Group updated successfully.", entity.Id);
        }
    }
}