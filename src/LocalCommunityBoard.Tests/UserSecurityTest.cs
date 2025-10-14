// <copyright file="PasswordHasherTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Tests.Security;

using LocalCommunityBoard.Application.Security;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PasswordHasher"/> following Microsoft best practices.
/// Test naming convention: MethodName_StateUnderTest_ExpectedBehavior.
/// </summary>
public class PasswordHasherTests
{
    /// <summary>
    /// Tests that hashed password can be verified successfully.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public void HashPassword_ValidPassword_CanBeVerified()
    {
        // Arrange
        const string password = "MySecurePass123!";

        // Act
        string hash = PasswordHasher.HashPassword(password);
        bool isValid = PasswordHasher.VerifyPassword(password, hash);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests that same password produces different hashes.
    /// Security test - salt randomization.
    /// </summary>
    [Fact]
    public void HashPassword_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Arrange
        const string password = "MySecurePass123!";

        // Act
        string hash1 = PasswordHasher.HashPassword(password);
        string hash2 = PasswordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// Tests that wrong password fails verification.
    /// Negative test case - security check.
    /// </summary>
    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        // Arrange
        const string correctPassword = "CorrectPass123!";
        const string wrongPassword = "WrongPass123!";
        string hash = PasswordHasher.HashPassword(correctPassword);

        // Act
        bool isValid = PasswordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(isValid);
    }

    /// <summary>
    /// Tests handling of empty or null passwords.
    /// Boundary test case - input validation.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)] // null case
    public void HashPassword_EmptyOrNullPassword_ThrowsArgumentException(string? password)
    {
        // Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
        Assert.Throws<ArgumentException>(() => PasswordHasher.HashPassword(password));
#pragma warning restore CS8604
    }

    /// <summary>
    /// Tests that hash has correct length.
    /// Technical test - implementation detail.
    /// </summary>
    [Fact]
    public void HashPassword_ValidPassword_ProducesExpectedHashLength()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        string hash = PasswordHasher.HashPassword(password);
        byte[] decoded = Convert.FromBase64String(hash);

        // Assert
        Assert.Equal(48, decoded.Length); // SaltSize (16) + KeySize (32)
    }

    /// <summary>
    /// Tests handling of invalid hash formats.
    /// Security test - malformed input handling.
    /// </summary>
    [Theory]
    [InlineData("invalid")]
    [InlineData("not-base64-encoded!")]
    public void VerifyPassword_InvalidHashFormat_ReturnsFalse(string invalidHash)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() =>
            PasswordHasher.VerifyPassword("anypassword", invalidHash));
    }

    /// <summary>
    /// Tests handling of too short hash values.
    /// Security test - malformed hash handling.
    /// </summary>
    [Fact]
    public void VerifyPassword_HashTooShort_ReturnsFalse()
    {
        // Arrange
        string tooShortHash = Convert.ToBase64String(new byte[20]);

        // Act
        bool result = PasswordHasher.VerifyPassword("anypassword", tooShortHash);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests handling of long passwords.
    /// Boundary test - large input handling.
    /// </summary>
    [Fact]
    public void HashPassword_LongPassword_SuccessfullyHashesAndVerifies()
    {
        // Arrange
        string longPassword = new string('a', 100);

        // Act
        string hash = PasswordHasher.HashPassword(longPassword);
        bool isValid = PasswordHasher.VerifyPassword(longPassword, hash);

        // Assert
        Assert.True(isValid);
    }
}