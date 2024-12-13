using BaseLibrary.Entities;

namespace ServerLibrary.Services.Contracts;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookAsync(int id, Book updatedBook);
    Task<bool> DeleteBookAsync(int id);
}
