using Books.App.Domain;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Books.App.Features.Books
{
    public class BookCreateRequest : Request, IRequest<CommandResponse>
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

    public class BookCreateHandler : Service<Book>, IRequestHandler<BookCreateRequest, CommandResponse>
    {
        public BookCreateHandler(DbContext db) : base(db)
        {
        }

        public async Task<CommandResponse> Handle(BookCreateRequest request, CancellationToken cancellationToken)
        {
            if (!await DbSet<Author>().AnyAsync(authorEntity => authorEntity.Id == request.AuthorId, cancellationToken))
                return Error("Author not found!");

            if (await DbSet().AnyAsync(bookEntity => bookEntity.AuthorId == request.AuthorId && bookEntity.Name == request.Name.Trim(), cancellationToken))
                return Error("Book with the same name and author exists!");

            var genreIds = request.GenreIds.Distinct().ToList();
            if (genreIds.Count > 0)
            {
                var validGenreCount = await DbSet<Genre>().CountAsync(genreEntity => genreIds.Contains(genreEntity.Id), cancellationToken);
                if (validGenreCount != genreIds.Count)
                    return Error("One or more genres are invalid!");
            }

            var entity = new Book
            {
                Name = request.Name.Trim(),
                NumberOfPages = request.NumberOfPages,
                PublishDate = request.PublishDate,
                Price = request.Price,
                IsTopSeller = request.IsTopSeller,
                AuthorId = request.AuthorId,
                GenreIds = genreIds
            };

            await CreateAsync(entity, cancellationToken);

            return Success("Book created successfully.", entity.Id);
        }
    }
}
