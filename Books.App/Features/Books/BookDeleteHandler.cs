using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.App.Features.Books
{
    public class BookDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class BookDeleteHandler : Service<Book>, IRequestHandler<BookDeleteRequest, CommandResponse>
    {
        public BookDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Book> DbSet()
        {
            return base.DbSet().Include(bookEntity => bookEntity.BookGenres);
        }

        public async Task<CommandResponse> Handle(BookDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(bookEntity => bookEntity.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Book not found!");

            Delete(entity.BookGenres);

            await DeleteAsync(entity, cancellationToken);

            return Success("Book deleted successfully.", entity.Id);
        }
    }
}
