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
    public class AuthorDeleteServiceBaseRequest : Request, IRequest<CommandResponse>
    {
    }

    public class AuthorDeleteServiceBaseHandler : ServiceBase, IRequestHandler<AuthorDeleteServiceBaseRequest, CommandResponse>
    {
        private readonly BooksDb _db;

        public AuthorDeleteServiceBaseHandler(BooksDb db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(AuthorDeleteServiceBaseRequest request, CancellationToken cancellationToken)
        {
            var entity = await _db.Authors.Include(authorEntity => authorEntity.Books)
                .SingleOrDefaultAsync(authorEntity => authorEntity.Id == request.Id, cancellationToken);

            if (entity is null)
                return Error("Author not found!");

            if (entity.Books.Count > 0)
                return Error("Author can't be deleted because it has relational books!");

            _db.Authors.Remove(entity);

            await _db.SaveChangesAsync(cancellationToken);

            return Success("Author deleted successfully.", entity.Id);
        }
    }
}
