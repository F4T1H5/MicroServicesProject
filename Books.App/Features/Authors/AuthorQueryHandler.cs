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
    public class AuthorQueryRequest : Request, IRequest<IQueryable<AuthorQueryResponse>>
    {
    }

    public class AuthorQueryResponse : Response
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AuthorQueryHandler : Service<Author>, IRequestHandler<AuthorQueryRequest, IQueryable<AuthorQueryResponse>>
    {
        public AuthorQueryHandler(DbContext db) : base(db)
        {
        }

        public Task<IQueryable<AuthorQueryResponse>> Handle(AuthorQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(authorEntity => new AuthorQueryResponse()
            {
                Id = authorEntity.Id,
                FirstName = authorEntity.FirstName,
                LastName = authorEntity.LastName
            });

            return Task.FromResult(query);
        }
    }
}
