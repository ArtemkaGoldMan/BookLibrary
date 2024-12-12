using BaseLibrary.Entities;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Contracts;
using System.ComponentModel.DataAnnotations;

namespace ServerLibrary.Services.Implementations;

public class BookService : IBookService
{
    private readonly AppDbContext _context;

    public BookService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        try
        {
            return await _context.Books.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(GetAllBooksAsync)}: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<Book>> GetBooksInRangeAsync(int i, int j)
    {
        try
        {
            if (i <= 0 || j < i) throw new ArgumentException("Invalid range specified.");

            return await _context.Books.Skip(i - 1).Take(j - i + 1).ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(GetBooksInRangeAsync)}: {ex.Message}");
            throw;
        }
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        if (id < 1)
        {
            throw new ArgumentException("Invalid argument: ID must be greater than 0.");
        }
        try
        {
            return await _context.Books.FindAsync(id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(GetBookByIdAsync)}: {ex.Message}");
            throw;
        }
    }

    public async Task<Book> CreateBookAsync(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        var validationContext = new ValidationContext(book);
        Validator.ValidateObject(book, validationContext, validateAllProperties: true);

        try
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(CreateBookAsync)}: {ex.Message}");
            throw;
        }
    }

    public async Task<Book?> UpdateBookAsync(int id, Book updatedBook)
    {
        ArgumentNullException.ThrowIfNull(updatedBook);

        var validationContext = new ValidationContext(updatedBook);
        Validator.ValidateObject(updatedBook, validationContext, validateAllProperties: true);

        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return null;

            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Genre = updatedBook.Genre;
            book.PublishedDate = updatedBook.PublishedDate;

            await _context.SaveChangesAsync();
            return book;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(UpdateBookAsync)}: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        if (id < 1)
        {
            throw new ArgumentException("Invalid argument: ID must be greater than 0.");
        }

        try
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {nameof(DeleteBookAsync)}: {ex.Message}");
            throw;
        }
    }
}
