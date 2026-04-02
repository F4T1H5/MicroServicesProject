using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(30, MinimumLength = 4)]
        public string UserName { get; set; }

        [Required, StringLength(15, MinimumLength = 3)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public Genders Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        //public DateTime RegistrationDate { get; set; } // we don't need this property in the request since we won't get its value from the API or UI

        [Range(0, 5)] // minimum value can be 0, maximum value can be 5
        public decimal Score { get; set; }

        public bool IsActive { get; set; }

        public string Address { get; set; }

        // [Required] // can be defined if each user must have a country
        public int? CountryId { get; set; }

        //[Required] // can be defined if each user must have a city
        public int? CityId { get; set; }

        //[Reqired] // can be defined if each user must have a group
        public int? GroupId { get; set; }

        //[Required] // can be defined if each user must have at least one role
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UserCreateHandler : Service<User>, IRequestHandler<UserCreateRequest, CommandResponse>
    {
        private readonly DbContext _db;

        public UserCreateHandler(DbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(UserCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.IsActive && u.UserName == request.UserName, cancellationToken))
                return Error("Active user with the same user name exists!");

            // Validate GroupId if provided
            if (request.GroupId.HasValue)
            {
                var groupExists = await _db.Set<Group>().AnyAsync(g => g.Id == request.GroupId.Value, cancellationToken);
                if (!groupExists)
                    return Error($"Group with ID {request.GroupId} does not exist!");
            }

            // Validate RoleIds if provided
            if (request.RoleIds != null && request.RoleIds.Any())
            {
                var existingRoleIds = await _db.Set<Role>()
                    .Where(r => request.RoleIds.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToListAsync(cancellationToken);

                var invalidRoleIds = request.RoleIds.Except(existingRoleIds).ToList();
                if (invalidRoleIds.Any())
                    return Error($"Role(s) with ID(s) {string.Join(", ", invalidRoleIds)} do not exist!");
            }

            var entity = new User
            {
                UserName = request.UserName,
                Password = request.Password,
                FirstName = request.FirstName?.Trim(),
                LastName = request.LastName?.Trim(),
                Gender = request.Gender,
                BirthDate = request.BirthDate,
                RegistrationDate = DateTime.Now,
                Score = request.Score,
                IsActive = request.IsActive,
                Address = request.Address?.Trim(),
                CountryId = request.CountryId,
                CityId = request.CityId,
                GroupId = request.GroupId,
                RoleIds = request.RoleIds ?? new List<int>()
            };

            await CreateAsync(entity, cancellationToken);

            return Success("User created successfully.", entity.Id);
        }
    }
}