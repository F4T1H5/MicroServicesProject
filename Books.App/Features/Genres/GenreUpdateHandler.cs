using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Books.App.Features.Genres
{
    public class GenreUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(30)]
        public string Name { get; set; }
    }

    public class GenreUpdateHandler : Service<Genre>, IRequestHandler<GenreUpdateRequest, CommandResponse>
    {
        public GenreUpdateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(GenreUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbSet().AnyAsync(genreEntity => genreEntity.Id != request.Id && genreEntity.Name == request.Name.Trim(), cancellationToken))
                return Error("Genre with the same name exists!");

            var entity = await DbSet().SingleOrDefaultAsync(genreEntity => genreEntity.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");

            entity.Name = request.Name.Trim();

            await UpdateAsync(entity, cancellationToken);

            return Success("Genre updated successfully.", entity.Id);
        }
    }
}
