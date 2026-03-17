using CORE.APP.Domain;
using System.ComponentModel.DataAnnotations;

namespace Books.App.Domain
{
    public class Author : Entity
    {
        [Required]
        [StringLength(25)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(25)]
        public string LastName { get; set; }

        public List<Book> Books { get; set; } = new List<Book>();
    }
}
