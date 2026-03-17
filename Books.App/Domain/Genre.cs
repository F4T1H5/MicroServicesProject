using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Books.App.Domain
{
    public class Genre : Entity
    {
        [Required, StringLength(30)]
        public string Name { get; set; }

        public List<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        [NotMapped]
        public List<int> BookIds
        {
            get => BookGenres.Select(bookGenreEntity => bookGenreEntity.BookId).ToList();
            set => BookGenres = value.Select(bookIdValue => new BookGenre() { BookId = bookIdValue }).ToList();
        }
    }
}
