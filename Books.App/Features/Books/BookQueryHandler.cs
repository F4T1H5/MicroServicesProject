using Books.App.Domain;
using Books.App.Features.Authors;
using CORE.APP.Models;
using CORE.APP.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Books.App.Features.Books
{
    public class BookQueryRequest : Request, IRequest<IQueryable<BookQueryResponse>>
    {
        public string Name { get; set; }
        public short? NumberOfPagesStart { get; set; }
        public short? NumberOfPagesEnd { get; set; }
        public DateTime? PublishDateStart { get; set; }
        public DateTime? PublishDateEnd { get; set; }
        public decimal? PriceStart { get; set; }
        public decimal? PriceEnd { get; set; }
        public bool? IsTopSeller { get; set; }
        public int? AuthorId { get; set; }
        public List<int> GenreIds { get; set; } = new List<int>();
    }

    public class GenreQueryResponse : Response
    {
        public string Name { get; set; }
    }

    public class BookQueryResponse : Response
    {
        public string Name { get; set; }
        public short? NumberOfPages { get; set; }
        public DateTime PublishDate { get; set; }
        public decimal Price { get; set; }
        public bool IsTopSeller { get; set; }
        public int AuthorId { get; set; }
        public List<int> GenreIds { get; set; }

        public string PublishDateF { get; set; }
        public string PriceF { get; set; }
        public string IsTopSellerF { get; set; }
        public string AuthorFullName { get; set; }
        public List<string> GenresF { get; set; }

        public AuthorQueryResponse Author { get; set; }
        public List<GenreQueryResponse> Genres { get; set; }
    }

    public class BookQueryHandler : Service<Book>, IRequestHandler<BookQueryRequest, IQueryable<BookQueryResponse>>
    {
        public BookQueryHandler(DbContext db) : base(db)
        {
        }

        protected override IQueryable<Book> DbSet()
        {
            return base.DbSet()
                .AsNoTracking()
                .Include(bookEntity => bookEntity.Author)
                .Include(bookEntity => bookEntity.BookGenres).ThenInclude(bookGenreEntity => bookGenreEntity.Genre)
                .OrderByDescending(bookEntity => bookEntity.IsTopSeller)
                .ThenByDescending(bookEntity => bookEntity.PublishDate)
                .ThenBy(bookEntity => bookEntity.Name);
        }

        public Task<IQueryable<BookQueryResponse>> Handle(BookQueryRequest request, CancellationToken cancellationToken)
        {
            var entityQuery = DbSet();

            if (!string.IsNullOrWhiteSpace(request.Name))
                entityQuery = entityQuery.Where(bookEntity => bookEntity.Name.Contains(request.Name.Trim()));

            if (request.NumberOfPagesStart.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.NumberOfPages.HasValue && bookEntity.NumberOfPages.Value >= request.NumberOfPagesStart.Value);

            if (request.NumberOfPagesEnd.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.NumberOfPages.HasValue && bookEntity.NumberOfPages.Value <= request.NumberOfPagesEnd.Value);

            if (request.PublishDateStart.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.PublishDate.Date >= request.PublishDateStart.Value.Date);

            if (request.PublishDateEnd.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.PublishDate.Date <= request.PublishDateEnd.Value.Date);

            if (request.PriceStart.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.Price >= request.PriceStart.Value);

            if (request.PriceEnd.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.Price <= request.PriceEnd.Value);

            if (request.IsTopSeller.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.IsTopSeller == request.IsTopSeller.Value);

            if (request.AuthorId.HasValue)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.AuthorId == request.AuthorId.Value);

            if (request.GenreIds.Count > 0)
                entityQuery = entityQuery.Where(bookEntity => bookEntity.BookGenres.Any(bookGenreEntity => request.GenreIds.Contains(bookGenreEntity.GenreId)));

            var query = entityQuery.Select(bookEntity => new BookQueryResponse
            {
                Id = bookEntity.Id,
                Name = bookEntity.Name,
                NumberOfPages = bookEntity.NumberOfPages,
                PublishDate = bookEntity.PublishDate,
                Price = bookEntity.Price,
                IsTopSeller = bookEntity.IsTopSeller,
                AuthorId = bookEntity.AuthorId,
                GenreIds = bookEntity.GenreIds,

                PublishDateF = bookEntity.PublishDate.ToShortDateString(),
                PriceF = bookEntity.Price.ToString("C2"),
                IsTopSellerF = bookEntity.IsTopSeller ? "Top Seller" : "Standard",
                AuthorFullName = bookEntity.Author.FirstName + " " + bookEntity.Author.LastName,
                GenresF = bookEntity.BookGenres.Select(bookGenreEntity => bookGenreEntity.Genre.Name).ToList(),

                Author = new AuthorQueryResponse
                {
                    Id = bookEntity.Author.Id,
                    FirstName = bookEntity.Author.FirstName,
                    LastName = bookEntity.Author.LastName
                },
                Genres = bookEntity.BookGenres.Select(bookGenreEntity => new GenreQueryResponse
                {
                    Id = bookGenreEntity.Genre.Id,
                    Name = bookGenreEntity.Genre.Name
                }).ToList()
            });

            return Task.FromResult(query);
        }
    }
}
