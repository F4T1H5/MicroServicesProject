using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class GroupDeleteHandler : Service<Group>, IRequestHandler<GroupDeleteRequest, CommandResponse>
    {
        public GroupDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GroupDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().Include(groupEntity => groupEntity.Users)
                .SingleOrDefaultAsync(groupEntity => groupEntity.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Group not found!");

            if (entity.Users.Count > 0)
                return Error("Group can't be deleted because it has relational users!");

            await DeleteAsync(entity, cancellationToken);

            return Success("Group deleted successfully.", entity.Id);
        }
    }
}