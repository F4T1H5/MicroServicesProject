using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Books.App.Features.Books
{
    public class BookUpdateRequest : Request, IRequest<CommandResponse>
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Range(1, short.MaxValue)]
        public short? NumberOfPages { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }

        [Range(0.01, (double)decimal.MaxValue)]
        public decimal Price { get; set; }

        public bool IsTopSeller { get; set; }

        [Required]
        public int AuthorId { get; set; }

        public List<int> GenreIds { get; set; } = new List<int>();
    }

    public class BookUpdateHandler : Service<Book>, IRequestHandler<BookUpdateRequest, CommandResponse>
    {
        public BookUpdateHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Book> DbSet()
        {
            return base.DbSet().Include(bookEntity => bookEntity.BookGenres);
        }

        public async Task<CommandResponse> Handle(BookUpdateRequest request, CancellationToken cancellationToken)
        {
            if (!await DbSet<Author>().AnyAsync(authorEntity => authorEntity.Id == request.AuthorId, cancellationToken))
                return Error("Author not found!");

            if (await base.DbSet().AnyAsync(bookEntity => bookEntity.Id != request.Id && bookEntity.AuthorId == request.AuthorId && bookEntity.Name == request.Name.Trim(), cancellationToken))
                return Error("Book with the same name and author exists!");

            var genreIds = request.GenreIds.Distinct().ToList();
            if (genreIds.Count > 0)
            {
                var validGenreCount = await DbSet<Genre>().CountAsync(genreEntity => genreIds.Contains(genreEntity.Id), cancellationToken);
                if (validGenreCount != genreIds.Count)
                    return Error("One or more genres are invalid!");
            }

            var entity = await DbSet().SingleOrDefaultAsync(bookEntity => bookEntity.Id == request.Id, cancellationToken);
            if (entity is null)
                return Error("Book not found!");

            Delete(entity.BookGenres);

            entity.Name = request.Name.Trim();
            entity.NumberOfPages = request.NumberOfPages;
            entity.PublishDate = request.PublishDate;
            entity.Price = request.Price;
            entity.IsTopSeller = request.IsTopSeller;
            entity.AuthorId = request.AuthorId;
            entity.GenreIds = genreIds;

            await UpdateAsync(entity, cancellationToken);

            return Success("Book updated successfully.", entity.Id);
        }
    }
}
