using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupCreateServiceBaseRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(100)]
        public string Title { get; set; }

        public class GroupCreateServiceBaseHandler : ServiceBase, IRequestHandler<GroupCreateServiceBaseRequest, CommandResponse>
        {
            private readonly UsersDb _db;

            public GroupCreateServiceBaseHandler(UsersDb db)
            {
                _db = db;
            }

            public async Task<CommandResponse> Handle(GroupCreateServiceBaseRequest request, CancellationToken cancellationToken)
            {
                if (await _db.Groups.AnyAsync(groupEntity => groupEntity.Title == request.Title.Trim(), cancellationToken))
                    return Error("Group with the same title exists!");

                var entity = new Group()
                {
                    Title = request.Title.Trim()
                };

                _db.Groups.Add(entity);

                await _db.SaveChangesAsync(cancellationToken);

                return Success("Group created successfully.", entity.Id);
            }
        }
    }
}