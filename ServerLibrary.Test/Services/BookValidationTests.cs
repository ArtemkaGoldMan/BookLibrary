﻿using BaseLibrary.Entities;
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
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InMemoryTestDb;Trusted_Connection=True;MultipleActiveResultSets=true")
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public void Book_Title_ShouldNotExceedMaxLength()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book
            {
                Title = new string('A', 101), // Exceeds max length of 100
                Author = "John Doe",
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*The value of 'Title' exceeds the maximum length of 100.*");
        }



        [Fact]
        public void Book_Title_ShouldNotBeEmptyOrWhitespace()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book
            {
                Title = " ", // Invalid: Whitespace only
                Author = "Valid Author",
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () => context.Books.Add(book);
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*cannot be empty or whitespace.*");
        }

        [Fact]
        public void Book_Author_ShouldNotBeEmptyOrWhitespace()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book
            {
                Title = "Valid Title",
                Author = " ", // Invalid: Whitespace only
                PublishedDate = DateTime.Now,
                Genre = "Fiction"
            };

            // Act
            Action act = () =>
            {
                context.Books.Add(book);
                context.SaveChanges();
            };
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*cannot be empty or whitespace.*");
        }


        [Fact]
        public void Book_Author_ShouldBeRequired()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
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
                context.Books.Add(book);
                context.SaveChanges(); // This triggers the database update and validation
            };
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>();
        }

        [Fact]
        public void Book_Genre_ShouldNotExceedMaxLength()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
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
                context.Books.Add(book);
                context.SaveChanges();
            };
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*The value of 'Genre' exceeds the maximum length of 50.*");
        }

        [Fact]
        public void Book_PublishedDate_ShouldBeRequired()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book
            {
                Title = "Valid Title",
                Author = "Valid Author",
                Genre = "Fiction",
                PublishedDate = default // PublishedDate is required
            };

            // Act
            Action act = () =>
            {
                context.Books.Add(book);
                context.SaveChanges();
            };
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*Cannot insert the value NULL into column 'PublishedDate'.*");
        }

        [Fact]
        public void Book_PublishedDate_ShouldNotBeInFuture()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Database.BeginTransaction();
            var book = new Book
            {
                Title = "Valid Title",
                Author = "Valid Author",
                PublishedDate = DateTime.Now.AddDays(1), // Invalid: Future date
                Genre = "Fiction"
            };

            // Act
            Action act = () =>
            {
                context.Books.Add(book);
                context.SaveChanges();
            };
            context.ChangeTracker.Clear();

            // Assert
            act.Should().Throw<DbUpdateException>()
                .WithMessage("*PublishedDate cannot be in the future.*");
        }

    }
}
