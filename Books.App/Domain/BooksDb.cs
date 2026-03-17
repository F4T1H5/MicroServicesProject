using Microsoft.EntityFrameworkCore;

namespace Books.App.Domain
{
    public class BooksDb : DbContext
    {
        public DbSet<Author> Authors { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<Genre> Genres { get; set; }

        public DbSet<BookGenre> BookGenres { get; set; }


        public BooksDb(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Index configurations:
            // Name data of the Genres table can not have multiple same values.
            modelBuilder.Entity<Genre>().HasIndex(genreEntity => genreEntity.Name).IsUnique();

            // Composite index on FirstName and LastName for optimizing searches involving both fields.
            modelBuilder.Entity<Author>().HasIndex(authorEntity => new { authorEntity.FirstName, authorEntity.LastName });

            // Defining indices for optimizing query performance on frequently searched properties.
            modelBuilder.Entity<Book>().HasIndex(bookEntity => bookEntity.AuthorId);

            // Prevent duplicate Book-Genre pairs.
            modelBuilder.Entity<BookGenre>().HasIndex(bookGenreEntity => new { bookGenreEntity.BookId, bookGenreEntity.GenreId }).IsUnique();

            // Relationship configurations:
            // Configuration should start with the entities that have the foreign keys.
            modelBuilder.Entity<BookGenre>()
                .HasOne(bookGenreEntity => bookGenreEntity.Book) // each BookGenre entity has one related Book entity
                .WithMany(bookEntity => bookEntity.BookGenres) // each Book entity has many related BookGenre entities
                .HasForeignKey(bookGenreEntity => bookGenreEntity.BookId) // the foreign key property in the BookGenre entity that
                                                                          // references the primary key of the related Book entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a Book entity if there are related BookGenre entities

            modelBuilder.Entity<BookGenre>()
                .HasOne(bookGenreEntity => bookGenreEntity.Genre) // each BookGenre entity has one related Genre entity
                .WithMany(genreEntity => genreEntity.BookGenres) // each Genre entity has many related BookGenre entities
                .HasForeignKey(bookGenreEntity => bookGenreEntity.GenreId) // the foreign key property in the BookGenre entity that
                                                                            // references the primary key of the related Genre entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of a Genre entity if there are related BookGenre entities

            modelBuilder.Entity<Book>()
                .HasOne(bookEntity => bookEntity.Author) // each Book entity has one related Author entity
                .WithMany(authorEntity => authorEntity.Books) // each Author entity has many related Book entities
                .HasForeignKey(bookEntity => bookEntity.AuthorId) // the foreign key property in the Book entity that
                                                                  // references the primary key of the related Author entity
                .OnDelete(DeleteBehavior.NoAction); // prevents deletion of an Author entity if there are related Book entities
        }
    }
}
