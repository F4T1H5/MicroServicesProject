using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.App.Features.Genres
{
    public class GenreDeleteRequest : Request, IRequest<CommandResponse>
    {
    }

    public class GenreDeleteHandler : Service<Genre>, IRequestHandler<GenreDeleteRequest, CommandResponse>
    {
        public GenreDeleteHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Genre> DbSet()
        {
            return base.DbSet().Include(genreEntity => genreEntity.BookGenres);
        }

        public async Task<CommandResponse> Handle(GenreDeleteRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSet().SingleOrDefaultAsync(genreEntity => genreEntity.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");

            Delete(entity.BookGenres);

            await DeleteAsync(entity, cancellationToken);

            return Success("Genre deleted successfully.", entity.Id);
        }
    }
}
