using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Users;

namespace Users.APP.Features.Roles
{
    public class RoleQueryRequest : Request, IRequest<IQueryable<RoleQueryResponse>>
    {
    }

    public class RoleQueryResponse : Response
    {
        public string Name { get; set; }

        public int UserCount { get; set; }
        public string UsersF { get; set; }
        public List<UserQueryResponse> Users { get; set; }
    }

    public class RoleQueryHandler : Service<Role>, IRequestHandler<RoleQueryRequest, IQueryable<RoleQueryResponse>>
    {
        public RoleQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Role> DbSet()
        {
            return base.DbSet() // will return Roles DbSet
                .Include(r => r.UserRoles).ThenInclude(ur => ur.User) // will first include the relational UserRoles then User data
                .OrderBy(r => r.Name); // query will be ordered ascending by Name values
        }

        public Task<IQueryable<RoleQueryResponse>> Handle(RoleQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(r => new RoleQueryResponse()
            {
                Id = r.Id,
                Name = r.Name,

                UserCount = r.UserRoles.Count, // returns the users count of each role
                UsersF = string.Join(", ", r.UserRoles.Select(ur => ur.User.UserName)), // returns a comma seperated user names string for each role

                Users = r.UserRoles.Select(ur => new UserQueryResponse
                {
                    Id = ur.User.Id,
                    UserName = ur.User.UserName,
                    Password = ur.User.Password,
                    IsActive = ur.User.IsActive,
                    RegistrationDate = ur.User.RegistrationDate,
                    BirthDate = ur.User.BirthDate,
                    Score = ur.User.Score,
                    FirstName = ur.User.FirstName,
                    LastName = ur.User.LastName,
                    Gender = ur.User.Gender,
                    Address = ur.User.Address,
                    GroupId = ur.User.GroupId,
                    RoleIds = ur.User.RoleIds,
                    CountryId = ur.User.CountryId,
                    CityId = ur.User.CityId,

                    FullName = ur.User.FirstName + " " + ur.User.LastName,

                    IsActiveF = ur.User.IsActive ? "Active" : "Inactive",

                    BirthDateF = ur.User.BirthDate.HasValue ? ur.User.BirthDate.Value.ToString("MM/dd/yyyy") : string.Empty,

                    RegistrationDateF = ur.User.RegistrationDate.ToShortDateString(),
                    ScoreF = ur.User.Score.ToString("N1"), // N: number format, C: currency format, 1: one decimal
                    GenderF = ur.User.Gender.ToString() // will assign Woman or Man
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}