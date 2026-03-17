using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Books.App.Features.Authors
{
    public class AuthorCreateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(25)]
        public string FirstName { get; set; }

        [Required, StringLength(25)]
        public string LastName { get; set; }
    }

    public class AuthorCreateHandler : Service<Author>, IRequestHandler<AuthorCreateRequest, CommandResponse>
    {
        public AuthorCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(AuthorCreateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(authorEntity => authorEntity.FirstName == request.FirstName.Trim()
                && authorEntity.LastName == request.LastName.Trim(), cancellationToken))
                return Error("Author with the same first name and last name exists!");

            var entity = new Author()
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim()
            };

            await CreateAsync(entity, cancellationToken);

            return Success("Author created successfully.", entity.Id);
        }
    }
}
