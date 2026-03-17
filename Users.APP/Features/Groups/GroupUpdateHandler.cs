using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(100)]
        public string Title { get; set; }
    }

    public class GroupUpdateHandler : Service<Group>, IRequestHandler<GroupUpdateRequest, CommandResponse>
    {
        public GroupUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GroupUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(groupEntity => groupEntity.Id != request.Id
                && groupEntity.Title == request.Title.Trim(), cancellationToken))
                return Error("Group with the same title exists!");

            var entity = await DbSet().SingleOrDefaultAsync(groupEntity => groupEntity.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Group not found!");

            entity.Title = request.Title.Trim(); 

            await UpdateAsync(entity, cancellationToken);

            return Success("Group updated successfully.", entity.Id);
        }
    }
}