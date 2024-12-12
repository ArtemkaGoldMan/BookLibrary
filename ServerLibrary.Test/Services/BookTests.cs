using BaseLibrary.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;
using ServerLibrary.Services.Implementations;
using System.ComponentModel.DataAnnotations;

namespace ServerLibrary.Test.Services;

public class BookTests : IDisposable
{
    private readonly AppDbContext _context;

    public BookTests()
    {
        _context = GetSqlServerDbContext();
        _context.Database.BeginTransaction();
    }

    private AppDbContext GetSqlServerDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    public void Dispose()
    {
        _context.Database.RollbackTransaction();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateBook_ShouldAddBookToDatabase()
    {
        // Arrange
        var service = new BookService(_context);
        var book = new Book { Title = "Test Book", Author = "John Doe", PublishedDate = DateTime.Now, Genre = "Fiction" };

        // Act
        var createdBook = await service.CreateBookAsync(book);
        _context.ChangeTracker.Clear();

        // Assert
        createdBook.Should().NotBeNull();
        createdBook.Title.Should().Be("Test Book");
        createdBook.Id.Should().BeGreaterThan(0);
        _context.Books.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrowException_WhenBookIsNull()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.CreateBookAsync(null!);
        _context.ChangeTracker.Clear();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks_WhenBooksExist()
    {
        // Arrange
        _context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
            new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" }
        );
        await _context.SaveChangesAsync();

        var service = new BookService(_context);
        _context.ChangeTracker.Clear();

        // Act
        var books = await service.GetAllBooksAsync();

        // Assert
        books.Should().NotBeNull();
        books.Should().HaveCount(2);
        books.Should().Contain(b => b.Title == "Book 1" && b.Author == "Author 1");
        books.Should().Contain(b => b.Title == "Book 2" && b.Author == "Author 2");
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnEmpty_WhenNoBooksExist()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        var books = await service.GetAllBooksAsync();

        // Assert
        books.Should().NotBeNull();
        books.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBooksInRangeAsync_ShouldReturnBooks_WhenRangeIsValid()
    {
        // Arrange
        _context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
            new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" },
            new Book { Title = "Book 3", Author = "Author 3", PublishedDate = DateTime.Now, Genre = "Genre 3" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        var books = await service.GetBooksInRangeAsync(1, 2);

        // Assert
        books.Should().HaveCount(2);
        books.ElementAt(0).Title.Should().Be("Book 1");
        books.ElementAt(1).Title.Should().Be("Book 2");
    }

    [Fact]
    public async Task GetBooksInRangeAsync_ShouldThrowArgumentException_WhenRangeIsInvalid()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.GetBooksInRangeAsync(3, 2);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid range specified.");
    }

    [Fact]
    public async Task GetBooksInRangeAsync_ShouldReturnEmpty_WhenRangeExceedsTotalBooks()
    {
        // Arrange
        _context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        var books = await service.GetBooksInRangeAsync(2, 3);

        // Assert
        books.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBooksInRangeAsync_ShouldReturnAllBooks_WhenRangeCoversEntireList()
    {
        // Arrange
        _context.Books.AddRange(
            new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
            new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" }
        );
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        var books = await service.GetBooksInRangeAsync(1, 2);

        // Assert
        books.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldReturnCorrectBook_WhenBookExists()
    {
        // Arrange
        var book = new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        var retrievedBook = await service.GetBookByIdAsync(book.Id);

        // Assert
        retrievedBook.Should().NotBeNull();
        retrievedBook!.Id.Should().Be(book.Id);
        retrievedBook.Title.Should().Be(book.Title);
        retrievedBook.Author.Should().Be(book.Author);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        var retrievedBook = await service.GetBookByIdAsync(1); // ID does not exist

        // Assert
        retrievedBook.Should().BeNull();
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldThrowException_WhenIdIsLessThanZero()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.GetBookByIdAsync(-1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid argument: ID must be greater than 0.");
    }

    [Fact]
    public async Task UpdateBook_ShouldModifyBookDetails()
    {
        // Arrange
        var book = new Book { Title = "Old Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        var updatedBook = new Book { Title = "New Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
        var result = await service.UpdateBookAsync(book.Id, updatedBook);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
    }

    [Fact]
    public async Task DeleteBook_ShouldRemoveBookFromDatabase()
    {
        // Arrange
        var book = new Book { Title = "Book to Delete", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        var service = new BookService(_context);

        // Act
        var result = await service.DeleteBookAsync(book.Id);
        _context.ChangeTracker.Clear();

        // Assert
        result.Should().BeTrue();
        _context.Books.Count().Should().Be(0);
    }

    [Fact]
    public void Book_Title_ShouldNotExceedMaxLength()
    {
        // Arrange
        var book = new Book
        {
            Title = new string('A', 101), // Exceeds max length of 100
            Author = "John Doe",
            PublishedDate = DateTime.Now,
            Genre = "Fiction"
        };

        // Act
        Action act = () =>
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        };
        _context.ChangeTracker.Clear();

        // Assert
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public async Task Book_Title_ShouldNotBeEmptyOrWhitespace()
    {
        // Arrange
        var book = new Book
        {
            Title = " ", // Invalid: Whitespace only
            Author = "Valid Author",
            PublishedDate = DateTime.Now,
            Genre = "Fiction"
        };

        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.CreateBookAsync(book);
        _context.ChangeTracker.Clear();

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public void Book_Author_ShouldBeRequired()
    {
        // Arrange
        var book = new Book
        {
            Title = "Valid Title",
            Author = null, // Author is required
            PublishedDate = DateTime.Now,
            Genre = "Fiction"
        };

        // Act
        Action act = () =>
        {
            _context.Books.Add(book);
            _context.SaveChanges(); // This triggers the database update and validation
        };
        _context.ChangeTracker.Clear();

        // Assert
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public void Book_Genre_ShouldNotExceedMaxLength()
    {
        // Arrange
        var book = new Book
        {
            Title = "Valid Title",
            Author = "Valid Author",
            PublishedDate = DateTime.Now,
            Genre = new string('B', 51) // Exceeds max length of 50
        };

        // Act
        Action act = () =>
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        };
        _context.ChangeTracker.Clear();

        // Assert
        act.Should().Throw<DbUpdateException>();
    }

    [Fact]
    public async Task CreateBook_ShouldFail_WhenPublishedDateIsInFuture()
    {
        // Arrange
        var service = new BookService(_context);
        var book = new Book
        {
            Title = "Future Book",
            Author = "John Doe",
            PublishedDate = DateTime.Now.AddDays(1), // Future date
            Genre = "Fiction"
        };

        // Act
        Func<Task> act = async () => await service.CreateBookAsync(book);
        _context.ChangeTracker.Clear();

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("Published date must be less than the current date and time.");
    }

    [Fact]
    public async Task CreateBook_ShouldSucceed_WhenPublishedDateIsInPast()
    {
        // Arrange
        var service = new BookService(_context);
        var book = new Book
        {
            Title = "Past Book",
            Author = "John Doe",
            PublishedDate = DateTime.Now.AddDays(-1), // Past date
            Genre = "Fiction"
        };

        // Act
        var createdBook = await service.CreateBookAsync(book);
        _context.ChangeTracker.Clear();

        // Assert
        createdBook.Should().NotBeNull();
        createdBook.PublishedDate.Should().Be(book.PublishedDate);
    }

    [Fact]
    public async Task CreateBook_ShouldSucceed_WhenPublishedDateIsToday()
    {
        // Arrange
        var service = new BookService(_context);
        var book = new Book
        {
            Title = "Today Book",
            Author = "John Doe",
            PublishedDate = DateTime.Now, // Today's date
            Genre = "Fiction"
        };

        // Act
        var createdBook = await service.CreateBookAsync(book);
        _context.ChangeTracker.Clear();

        // Assert
        createdBook.Should().NotBeNull();
        createdBook.PublishedDate.Should().Be(book.PublishedDate);
    }

    //
    [Fact]
    public async Task UpdateBook_ShouldReturnNull_WhenBookDoesNotExist()
    {
        // Arrange
        var service = new BookService(_context);
        var updatedBook = new Book { Title = "Non-existent Book", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };

        // Act
        var result = await service.UpdateBookAsync(-1, updatedBook);
        _context.ChangeTracker.Clear();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBook_ShouldThrowException_WhenUpdatedBookIsNull()
    {
        // Arrange
        var book = new Book { Title = "Original Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.UpdateBookAsync(book.Id, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateBook_ShouldFail_WhenUpdatedBookHasInvalidData()
    {
        // Arrange
        var book = new Book { Title = "Valid Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var service = new BookService(_context);

        var invalidUpdatedBook = new Book { Title = "", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };

        // Act
        Func<Task> act = async () => await service.UpdateBookAsync(book.Id, invalidUpdatedBook);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteBook_ShouldThrowException_WhenIdIsInvalid()
    {
        // Arrange
        var service = new BookService(_context);

        // Act
        Func<Task> act = async () => await service.DeleteBookAsync(0);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid argument: ID must be greater than 0.");
    }
}
