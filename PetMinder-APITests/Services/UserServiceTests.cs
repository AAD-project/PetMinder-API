using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Models;
using Api.Services.Implementations;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace Api.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _mockMapper = new Mock<IMapper>();
            _userService = new UserService(_mockUserManager.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnUsers_WhenUsersExist()
        {
            // Arrange
            var users = new List<User> { new User { Id = "user1", FirstName = "Test" } };
            var userDtos = new List<UserResponseDto> { new UserResponseDto { Id = "user1", FirstName = "Test", LastName = "User", Email = "test@example.com" } };

            _mockUserManager.Setup(um => um.Users).Returns(users.AsQueryable().BuildMockDbSet().Object);
            _mockMapper.Setup(m => m.Map<IEnumerable<UserResponseDto>>(users)).Returns(userDtos);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userDtos.Count, result.Count());
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnEmptyList_WhenNoUsersExist()
        {
            // Arrange
            _mockUserManager.Setup(um => um.Users).Returns(Enumerable.Empty<User>().AsQueryable().BuildMockDbSet().Object);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddUserAsync_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            var userDto = new UserCreateRequestDto { FirstName = "New", LastName = "User" };
            var user = new User { Id = "user1", FirstName = "New", LastName = "User" };
            var createdUserDto = new UserResponseDto { Id = "user1", FirstName = "New", LastName = "User", Email = "newuser@example.com" };

            _mockMapper.Setup(m => m.Map<User>(userDto)).Returns(user);
            _mockUserManager.Setup(um => um.CreateAsync(user)).ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(user)).Returns(createdUserDto);

            // Act
            var result = await _userService.AddUserAsync(userDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(createdUserDto.Id, result.Id);
        }

        [Fact]
        public async Task AddUserAsync_ShouldThrowInvalidOperationException_WhenUserCreationFails()
        {
            // Arrange
            var userDto = new UserCreateRequestDto { FirstName = "New", LastName = "User" };
            var user = new User { FirstName = "New", LastName = "User" };

            _mockMapper.Setup(m => m.Map<User>(userDto)).Returns(user);
            _mockUserManager.Setup(um => um.CreateAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to create user" }));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.AddUserAsync(userDto));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var updatedUserDto = new UserCreateRequestDto { FirstName = "Updated", LastName = "User" };
            var existingUser = new User { Id = userId, FirstName = "Original" };
            var updatedUserDtoResponse = new UserResponseDto { Id = userId, FirstName = "Updated", LastName = "User", Email = "updateduser@example.com" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserManager.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Success);
            _mockMapper.Setup(m => m.Map(updatedUserDto, existingUser)).Callback(() => existingUser.FirstName = updatedUserDto.FirstName);
            _mockMapper.Setup(m => m.Map<UserResponseDto>(existingUser)).Returns(updatedUserDtoResponse);

            // Act
            var result = await _userService.UpdateUserAsync(userId, updatedUserDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedUserDto.FirstName, result.FirstName);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.UpdateUserAsync("user1", new UserCreateRequestDto { FirstName = "First", LastName = "User" }));
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldThrowInvalidOperationException_WhenUpdateFails()
        {
            // Arrange
            var userId = "user1";
            var updatedUserDto = new UserCreateRequestDto { FirstName = "Updated", LastName = "User" };
            var existingUser = new User { Id = userId, FirstName = "Original" };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserManager.Setup(um => um.UpdateAsync(existingUser)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to update user" }));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.UpdateUserAsync(userId, updatedUserDto));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUser_WhenUserExists()
        {
            // Arrange
            var userId = "user1";
            var user = new User { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            // Act
            await _userService.DeleteUserAsync(userId);

            // Assert
            _mockUserManager.Verify(um => um.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowKeyNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByIdAsync("user1")).ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _userService.DeleteUserAsync("user1"));
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowInvalidOperationException_WhenDeletionFails()
        {
            // Arrange
            var userId = "user1";
            var user = new User { Id = userId };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId)).ReturnsAsync(user);
            _mockUserManager.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Failed to delete user" }));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.DeleteUserAsync(userId));
        }
    }
}

internal static class MockDbSetExtensions
{
    public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> source) where T : class
    {
        var mockDbSet = new Mock<DbSet<T>>();
        mockDbSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(source.GetEnumerator()));
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(source.Provider));
        mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(source.Expression);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(source.ElementType);
        mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(source.GetEnumerator());
        return mockDbSet;
    }
}

internal class TestAsyncQueryProvider<T> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
        var result = Execute(expression);
        return typeof(TResult).IsAssignableFrom(typeof(Task)) ? (TResult)(object)Task.FromResult(result) : (TResult)result;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider
    {
        get { return new TestAsyncQueryProvider<T>(this); }
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current
    {
        get { return _inner.Current; }
    }
}
