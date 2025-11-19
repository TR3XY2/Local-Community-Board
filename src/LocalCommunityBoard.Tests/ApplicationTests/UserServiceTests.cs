// <copyright file="UserServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Tests.Services;

using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using LocalCommunityBoard.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="UserService"/> following Microsoft best practices.
/// Test naming convention: MethodName_StateUnderTest_ExpectedBehavior.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> mockUserRepository;
    private readonly Mock<ILogger<UserService>> mockLogger;
    private readonly UserService sut; // System Under Test

    /// <summary>
    /// Initializes a new instance of the <see cref="UserServiceTests"/> class.
    /// Add a mock ILogger<UserService> to the UserServiceTests class to satisfy the constructor dependency.
    /// </summary>
    public UserServiceTests()
    {
        this.mockUserRepository = new Mock<IUserRepository>();
        this.mockLogger = new Mock<ILogger<UserService>>();
        this.sut = new UserService(this.mockUserRepository.Object, this.mockLogger.Object);
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

    #region LogoutAsync Tests
    /// <summary>
    /// Tests successful logout with existing user.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        const int userId = 1;
        var expectedUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Status = UserStatus.Active
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await this.sut.LogoutAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal(expectedUser.Username, result.Username);

        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);
    }

    /// <summary>
    /// Tests logout with non-existent user.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task LogoutAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        const int nonExistentUserId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.LogoutAsync(nonExistentUserId);

        // Assert
        Assert.Null(result);

        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(nonExistentUserId),
            Times.Once);
    }

    /// <summary>
    /// Tests logout with various user IDs.
    /// Boundary test cases.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)] // Invalid ID to test edge case
    public async Task LogoutAsync_WithVariousUserIds_CallsRepository(int userId)
    {
        // Arrange
        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        await this.sut.LogoutAsync(userId);

        // Assert
        this.mockUserRepository.Verify(
            x => x.GetByIdAsync(userId),
            Times.Once);
    }
    #endregion

    #region UpdatePersonalInfoAsync Tests
    /// <summary>
    /// Tests successful personal info update.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task UpdatePersonalInfoAsync_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        const int userId = 1;
        const string newUsername = "newusername";
        const string newEmail = "newemail@example.com";

        var existingUser = new User
        {
            Id = userId,
            Username = "oldusername",
            Email = "old@example.com"
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(newEmail))
            .ReturnsAsync(false);

        // Act
        var result = await this.sut.UpdatePersonalInfoAsync(userId, newUsername, newEmail);

        // Assert
        Assert.True(result);
        Assert.Equal(newUsername, existingUser.Username);
        Assert.Equal(newEmail, existingUser.Email);

        this.mockUserRepository.Verify(x => x.Update(existingUser), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests personal info update with non-existent user.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task UpdatePersonalInfoAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int nonExistentUserId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(nonExistentUserId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.UpdatePersonalInfoAsync(nonExistentUserId, "newname", "new@example.com"));

        Assert.Equal("User not found", exception.Message);
    }

    /// <summary>
    /// Tests personal info update with an email that already exists.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task UpdatePersonalInfoAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 1;
        const string existingEmail = "existing@example.com";

        var user = new User
        {
            Id = userId,
            Username = "username",
            Email = "old@example.com"
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(existingEmail))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.UpdatePersonalInfoAsync(userId, "newname", existingEmail));

        Assert.Equal("Email is already in use by another account.", exception.Message);
    }

    /// <summary>
    /// Tests personal info update with null values.
    /// Ensures existing data remains unchanged.
    /// </summary>
    [Fact]
    public async Task UpdatePersonalInfoAsync_WithNullValues_KeepsExistingData()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            Username = "existingname",
            Email = "existing@example.com"
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act
        var result = await this.sut.UpdatePersonalInfoAsync(userId, null, null);

        // Assert
        Assert.True(result);
        Assert.Equal("existingname", existingUser.Username);
        Assert.Equal("existing@example.com", existingUser.Email);
    }

    #endregion

    #region GetAllUsersAsync Tests

    /// <summary>
    /// Tests that GetAllUsersAsync returns all users correctly from the repository.
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_WhenUsersExist_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1", Email = "user1@example.com" },
            new() { Id = 2, Username = "user2", Email = "user2@example.com" }
        };

        this.mockUserRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await this.sut.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        var userList = result.ToList();
        Assert.Equal(2, userList.Count);
        Assert.Equal("user1", userList[0].Username);
        Assert.Equal("user2@example.com", userList[1].Email);

        this.mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllUsersAsync returns an empty list when no users exist.
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_WhenNoUsersExist_ReturnsEmptyList()
    {
        // Arrange
        this.mockUserRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<User>());

        // Act
        var result = await this.sut.GetAllUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

        this.mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllUsersAsync logs an error and throws if the repository throws an exception.
    /// </summary>
    [Fact]
    public async Task GetAllUsersAsync_WhenRepositoryThrows_LogsErrorAndThrows()
    {
        // Arrange
        this.mockUserRepository
            .Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception("Database failure"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => this.sut.GetAllUsersAsync());
        Assert.Equal("Database failure", ex.Message);

        this.mockUserRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    #endregion

    #region AdminUpdateUserAsync Tests

    /// <summary>
    /// Tests successful admin update of username, email, and password.
    /// Positive test for admin-level modification.
    /// </summary>
    [Fact]
    public async Task AdminUpdateUserAsync_WithValidUpdates_UpdatesUserSuccessfully()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            Username = "olduser",
            Email = "old@example.com",
            Password = "oldhashed",
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.AdminUpdateUserAsync(
            userId,
            "newuser",
            "new@example.com",
            "NewPassword123");

        // Assert
        Assert.True(result);
        Assert.Equal("newuser", existingUser.Username);
        Assert.Equal("new@example.com", existingUser.Email);
        Assert.NotEqual("NewPassword123", existingUser.Password); // Password should be hashed

        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that AdminUpdateUserAsync throws when user not found.
    /// </summary>
    [Fact]
    public async Task AdminUpdateUserAsync_WithNonExistentUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.AdminUpdateUserAsync(userId, "name", "email@example.com", "password"));
    }

    /// <summary>
    /// Tests that AdminUpdateUserAsync throws when new email is already in use.
    /// </summary>
    [Fact]
    public async Task AdminUpdateUserAsync_WithExistingEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "old@example.com",
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        this.mockUserRepository
            .Setup(x => x.EmailExistsAsync("used@example.com"))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.AdminUpdateUserAsync(userId, null, "used@example.com", null));

        Assert.Equal("Email is already in use by another account.", exception.Message);
    }

    /// <summary>
    /// Tests that AdminUpdateUserAsync throws if new password is too short.
    /// </summary>
    [Fact]
    public async Task AdminUpdateUserAsync_WithShortPassword_ThrowsArgumentException()
    {
        // Arrange
        const int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@example.com",
            Password = "old",
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => this.sut.AdminUpdateUserAsync(userId, null, null, "123"));

        Assert.Equal("Password must be at least 6 characters long.", ex.Message);
    }

    #endregion

    #region BlockUserAsync Tests

    /// <summary>
    /// Tests successful blocking of a regular user.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task BlockUserAsync_WithRegularUser_BlocksSuccessfully()
    {
        // Arrange
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "regularuser",
            Email = "user@example.com",
            RoleId = 1, // Regular user
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.BlockUserAsync(userId);

        // Assert
        Assert.True(result);
        Assert.Equal(UserStatus.Blocked, user.Status);

        this.mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        this.mockUserRepository.Verify(x => x.Update(user), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that blocking an admin account throws InvalidOperationException.
    /// Negative test case - security protection.
    /// </summary>
    [Fact]
    public async Task BlockUserAsync_WithAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 2;
        var adminUser = new User
        {
            Id = userId,
            Username = "adminuser",
            Email = "admin@example.com",
            RoleId = 2, // Admin
            Status = UserStatus.Active,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.BlockUserAsync(userId));

        Assert.Equal("Cannot block administrator accounts.", exception.Message);

        // Verify that Update and SaveChanges were never called
        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests blocking non-existent user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task BlockUserAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.BlockUserAsync(userId);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that blocking logs appropriate warning when attempting to block admin.
    /// </summary>
    [Fact]
    public async Task BlockUserAsync_WhenBlockingAdmin_LogsWarning()
    {
        // Arrange
        const int userId = 2;
        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            RoleId = 2,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => this.sut.BlockUserAsync(userId));

        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Attempted to block admin user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UnblockUserAsync Tests

    /// <summary>
    /// Tests successful user unblocking — repository returns true.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task UnblockUserAsync_WithValidUserId_SetsStatusToActiveAndReturnsTrue()
    {
        // Arrange
        const int userId = 1;

        this.mockUserRepository
            .Setup(x => x.SetStatusAsync(userId, UserStatus.Active))
            .ReturnsAsync(true);

        // Act
        var result = await this.sut.UnblockUserAsync(userId);

        // Assert
        Assert.True(result);

        this.mockUserRepository.Verify(
            x => x.SetStatusAsync(userId, UserStatus.Active),
            Times.Once);

        // Verify that logger logs success
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("unblocked by admin")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests unsuccessful user unblocking — repository returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task UnblockUserAsync_WhenRepositoryReturnsFalse_LogsWarningAndReturnsFalse()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.SetStatusAsync(userId, UserStatus.Active))
            .ReturnsAsync(false);

        // Act
        var result = await this.sut.UnblockUserAsync(userId);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(
            x => x.SetStatusAsync(userId, UserStatus.Active),
            Times.Once);

        // Verify that logger logs warning
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to unblock user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Tests boundary values for user IDs during unblock operation.
    /// Ensures repository is always called.
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(int.MaxValue)]
    public async Task UnblockUserAsync_WithVariousUserIds_CallsRepository(int userId)
    {
        // Arrange
        this.mockUserRepository
            .Setup(x => x.SetStatusAsync(userId, UserStatus.Active))
            .ReturnsAsync(true);

        // Act
        await this.sut.UnblockUserAsync(userId);

        // Assert
        this.mockUserRepository.Verify(
            x => x.SetStatusAsync(userId, UserStatus.Active),
            Times.Once);
    }

    #endregion

        #region DemoteFromAdminAsync Tests

    /// <summary>
    /// Tests successful demotion of admin to regular user.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task DemoteFromAdminAsync_WithAdminUser_DemotesSuccessfully()
    {
        // Arrange
        const int userId = 2;
        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            Email = "admin@example.com",
            RoleId = 2, // Admin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.DemoteFromAdminAsync(userId);

        // Assert
        Assert.True(result);
        Assert.Equal(1, adminUser.RoleId); // Should be demoted to User

        this.mockUserRepository.Verify(x => x.Update(adminUser), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that demoting non-admin user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task DemoteFromAdminAsync_WithRegularUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 1;
        var regularUser = new User
        {
            Id = userId,
            RoleId = 1, // Regular user
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(regularUser);

        // Act
        var result = await this.sut.DemoteFromAdminAsync(userId);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    /// <summary>
    /// Tests demoting non-existent user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task DemoteFromAdminAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.DemoteFromAdminAsync(userId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that SuperAdmin (RoleId = 3) cannot be demoted.
    /// Security test case.
    /// </summary>
    [Fact]
    public async Task DemoteFromAdminAsync_WithSuperAdmin_ReturnsFalse()
    {
        // Arrange
        const int userId = 3;
        var superAdmin = new User
        {
            Id = userId,
            RoleId = 3, // SuperAdmin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(superAdmin);

        // Act
        var result = await this.sut.DemoteFromAdminAsync(userId);

        // Assert
        Assert.False(result);
        Assert.Equal(3, superAdmin.RoleId); // Role should remain unchanged

        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region PromoteToAdminAsync Tests

    /// <summary>
    /// Tests successful promotion of regular user to admin.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task PromoteToAdminAsync_WithRegularUser_PromotesSuccessfully()
    {
        // Arrange
        const int userId = 1;
        var regularUser = new User
        {
            Id = userId,
            Username = "user",
            RoleId = 1, // Regular user
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(regularUser);

        this.mockUserRepository
            .Setup(x => x.Update(It.IsAny<User>()))
            .Verifiable();

        this.mockUserRepository
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.sut.PromoteToAdminAsync(userId);

        // Assert
        Assert.True(result);
        Assert.Equal(2, regularUser.RoleId); // Should be promoted to Admin

        this.mockUserRepository.Verify(x => x.Update(regularUser), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that promoting already admin user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task PromoteToAdminAsync_WithExistingAdmin_ReturnsFalse()
    {
        // Arrange
        const int userId = 2;
        var adminUser = new User
        {
            Id = userId,
            RoleId = 2, // Already admin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        // Act
        var result = await this.sut.PromoteToAdminAsync(userId);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests promoting non-existent user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task PromoteToAdminAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.PromoteToAdminAsync(userId);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that SuperAdmin cannot be "promoted" again.
    /// Edge case test.
    /// </summary>
    [Fact]
    public async Task PromoteToAdminAsync_WithSuperAdmin_ReturnsFalse()
    {
        // Arrange
        const int userId = 3;
        var superAdmin = new User
        {
            Id = userId,
            RoleId = 3, // SuperAdmin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(superAdmin);

        // Act
        var result = await this.sut.PromoteToAdminAsync(userId);

        // Assert
        Assert.False(result);
        Assert.Equal(3, superAdmin.RoleId); // Role unchanged
    }

    #endregion

    #region DeleteUserByAdminAsync Tests

    /// <summary>
    /// Tests successful deletion of regular user by admin.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task DeleteUserByAdminAsync_WithRegularUser_DeletesSuccessfully()
    {
        // Arrange
        const int userId = 1;
        var user = new User
        {
            Id = userId,
            Username = "regularuser",
            Email = "user@example.com",
            RoleId = 1, // Regular user
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
        var result = await this.sut.DeleteUserByAdminAsync(userId);

        // Assert
        Assert.True(result);

        this.mockUserRepository.Verify(x => x.Delete(user), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that admin user cannot be deleted.
    /// Security test case.
    /// </summary>
    [Fact]
    public async Task DeleteUserByAdminAsync_WithAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 2;
        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            RoleId = 2, // Admin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.DeleteUserByAdminAsync(userId));

        Assert.Equal("Cannot delete administrator accounts.", exception.Message);

        this.mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that SuperAdmin cannot be deleted.
    /// Security test case.
    /// </summary>
    [Fact]
    public async Task DeleteUserByAdminAsync_WithSuperAdmin_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 3;
        var superAdmin = new User
        {
            Id = userId,
            Username = "superadmin",
            RoleId = 3, // SuperAdmin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(superAdmin);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.DeleteUserByAdminAsync(userId));

        Assert.Equal("Cannot delete administrator accounts.", exception.Message);
    }

    /// <summary>
    /// Tests deletion of non-existent user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task DeleteUserByAdminAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 999;

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.DeleteUserByAdminAsync(userId);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region DeleteOwnAccountAsync Tests

    /// <summary>
    /// Tests successful self-deletion with correct password.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task DeleteOwnAccountAsync_WithCorrectPassword_DeletesSuccessfully()
    {
        // Arrange
        const int userId = 1;
        const string password = "SecurePass123!";
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(password);

        var user = new User
        {
            Id = userId,
            Username = "user",
            Email = "user@example.com",
            Password = hashedPassword,
            RoleId = 1, // Regular user
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
        var result = await this.sut.DeleteOwnAccountAsync(userId, password);

        // Assert
        Assert.True(result);

        this.mockUserRepository.Verify(x => x.Delete(user), Times.Once);
        this.mockUserRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests self-deletion with incorrect password throws UnauthorizedAccessException.
    /// Security test case.
    /// </summary>
    [Fact]
    public async Task DeleteOwnAccountAsync_WithIncorrectPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const int userId = 1;
        const string correctPassword = "CorrectPass123!";
        const string wrongPassword = "WrongPass123!";
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(correctPassword);

        var user = new User
        {
            Id = userId,
            Password = hashedPassword,
            RoleId = 1,
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => this.sut.DeleteOwnAccountAsync(userId, wrongPassword));

        Assert.Equal("Password is incorrect.", exception.Message);

        this.mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that admin cannot delete own account.
    /// Security test case.
    /// </summary>
    [Fact]
    public async Task DeleteOwnAccountAsync_WithAdminUser_ThrowsInvalidOperationException()
    {
        // Arrange
        const int userId = 2;
        const string password = "AdminPass123!";
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(password);

        var adminUser = new User
        {
            Id = userId,
            Username = "admin",
            Password = hashedPassword,
            RoleId = 2, // Admin
        };

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(adminUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.sut.DeleteOwnAccountAsync(userId, password));

        Assert.Equal("Administrators cannot delete their own accounts.", exception.Message);

        this.mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests self-deletion with non-existent user returns false.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task DeleteOwnAccountAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        const int userId = 999;
        const string password = "AnyPass123!";

        this.mockUserRepository
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await this.sut.DeleteOwnAccountAsync(userId, password);

        // Assert
        Assert.False(result);

        this.mockUserRepository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that appropriate log is written on successful self-deletion.
    /// Logging test case.
    /// </summary>
    [Fact]
    public async Task DeleteOwnAccountAsync_WhenSuccessful_LogsInformation()
    {
        // Arrange
        const int userId = 1;
        const string password = "SecurePass123!";
        const string email = "user@example.com";
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(password);

        var user = new User
        {
            Id = userId,
            Email = email,
            Password = hashedPassword,
            RoleId = 1,
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
        await this.sut.DeleteOwnAccountAsync(userId, password);

        // Assert
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("deleted their account")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region LoginAsync Additional Tests

    /// <summary>
    /// Tests that blocked user can still login (returns user object).
    /// Business logic test - blocked users should be handled at controller level.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WithBlockedUser_ReturnsUser()
    {
        // Arrange
        const string email = "blocked@example.com";
        const string password = "Pass123!";
        var hashedPassword = LocalCommunityBoard.Application.Security.PasswordHasher.HashPassword(password);

        var blockedUser = new User
        {
            Id = 1,
            Email = email,
            Password = hashedPassword,
            Status = UserStatus.Blocked,
        };

        this.mockUserRepository
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(blockedUser);

        // Act
        var result = await this.sut.LoginAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserStatus.Blocked, result.Status);

        // Verify that logger logs the status
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Status: Blocked")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}
