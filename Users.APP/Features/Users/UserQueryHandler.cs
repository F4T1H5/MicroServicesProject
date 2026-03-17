using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Users.APP.Domain;
using Users.APP.Features.Groups;
using Users.APP.Features.Roles;

namespace Users.APP.Features.Users
{
    public class UserQueryRequest : Request, IRequest<IQueryable<UserQueryResponse>>
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Genders? Gender { get; set; }
        public DateTime? BirthDateStart { get; set; }
        public DateTime? BirthDateEnd { get; set; }
        public decimal? ScoreStart { get; set; }
        public decimal? ScoreEnd { get; set; }
        public bool? IsActive { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? GroupId { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserQueryResponse : Response
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Genders Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public decimal Score { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public int? GroupId { get; set; }
        public List<int> RoleIds { get; set; }

        public string FullName { get; set; }
        public string GenderF { get; set; }
        public string BirthDateF { get; set; }
        public string RegistrationDateF { get; set; }
        public string ScoreF { get; set; }
        public string IsActiveF { get; set; }
        public string Country { get; set; }
        public string City { get; set; }

        public string GroupF { get; set; }
        public List<string> RolesF { get; set; }
        public GroupQueryResponse Group { get; set; }
        public List<RoleQueryResponse> Roles { get; set; }
    }

    public class UserQueryHandler : Service<User>, IRequestHandler<UserQueryRequest, IQueryable<UserQueryResponse>>
    {
        public UserQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet() // will return Users DbSet
                .AsNoTracking() // AsNoTracking is optional and disables Entity Framework change tracking, therefore increases querying performance
                .Include(u => u.Group) // will include the relational Group data
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role) // will first include the relational UserRoles then Role data
                .OrderByDescending(u => u.IsActive) // query will be ordered descending by IsActive values
                .ThenBy(u => u.RegistrationDate) // after order is done for IsActive, ordered query will be ordered ascending by RegistrationDate values
                .ThenBy(u => u.UserName); // after order is done for RegistrationDate, ordered query will be ordered ascending by UserName values
        }

        public Task<IQueryable<UserQueryResponse>> Handle(UserQueryRequest request, CancellationToken cancellationToken)
        {
            var entityQuery = DbSet();

            // if UserName != null and UserName.Trim() != ""
            if (!string.IsNullOrWhiteSpace(request.UserName))
                // apply user name filtering to the query for exact match
                // Way 1:
                //query = query.Where(u => u.UserName.Equals(request.UserName));
                // Way 2:
                entityQuery = entityQuery.Where(u => u.UserName == request.UserName);

            // if FirstName has a value other than null or empty string without whitespace characters
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                // apply first name filtering to the query for case-sensitive partial match,
                // for partial match StartsWith or EndsWith methods can also be used instead of Contains method
                entityQuery = entityQuery.Where(u => u.FirstName.Contains(request.FirstName.Trim()));

            // if LastName has a value other than null or empty string without whitespace characters
            if (!string.IsNullOrWhiteSpace(request.LastName))
                // apply last name filtering to the query for case-sensitive partial match
                entityQuery = entityQuery.Where(u => u.LastName.Contains(request.LastName.Trim()));

            // if Gender has a value therefore is not null
            if (request.Gender.HasValue) // if (request.Gender is not null) or if (request.Gender != null) can also be written
                // apply gender filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.Gender == request.Gender.Value);

            // if BirthDateStart has a value
            if (request.BirthDateStart.HasValue)
                // apply birth date start filtering to the query for greater than or equal match
                // Way 1: filtering with date and time value (e.g. 08/22/1990 13:45:57)
                //query = query.Where(u => u.BirthDate.HasValue && u.BirthDate.Value >= request.BirthDateStart.Value);
                // Way 2: filtering with date value only (e.g. 08/22/1990)
                entityQuery = entityQuery.Where(u => u.BirthDate.HasValue && u.BirthDate.Value.Date >= request.BirthDateStart.Value.Date);

            // if BirthDateEnd has a value
            if (request.BirthDateEnd.HasValue)
                // apply birth date end without time filtering to the query for less than or equal match
                entityQuery = entityQuery.Where(u => u.BirthDate.HasValue && u.BirthDate.Value.Date <= request.BirthDateEnd.Value.Date);

            // if ScoreStart has a value
            if (request.ScoreStart.HasValue)
                // apply score start filtering to the query for greater than or equal match
                entityQuery = entityQuery.Where(u => u.Score >= request.ScoreStart.Value);

            // if ScoreEnd has a value
            if (request.ScoreEnd.HasValue)
                // apply score end filtering to the query for less than or equal match
                entityQuery = entityQuery.Where(u => u.Score <= request.ScoreEnd.Value);

            // if IsActive has a value
            if (request.IsActive.HasValue)
                // apply is active filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.IsActive == request.IsActive.Value);

            // if CountryId has a value
            if (request.CountryId.HasValue)
                // apply country ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.CountryId == request.CountryId.Value);

            // if CityId has a value
            if (request.CityId.HasValue)
                // apply city ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.CityId == request.CityId.Value);

            // if GroupId has a value
            if (request.GroupId.HasValue)
                // apply group ID filtering to the query for exact match
                entityQuery = entityQuery.Where(u => u.GroupId == request.GroupId.Value);

            // if RoleIds has a list with at least one element
            if (request.RoleIds.Count > 0) // Any() method can also be used instead of Count > 0
                // apply role IDs filtering to the query for any match
                entityQuery = entityQuery.Where(u => u.UserRoles.Any(ur => request.RoleIds.Contains(ur.RoleId)));

            var query = entityQuery.Select(u => new UserQueryResponse // () after the class name may not be used
            {
                Id = u.Id,
                UserName = u.UserName,
                Password = u.Password,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Gender = u.Gender,
                BirthDate = u.BirthDate,
                RegistrationDate = u.RegistrationDate,
                Score = u.Score,
                IsActive = u.IsActive,
                Address = u.Address,
                CountryId = u.CountryId,
                CityId = u.CityId,
                GroupId = u.GroupId,
                RoleIds = u.RoleIds,

                FullName = u.FirstName + " " + u.LastName,

                GenderF = u.Gender.ToString(), // will assign Woman or Man

                BirthDateF = u.BirthDate.HasValue ? u.BirthDate.Value.ToString("MM/dd/yyyy") : string.Empty,

                RegistrationDateF = u.RegistrationDate.ToShortDateString(),
                ScoreF = u.Score.ToString("N1"), // N: number format, C: currency format, 1: one decimal

                // assign "Active" or "Inactive" for IsActive boolean property using ternary operator
                IsActiveF = u.IsActive ? "Active" : "Inactive",

                Country = (u.CountryId ?? 0).ToString(),

                City = (u.CityId ?? 0).ToString(),

                GroupF = u.Group != null ? u.Group.Title : null,

                RolesF = u.UserRoles.Select(ur => ur.Role.Name).ToList(),

                Group = new GroupQueryResponse
                {
                    Id = u.Group.Id,
                    Title = u.Group.Title
                },
                Roles = u.UserRoles.Select(ur => new RoleQueryResponse
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}