using Books.App.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Books.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly BooksDb _db;
        private readonly IWebHostEnvironment _environment;

        public DatabaseController(BooksDb db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpGet, Route("~/api/SeedDb")]
        public IActionResult Seed()
        {
            // Can be uncommented to check if the running application's environment is not development, prevent seeding initial data to the database.
            //if (!_environment.IsDevelopment())
            //    return BadRequest("The seed operation can only be performed in development environment!");

            // Remove all existing book-genre relationships
            var bookGenres = _db.BookGenres.ToList();
            _db.BookGenres.RemoveRange(bookGenres);

            // Remove all existing books
            var books = _db.Books.ToList();
            _db.Books.RemoveRange(books);

            // Remove all existing genres
            var genres = _db.Genres.ToList();
            _db.Genres.RemoveRange(genres);

            // Remove all existing authors
            var authors = _db.Authors.ToList();
            _db.Authors.RemoveRange(authors);

            // Reset the ID values of all tables so when a new record is inserted, ID will start from 1.
            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='BookGenres';");
            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Books';");
            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Genres';");
            _db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE SET SEQ=0 WHERE NAME='Authors';");

            // Add default genres
            _db.Genres.Add(new Genre() { Name = "Fiction" });
            _db.Genres.Add(new Genre() { Name = "Science Fiction" });
            _db.Genres.Add(new Genre() { Name = "History" });

            _db.SaveChanges();

            // Add default authors and books
            _db.Authors.Add(new Author()
            {
                FirstName = "George",
                LastName = "Orwell",
                Books = new List<Book>()
                {
                    new Book()
                    {
                        Name = "1984",
                        NumberOfPages = 328,
                        PublishDate = new DateTime(1949, 6, 8),
                        Price = 19.99M,
                        IsTopSeller = true,
                        BookGenres = new List<BookGenre>()
                        {
                            new BookGenre() { GenreId = _db.Genres.SingleOrDefault(g => g.Name == "Fiction").Id },
                            new BookGenre() { GenreId = _db.Genres.SingleOrDefault(g => g.Name == "Science Fiction").Id }
                        }
                    }
                }
            });

            _db.Authors.Add(new Author()
            {
                FirstName = "Yuval Noah",
                LastName = "Harari",
                Books = new List<Book>()
                {
                    new Book()
                    {
                        Name = "Sapiens",
                        NumberOfPages = 443,
                        PublishDate = new DateTime(2011, 1, 1),
                        Price = 24.50M,
                        IsTopSeller = true,
                        BookGenres = new List<BookGenre>()
                        {
                            new BookGenre() { GenreId = _db.Genres.SingleOrDefault(g => g.Name == "History").Id }
                        }
                    }
                }
            });

            _db.SaveChanges();

            return Ok("Database seed successful.");
        }
    }
}
