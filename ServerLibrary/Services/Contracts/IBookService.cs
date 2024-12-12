using BaseLibrary.Entities;

namespace ServerLibrary.Services.Contracts;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<IEnumerable<Book>> GetBooksInRangeAsync(int i, int j);
    Task<Book?> GetBookByIdAsync(int id);
    Task<Book> CreateBookAsync(Book book);
    Task<Book?> UpdateBookAsync(int id, Book updatedBook);
    Task<bool> DeleteBookAsync(int id);
}
