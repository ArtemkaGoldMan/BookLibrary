using BaseLibrary.Validators;
using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities;

public class Book
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Title can't be empty or whitespace")]
    public required string Title { get; set; }

    [Required]
    [MaxLength(100)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Author name can't be empty or whitespace")]
    public required string Author { get; set; }

    [Required]
    [DateLessThanCurrent(ErrorMessage = "Published date must be less than the current date and time.")]
    public required DateTime PublishedDate { get; set; }

    [Required]
    [MaxLength(50)]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Genre can't be empty or whitespace")]
    public required string Genre { get; set; }
}
