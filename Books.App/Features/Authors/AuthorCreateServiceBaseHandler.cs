using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Books.App.Domain;

namespace Books.App.Features.Authors
{
    public class AuthorCreateServiceBaseRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(25)]
        public string FirstName { get; set; }

        [Required, StringLength(25)]
        public string LastName { get; set; }
    }

    public class AuthorCreateServiceBaseHandler : ServiceBase, IRequestHandler<AuthorCreateServiceBaseRequest, CommandResponse>
    {
        private readonly BooksDb _db;

        public AuthorCreateServiceBaseHandler(BooksDb db)
        {
            _db = db;
        }

        public async Task<CommandResponse> Handle(AuthorCreateServiceBaseRequest request, CancellationToken cancellationToken)
        {
            if (await _db.Authors.AnyAsync(authorEntity => authorEntity.FirstName == request.FirstName.Trim()
                && authorEntity.LastName == request.LastName.Trim(), cancellationToken))
                return Error("Author with the same first name and last name exists!");

            var entity = new Author()
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim()
            };

            _db.Authors.Add(entity);

            await _db.SaveChangesAsync(cancellationToken);

            return Success("Author created successfully.", entity.Id);
        }
    }
}
