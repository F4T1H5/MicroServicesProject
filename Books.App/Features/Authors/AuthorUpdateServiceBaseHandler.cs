using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Books.App.Domain;

namespace Books.App.Features.Authors
{
    public class AuthorUpdateServiceBaseRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(25)]
        public string FirstName { get; set; }

        [Required, StringLength(25)]
        public string LastName { get; set; }
    }

    public class AuthorUpdateServiceBaseHandler : ServiceBase, IRequestHandler<AuthorUpdateServiceBaseRequest, CommandResponse>
    {
        private readonly BooksDb _db;

        public AuthorUpdateServiceBaseHandler(BooksDb db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(AuthorUpdateServiceBaseRequest request, CancellationToken cancellationToken)
        {
            if (await _db.Authors.AnyAsync(authorEntity => authorEntity.Id != request.Id
                && authorEntity.FirstName == request.FirstName.Trim()
                && authorEntity.LastName == request.LastName.Trim(), cancellationToken))
                return Error("Author with the same first name and last name exists!");

            var entity = await _db.Authors.FindAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Author not found!");

            entity.FirstName = request.FirstName.Trim();
            entity.LastName = request.LastName.Trim();

            _db.Authors.Update(entity);

            await _db.SaveChangesAsync(cancellationToken);

            return Success("Author updated successfully.", entity.Id);
        }
    }
}
