using BaseLibrary.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ServerLibrary.Data;

namespace ServerLibrary.Test.Services
{
    public class BookValidationTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public void Book_Title_ShouldNotExceedMaxLength()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = new string('A', 101), // Exceeds max length of 100
                Author = "John Doe",
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*The value of 'Title' exceeds the maximum length of 100.*");
        }



        [Fact]
        public void Book_Title_ShouldNotBeEmptyOrWhitespace()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = " ", // Invalid: Whitespace only
                Author = "Valid Author",
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*cannot be empty or whitespace.*");
        }

        [Fact]
        public void Book_Author_ShouldNotBeEmptyOrWhitespace()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = "Valid Title",
                Author = " ", // Invalid: Whitespace only
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*cannot be empty or whitespace.*");
        }


        [Fact]
        public void Book_Author_ShouldBeRequired()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = "Valid Title",
                Author = null, // Author is required
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*Cannot insert the value NULL into column 'Author'.*");
        }

        [Fact]
        public void Book_Genre_ShouldNotExceedMaxLength()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = "Valid Title",
                Author = "Valid Author",
                PublishedDate = DateTime.Now,
                Genre = new string('B', 51) // Exceeds max length of 50
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*The value of 'Genre' exceeds the maximum length of 50.*");
        }

        [Fact]
        public void Book_PublishedDate_ShouldBeRequired()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = "Valid Title",
                Author = "Valid Author",
                Genre = "Fiction",
                PublishedDate = default // PublishedDate is required
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*Cannot insert the value NULL into column 'PublishedDate'.*");
        }

        [Fact]
        public void Book_PublishedDate_ShouldNotBeInFuture()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var book = new Book
            {
                Title = "Valid Title",
                Author = "Valid Author",
                PublishedDate = DateTime.Now.AddDays(1), // Invalid: Future date
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*PublishedDate cannot be in the future.*");
        }

    }
}
