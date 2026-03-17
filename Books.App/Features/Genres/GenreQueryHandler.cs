using Books.App.Domain;
using Books.App.Features.Authors;
using Books.App.Features.Books;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.App.Features.Genres
{
    public class GenreQueryRequest : Request, IRequest<IQueryable<GenreQueryResponse>>
    {
    }

    public class GenreQueryResponse : Response
    {
        public string Name { get; set; }

        public int BookCount { get; set; }
        public string BooksF { get; set; }
        public List<BookQueryResponse> Books { get; set; }
    }

    public class GenreQueryHandler : Service<Genre>, IRequestHandler<GenreQueryRequest, IQueryable<GenreQueryResponse>>
    {
        public GenreQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Genre> DbSet()
        {
            return base.DbSet()
                .AsNoTracking()
                .Include(genreEntity => genreEntity.BookGenres).ThenInclude(bookGenreEntity => bookGenreEntity.Book).ThenInclude(bookEntity => bookEntity.Author)
                .Include(genreEntity => genreEntity.BookGenres).ThenInclude(bookGenreEntity => bookGenreEntity.Book).ThenInclude(bookEntity => bookEntity.BookGenres).ThenInclude(bookGenreEntity => bookGenreEntity.Genre)
                .OrderBy(genreEntity => genreEntity.Name);
        }

        public Task<IQueryable<GenreQueryResponse>> Handle(GenreQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbSet().Select(genreEntity => new GenreQueryResponse
            {
                Id = genreEntity.Id,
                Name = genreEntity.Name,

                BookCount = genreEntity.BookGenres.Count,
                BooksF = string.Join(", ", genreEntity.BookGenres.Select(bookGenreEntity => bookGenreEntity.Book.Name)),

                Books = genreEntity.BookGenres.Select(bookGenreEntity => new BookQueryResponse
                {
                    Id = bookGenreEntity.Book.Id,
                    Name = bookGenreEntity.Book.Name,
                    NumberOfPages = bookGenreEntity.Book.NumberOfPages,
                    PublishDate = bookGenreEntity.Book.PublishDate,
                    Price = bookGenreEntity.Book.Price,
                    IsTopSeller = bookGenreEntity.Book.IsTopSeller,
                    AuthorId = bookGenreEntity.Book.AuthorId,
                    GenreIds = bookGenreEntity.Book.GenreIds,

                    PublishDateF = bookGenreEntity.Book.PublishDate.ToShortDateString(),
                    PriceF = bookGenreEntity.Book.Price.ToString("C2"),
                    IsTopSellerF = bookGenreEntity.Book.IsTopSeller ? "Top Seller" : "Standard",
                    AuthorFullName = bookGenreEntity.Book.Author.FirstName + " " + bookGenreEntity.Book.Author.LastName,
                    GenresF = bookGenreEntity.Book.BookGenres.Select(bookBookGenreEntity => bookBookGenreEntity.Genre.Name).ToList(),

                    Author = new AuthorQueryResponse
                    {
                        Id = bookGenreEntity.Book.Author.Id,
                        FirstName = bookGenreEntity.Book.Author.FirstName,
                        LastName = bookGenreEntity.Book.Author.LastName
                    },
                    Genres = bookGenreEntity.Book.BookGenres.Select(bookBookGenreEntity => new global::Books.App.Features.Books.GenreQueryResponse
                    {
                        Id = bookBookGenreEntity.Genre.Id,
                        Name = bookBookGenreEntity.Genre.Name
                    }).ToList()
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}
