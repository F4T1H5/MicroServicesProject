using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Users.APP.Domain;

namespace Users.APP.Features.Users
{
    public class UserUpdateRequest : Request, IRequest<CommandResponse>
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


        [Range(0, 5)]
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

    public class UserUpdateHandler : Service<User>, IRequestHandler<UserUpdateRequest, CommandResponse>
    {
        public UserUpdateHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<User> DbSet()
        {
            return base.DbSet().Include(u => u.UserRoles);
        }

        public async Task<CommandResponse> Handle(UserUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(u => u.Id != request.Id && u.IsActive && u.UserName == request.UserName, cancellationToken))
                return Error("Active user with the same user name exists!");

            var entity = await DbSet().SingleOrDefaultAsync(u => u.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("User not found!");

            Delete(entity.UserRoles);

            entity.UserName = request.UserName;
            entity.Password = request.Password;
            entity.FirstName = request.FirstName?.Trim(); // ? is used because request.FirstName can be null
            entity.LastName = request.LastName?.Trim(); // ? is used because request.LastName can be null
            entity.Gender = request.Gender;
            entity.BirthDate = request.BirthDate;
            entity.Score = request.Score;
            entity.IsActive = request.IsActive;
            entity.Address = request.Address?.Trim(); // ? is used because request.Address can be null
            entity.CountryId = request.CountryId;
            entity.CityId = request.CityId;
            entity.GroupId = request.GroupId;
            entity.RoleIds = request.RoleIds;

            await UpdateAsync(entity, cancellationToken); // will update the entity in the Users DbSet and since save default parameter's value is true, will save changes to the database

            return Success("User updated successfully.", entity.Id);
        }
    }
}