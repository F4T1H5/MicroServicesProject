using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Users.APP.Domain;

namespace Users.APP.Features.Groups
{
    public class GroupQueryServiceBaseRequest : Request, IRequest<IQueryable<GroupQueryServiceBaseResponse>>
    {
    }

    public class GroupQueryServiceBaseResponse : Response
    {
        public string Title { get; set; }
    }

    public class GroupQueryServiceBaseHandler : ServiceBase, IRequestHandler<GroupQueryServiceBaseRequest, IQueryable<GroupQueryServiceBaseResponse>>
    {
        private readonly UsersDb _db;

        public GroupQueryServiceBaseHandler(UsersDb db)
        {
            _db = db;
        }

        public Task<IQueryable<GroupQueryServiceBaseResponse>> Handle(GroupQueryServiceBaseRequest request, CancellationToken cancellationToken)
        {
            var query = _db.Groups.Select(groupEntity => new GroupQueryServiceBaseResponse()
            {
                Id = groupEntity.Id,
                Title = groupEntity.Title
            });

            return Task.FromResult(query);
        }
    }
}