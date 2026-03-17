using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Books.App.Domain;

namespace Books.App.Features.Authors
{
    public class AuthorDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class AuthorDeleteHandler : Service<Author>, IRequestHandler<AuthorDeleteRequest, CommandResponse>
    {
        public AuthorDeleteHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(AuthorDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().Include(authorEntity => authorEntity.Books)
                .SingleOrDefaultAsync(authorEntity => authorEntity.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Author not found!");

            if (entity.Books.Count > 0)
                return Error("Author can't be deleted because it has relational books!");

            await DeleteAsync(entity, cancellationToken);

            return Success("Author deleted successfully.", entity.Id);
        }
    }
}
