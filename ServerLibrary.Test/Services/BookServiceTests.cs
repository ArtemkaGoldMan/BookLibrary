using Microsoft.EntityFrameworkCore;
using BaseLibrary.Entities;
using ServerLibrary.Data;
using ServerLibrary.Services.Implementations;
using FluentAssertions;

namespace ServerLibrary.Test.Services
{
    public class BookServiceTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InMemoryTestDb;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        //TESTS//CreateBookAsync//

        [Fact]
        public async Task CreateBook_ShouldAddBookToDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var service = new BookService(context);
            var book = new Book { Title = "Test Book", Author = "John Doe", PublishedDate = DateTime.Now, Genre = "Fiction" };

            // Act
            var createdBook = await service.CreateBookAsync(book);
            context.ChangeTracker.Clear();

            // Assert
            createdBook.Should().NotBeNull();
            createdBook.Title.Should().Be("Test Book");
            createdBook.Id.Should().BeGreaterThan(0);
            context.Books.Count().Should().Be(1);
        }

        [Fact]
        public async Task CreateBookAsync_ShouldThrowException_WhenBookIsNull()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new BookService(context);

            context.Database.BeginTransaction();
            // Act
            Func<Task> act = async () => await service.CreateBookAsync(null!);
            context.ChangeTracker.Clear();

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        //TESTS//GetAllBooksAsync//
        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnAllBooks_WhenBooksExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            context.Books.AddRange(
                new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
                new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" }
            );
            await context.SaveChangesAsync();

            var service = new BookService(context);
            context.ChangeTracker.Clear();
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
            var context = GetInMemoryDbContext();
            var service = new BookService(context);

            // Act
            var books = await service.GetAllBooksAsync();

            // Assert
            books.Should().NotBeNull();
            books.Should().BeEmpty();
        }


        //TESTS//GetBooksInRangeAsync//

        [Fact]
        public async Task GetBooksInRangeAsync_ShouldReturnBooks_WhenRangeIsValid()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            context.Books.AddRange(
                new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
                new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" },
                new Book { Title = "Book 3", Author = "Author 3", PublishedDate = DateTime.Now, Genre = "Genre 3" }
            );
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var service = new BookService(context);

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
            var context = GetInMemoryDbContext();
            var service = new BookService(context);

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
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            context.Books.AddRange(
                new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" }
            );
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var service = new BookService(context);

            // Act
            var books = await service.GetBooksInRangeAsync(2, 3);

            // Assert
            books.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBooksInRangeAsync_ShouldReturnAllBooks_WhenRangeCoversEntireList()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            context.Books.AddRange(
                new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" },
                new Book { Title = "Book 2", Author = "Author 2", PublishedDate = DateTime.Now, Genre = "Genre 2" }
            );
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var service = new BookService(context);

            // Act
            var books = await service.GetBooksInRangeAsync(1, 2);

            // Assert
            books.Should().HaveCount(2);
        }


        //TESTS//GetBookById//

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnCorrectBook_WhenBookExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book { Title = "Book 1", Author = "Author 1", PublishedDate = DateTime.Now, Genre = "Genre 1" };
            context.Books.Add(book);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var service = new BookService(context);

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
            var context = GetInMemoryDbContext();
            var service = new BookService(context);

            // Act
            var retrievedBook = await service.GetBookByIdAsync(1); // ID does not exist

            // Assert
            retrievedBook.Should().BeNull();
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldThrowException_WhenIdIsLessThanZero()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var service = new BookService(context);

            // Act
            Func<Task> act = async () => await service.GetBookByIdAsync(-1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("Invalid argument: ID must be greater than 0.");
        }

        //TESTS//UpdateBook//

        [Fact]
        public async Task UpdateBook_ShouldModifyBookDetails()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book { Title = "Old Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
            context.Books.Add(book);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var service = new BookService(context);

            // Act
            var updatedBook = new Book { Title = "New Title", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
            var result = await service.UpdateBookAsync(book.Id, updatedBook);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("New Title");
        }

        //TESTS//DeleteBook//

        [Fact]
        public async Task DeleteBook_ShouldRemoveBookFromDatabase()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book { Title = "Book to Delete", Author = "Author", PublishedDate = DateTime.Now, Genre = "Genre" };
            context.Books.Add(book);
            await context.SaveChangesAsync();

            var service = new BookService(context);

            // Act
            var result = await service.DeleteBookAsync(book.Id);
            context.ChangeTracker.Clear();

            // Assert
            result.Should().BeTrue();
            //context.Books.Count().Should().Be(0);
        }
    }
}
