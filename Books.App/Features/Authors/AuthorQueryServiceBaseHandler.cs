using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Books.App.Domain;

namespace Books.App.Features.Authors
{
    public class AuthorQueryServiceBaseRequest : Request, IRequest<IQueryable<AuthorQueryServiceBaseResponse>>
    {
    }

    public class AuthorQueryServiceBaseResponse : Response
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AuthorQueryServiceBaseHandler : ServiceBase, IRequestHandler<AuthorQueryServiceBaseRequest, IQueryable<AuthorQueryServiceBaseResponse>>
    {
        private readonly BooksDb _db;

        public AuthorQueryServiceBaseHandler(BooksDb db)
        {
            _db = db;
        }

        public Task<IQueryable<AuthorQueryServiceBaseResponse>> Handle(AuthorQueryServiceBaseRequest request, CancellationToken cancellationToken)
        {
            var query = _db.Authors.Select(authorEntity => new AuthorQueryServiceBaseResponse()
            {
                Id = authorEntity.Id,
                FirstName = authorEntity.FirstName,
                LastName = authorEntity.LastName
            });

            return Task.FromResult(query);
        }
    }
}
