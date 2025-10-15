// <copyright file="UserSessionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Tests.Sessions;

using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using Xunit;

/// <summary>
/// Unit tests for <see cref="UserSession"/> following Microsoft best practices.
/// Test naming convention: MethodName_StateUnderTest_ExpectedBehavior.
/// </summary>
public class UserSessionTests
{
    private readonly UserSession sut; // System Under Test

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSessionTests"/> class.
    /// Setup is done in constructor as per best practices.
    /// </summary>
    public UserSessionTests()
    {
        this.sut = new UserSession();
    }

    #region Constructor Tests

    /// <summary>
    /// Tests initial state after construction.
    /// Ensures proper initialization.
    /// </summary>
    [Fact]
    public void Constructor_WhenCalled_InitializesWithNoUser()
    {
        // Assert
        Assert.Null(this.sut.CurrentUser);
        Assert.False(this.sut.IsLoggedIn);
        Assert.Equal("Guest", this.sut.DisplayName);
    }

    #endregion

    #region Login Tests

    /// <summary>
    /// Tests successful login with valid user.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public void Login_WithValidUser_SetsCurrentUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Status = UserStatus.Active
        };

        // Act
        this.sut.Login(user);

        // Assert
        Assert.NotNull(this.sut.CurrentUser);
        Assert.True(this.sut.IsLoggedIn);
        Assert.Equal(user.Username, this.sut.DisplayName);
        Assert.Equal(user, this.sut.CurrentUser);
    }

    /// <summary>
    /// Tests that login updates session state correctly.
    /// State verification test.
    /// </summary>
    [Fact]
    public void Login_WhenUserLogsIn_UpdatesSessionState()
    {
        // Arrange
        var user = new User { Username = "testuser" };

        // Act
        this.sut.Login(user);

        // Assert
        Assert.True(this.sut.IsLoggedIn);
        Assert.Equal("testuser", this.sut.DisplayName);
    }

    #endregion

    #region Logout Tests

    /// <summary>
    /// Tests successful logout.
    /// Positive test case.
    /// </summary>
    [Fact]
    public void Logout_WhenCalled_ClearsCurrentUser()
    {
        // Arrange
        var user = new User { Username = "testuser" };
        this.sut.Login(user);

        // Act
        this.sut.Logout();

        // Assert
        Assert.Null(this.sut.CurrentUser);
        Assert.False(this.sut.IsLoggedIn);
        Assert.Equal("Guest", this.sut.DisplayName);
    }

    /// <summary>
    /// Tests logout when no user is logged in.
    /// Edge case test.
    /// </summary>
    [Fact]
    public void Logout_WhenNoUserLoggedIn_MaintainsInitialState()
    {
        // Act
        this.sut.Logout();

        // Assert
        Assert.Null(this.sut.CurrentUser);
        Assert.False(this.sut.IsLoggedIn);
        Assert.Equal("Guest", this.sut.DisplayName);
    }

    #endregion

    #region Property Tests

    /// <summary>
    /// Tests IsLoggedIn property behavior.
    /// Property verification test.
    /// </summary>
    [Fact]
    public void IsLoggedIn_ReflectsCurrentLoginState()
    {
        // Arrange
        var user = new User { Username = "testuser" };

        // Assert - Initially not logged in
        Assert.False(this.sut.IsLoggedIn);

        // Act & Assert - After login
        this.sut.Login(user);
        Assert.True(this.sut.IsLoggedIn);

        // Act & Assert - After logout
        this.sut.Logout();
        Assert.False(this.sut.IsLoggedIn);
    }

    /// <summary>
    /// Tests DisplayName property behavior.
    /// Property verification test.
    /// </summary>
    [Theory]
    [InlineData("testuser")]
    [InlineData("John Doe")]
    [InlineData("user123")]
    public void DisplayName_ReturnsCorrectValue(string username)
    {
        // Arrange
        var user = new User { Username = username };

        // Assert - Initially guest
        Assert.Equal("Guest", this.sut.DisplayName);

        // Act & Assert - After login
        this.sut.Login(user);
        Assert.Equal(username, this.sut.DisplayName);

        // Act & Assert - After logout
        this.sut.Logout();
        Assert.Equal("Guest", this.sut.DisplayName);
    }

    #endregion
}
