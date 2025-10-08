// <copyright file="UserServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Tests.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="UserService"/> following Microsoft best practices.
/// Test naming convention: MethodName_StateUnderTest_ExpectedBehavior.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly UserService sut; // System Under Test

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// Setup is done in constructor as per best practices.
    /// </summary>
    public UserServiceTests()
    {
        this.mockUserRepository = new Mock<IUserRepository>();
        this.sut = new UserService(this.mockUserRepository.Object);
    }

    #region RegisterAsync Tests

    /// <summary>
    /// Tests successful user registration with valid data.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithValidData_CreatesUserSuccessfully()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string password = "SecurePass123!";

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);

        this.mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.RegisterAsync(username, email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal(email, result.Email);
        Assert.NotEqual(password, result.Password); // Password should be hashed
        Assert.Equal(1, result.RoleId); // Default role
        Assert.Equal(UserStatus.Active, result.Status);

        // Verify repository methods were called exactly once
        this.mockUserRepository.Verify(
            x => x.EmailExistsAsync(email),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.Username == username &&
                u.Email == email &&
                u.RoleId == 1 &&
                u.Status == UserStatus.Active)),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    /// <summary>
    /// Tests registration failure when email already exists.
    /// Negative test case - should throw exception.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        const string username = "testuser";
        const string email = "existing@example.com";
        const string password = "SecurePass123!";

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.RegisterAsync(username, email, password));

        Assert.Equal("Email already in use", exception.Message);

        // Verify AddAsync was never called when email exists
        this.mockUserRepository.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);
    }

    /// <summary>
    /// Tests registration with various email formats using Theory.
    /// Data-driven test to verify multiple scenarios.
    /// </summary>
    [Theory]
    [InlineData("user1@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("user+tag@subdomain.example.com")]
    public async Task RegisterAsync_WithVariousEmailFormats_CreatesUser(string email)
    {
        // Arrange
        const string username = "testuser";
        const string password = "SecurePass123!";

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);

        this.mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.RegisterAsync(username, email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    /// <summary>
    /// Tests that password is properly hashed during registration.
    /// Security-focused test.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_WithPlainTextPassword_StoresHashedPassword()
    {
        // Arrange
        const string username = "testuser";
        const string email = "test@example.com";
        const string plainPassword = "MySecurePassword123!";

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(email))
            .ReturnsAsync(false);

        User? capturedUser = null;
        this.mockUserRepository
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .Returns(Task.CompletedTask);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await this.sut.RegisterAsync(username, email, plainPassword);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.NotEqual(plainPassword, capturedUser.Password);
        Assert.NotEmpty(capturedUser.Password);
        // Password hash should be significantly longer than original
        Assert.True(capturedUser.Password.Length > plainPassword.Length);
    }

    #endregion

    #region LoginAsync Tests

    /// <summary>
    /// Tests successful login with valid credentials.
    /// Positive authentication test.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        const string email = "test@example.com";
        const string password = "SecurePass123!";

        // Create a user with hashed password
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(password);
        var expectedUser = new User
        {
            Id = 1,
            Username = "testuser",
            Email = email,
            Password = hashedPassword,
            RoleId = 1,
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await this.sut.LoginAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.Username, result.Username);
        Assert.Equal(expectedUser.Email, result.Email);

        this.mockUserRepository.Verify(
            x => x.GetByEmailAsync(email),
            Times.Once);
    }

    /// <summary>
    /// Tests login failure with incorrect password.
    /// Negative authentication test - wrong password.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsNull()
    {
        // Arrange
        const string email = "test@example.com";
        const string correctPassword = "CorrectPass123!";
        const string wrongPassword = "WrongPassword";

        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(correctPassword);
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = email,
            Password = hashedPassword,
            RoleId = 1,
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await this.sut.LoginAsync(email, wrongPassword);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests login failure with non-existent email.
    /// Negative authentication test - user not found.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsNull()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        const string password = "AnyPassword123!";

        this.mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.LoginAsync(email, password);

        // Assert
        Assert.Null(result);

        this.mockUserRepository.Verify(
            x => x.GetByEmailAsync(email),
            Times.Once);
    }

    /// <summary>
    /// Tests login with various invalid password formats.
    /// Boundary test for password validation.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("short")]
    public async Task LoginAsync_WithInvalidPasswordFormats_ReturnsNull(string invalidPassword)
    {
        // Arrange
        const string email = "test@example.com";
        const string correctPassword = "CorrectPass123!";

        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(correctPassword);
        var user = new User
        {
            Id = 1,
            Email = email,
            Password = hashedPassword,
            RoleId = 1,
        };

        this.mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        var result = await this.sut.LoginAsync(email, invalidPassword);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetByIdAsync Tests

    /// <summary>
    /// Tests retrieval of existing user by ID.
    /// Positive test for read operation.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithExistingUserId_ReturnsUser()
    {
        // Arrange
        const int userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            RoleId = 1,
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await this.sut.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(expectedUser.Username, result.Username);
        Assert.Equal(expectedUser.Email, result.Email);

        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);
    }

    /// <summary>
    /// Tests retrieval with non-existent user ID.
    /// Negative test - user not found.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithNonExistentUserId_ReturnsNull()
    {
        // Arrange
        const int nonExistentUserId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.GetByIdAsync(nonExistentUserId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests retrieval with various user IDs.
    /// Boundary test with edge cases.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public async Task GetByIdAsync_WithVariousUserIds_CallsRepository(int userId)
    {
        // Arrange
        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        await this.sut.GetByIdAsync(userId);

        // Assert
        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    /// <summary>
    /// Tests successful user update.
    /// Positive test for update operation.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesAndReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "updateduser",
            Email = "updated@example.com",
            Password = "hashedpassword",
            RoleId = 1,
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.Update(user))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.UpdateAsync(user);

        // Assert
        Assert.True(result);

        this.mockUserRepository.Verify(
            x => x.Update(user),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    /// <summary>
    /// Tests update with modified user properties.
    /// Ensures all changes are persisted.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WithModifiedProperties_PersistsChanges()
    {
        // Arrange
        var originalUser = new User
        {
            Id = 1,
            Username = "oldname",
            Email = "old@example.com",
            Status = UserStatus.Active,
        };

        var modifiedUser = new User
        {
            Id = 1,
            Username = "newname",
            Email = "new@example.com",
            Status = UserStatus.Blocked,
        };

        User? capturedUser = null;
        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await this.sut.UpdateAsync(modifiedUser);

        // Assert
        Assert.NotNull(capturedUser);
        Assert.Equal("newname", capturedUser.Username);
        Assert.Equal("new@example.com", capturedUser.Email);
        Assert.Equal(UserStatus.Blocked, capturedUser.Status);
    }

    #endregion

    #region DeleteAsync Tests

    /// <summary>
    /// Tests successful user deletion.
    /// Positive test for delete operation.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithExistingUserId_DeletesAndReturnsTrue()
    {
        // Arrange
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        this.mockUserRepository
            .Setup(x => x.Delete(user))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.DeleteAsync(userId);

        // Assert
        Assert.True(result);

        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.Delete(user),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    /// <summary>
    /// Tests deletion with non-existent user ID.
    /// Negative test - user not found.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithNonExistentUserId_ReturnsFalse()
    {
        // Arrange
        const int nonExistentUserId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.DeleteAsync(nonExistentUserId);

        // Assert
        Assert.False(result);

        // Verify Delete was never called
        this.mockUserRepository.Verify(
            x => x.Delete(It.IsAny<User>()),
            Times.Never);

        this.mockUserRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    #endregion

    #region ChangePasswordAsync Tests

    /// <summary>
    /// Tests successful password change with correct old password.
    /// Positive test for password update.
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithCorrectOldPassword_ChangesPasswordSuccessfully()
    {
        // Arrange
        const int userId = 1;
        const string oldPassword = "OldPass123!";
        const string newPassword = "NewSecurePass456!";

        var hashedOldPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(oldPassword);
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Password = hashedOldPassword,
            RoleId = 1,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        User? capturedUser = null;
        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.True(result);
        Assert.NotNull(capturedUser);
        Assert.NotEqual(hashedOldPassword, capturedUser.Password);
        Assert.NotEqual(newPassword, capturedUser.Password); // Should be hashed

        this.mockUserRepository.Verify(
            x => x.Update(It.Is<User>(u => u.Id == userId)),
            Times.Once);

        this.mockUserRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);
    }

    /// <summary>
    /// Tests password change failure with incorrect old password.
    /// Negative test - authentication failure.
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithIncorrectOldPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const int userId = 1;
        const string correctOldPassword = "CorrectPass123!";
        const string incorrectOldPassword = "WrongPass123!";
        const string newPassword = "NewPass456!";

        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(correctOldPassword);
        var user = new User
        {
            Id = userId,
            Password = hashedPassword,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.sut.ChangePasswordAsync(userId, incorrectOldPassword, newPassword));

        Assert.Equal("Old password is incorrect", exception.Message);

        // Verify Update was never called
        this.mockUserRepository.Verify(
            x => x.Update(It.IsAny<User>()),
            Times.Never);
    }

    /// <summary>
    /// Tests password change with non-existent user.
    /// Negative test - user not found.
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int nonExistentUserId = 999;
        const string oldPassword = "OldPass123!";
        const string newPassword = "NewPass456!";

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.ChangePasswordAsync(nonExistentUserId, oldPassword, newPassword));

        Assert.Equal("User not found", exception.Message);
    }

    /// <summary>
    /// Tests that new password is different from old password hash.
    /// Security test - ensures password is re-hashed.
    /// </summary>
    [Fact]
    public async Task ChangePasswordAsync_WithValidData_CreatesNewPasswordHash()
    {
        // Arrange
        const int userId = 1;
        const string oldPassword = "OldPass123!";
        const string newPassword = "NewPass456!";

        var oldHashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(oldPassword);
        var user = new User
        {
            Id = userId,
            Password = oldHashedPassword,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        string? newHashedPassword = null;
        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Callback<User>(u => newHashedPassword = u.Password);

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await this.sut.ChangePasswordAsync(userId, oldPassword, newPassword);

        // Assert
        Assert.NotNull(newHashedPassword);
        Assert.NotEqual(oldHashedPassword, newHashedPassword);
        Assert.NotEqual(newPassword, newHashedPassword);
    }

    #endregion
}
