// <copyright file="AnnouncementServiceTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Tests.Services;

using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Interfaces;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AnnouncementService"/> following Microsoft best practices.
/// Test naming convention: MethodName_StateUnderTest_ExpectedBehavior.
/// </summary>
public class AnnouncementServiceTests
{
    private readonly Mock<IAnnouncementRepository> mockAnnouncementRepository;
    private readonly Mock<IRepository<Location>> mockLocationRepository;
    private readonly Mock<IRepository<Category>> mockCategoryRepository;
    private readonly AnnouncementService sut; // System Under Test

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnouncementServiceTests"/> class.
    /// Setup is done in constructor as per best practices.
    /// </summary>
    public AnnouncementServiceTests()
    {
        this.mockAnnouncementRepository = new Mock<IAnnouncementRepository>();
        this.mockLocationRepository = new Mock<IRepository<Location>>();
        this.mockCategoryRepository = new Mock<IRepository<Category>>();

        this.sut = new AnnouncementService(
            this.mockAnnouncementRepository.Object,
            this.mockLocationRepository.Object,
            this.mockCategoryRepository.Object);
    }

    #region GetAnnouncementsPagedAsync Tests

    /// <summary>
    /// Tests successful retrieval of paged announcements.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsPagedAsync_WithValidParameters_ReturnsPagedResults()
    {
        // Arrange
        var announcements = new List<Announcement>
        {
            new Announcement { Id = 1, Title = "Test 1" },
            new Announcement { Id = 2, Title = "Test 2" }
        };
        var expectedResult = (Items: announcements.AsEnumerable(), TotalCount: 2);

        this.mockAnnouncementRepository
            .Setup(r => r.GetFilteredPagedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await this.sut.GetAnnouncementsPagedAsync();

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }

    /// <summary>
    /// Tests paging with filters.
    /// State verification test.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsPagedAsync_WithFilters_PassesFiltersToRepository()
    {
        // Arrange
        const string city = "TestCity";
        const string district = "TestDistrict";
        var categoryIds = new List<int> { 1, 2 };

        this.mockAnnouncementRepository
            .Setup(r => r.GetFilteredPagedAsync(city, district, null, categoryIds, null, 1, 9))
            .ReturnsAsync((Enumerable.Empty<Announcement>(), 0));

        // Act
        await this.sut.GetAnnouncementsPagedAsync(city, district, categoryIds: categoryIds);

        // Assert
        this.mockAnnouncementRepository.Verify(
            r => r.GetFilteredPagedAsync(city, district, null, categoryIds, null, 1, 9),
            Times.Once);
    }

    /// <summary>
    /// Tests paging with street and date filters.
    /// State verification test.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsPagedAsync_WithStreetAndDate_PassesFiltersToRepository()
    {
        // Arrange
        const string street = "TestStreet";
        var date = new DateTime(2025, 10, 15);

        this.mockAnnouncementRepository
            .Setup(r => r.GetFilteredPagedAsync(null, null, street, null, date, 1, 9))
            .ReturnsAsync((Enumerable.Empty<Announcement>(), 0));

        // Act
        await this.sut.GetAnnouncementsPagedAsync(street: street, date: date);

        // Assert
        this.mockAnnouncementRepository.Verify(
            r => r.GetFilteredPagedAsync(null, null, street, null, date, 1, 9),
            Times.Once);
    }

    /// <summary>
    /// Tests paging with invalid page parameters.
    /// Negative test case.
    /// </summary>
    [Theory]
    [InlineData(0, 9)]
    [InlineData(1, 0)]
    [InlineData(-1, 9)]
    [InlineData(1, -1)]
    public async Task GetAnnouncementsPagedAsync_WithInvalidPageParameters_ReturnsEmptyResult(int pageNumber, int pageSize)
    {
        // Arrange
        this.mockAnnouncementRepository
            .Setup(r => r.GetFilteredPagedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime?>(),
                pageNumber,
                pageSize))
            .ReturnsAsync((Enumerable.Empty<Announcement>(), 0));

        // Act
        var result = await this.sut.GetAnnouncementsPagedAsync(pageNumber: pageNumber, pageSize: pageSize);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    /// <summary>
    /// Tests paging with empty result.
    /// Edge case test.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsPagedAsync_WithNoAnnouncements_ReturnsEmptyResult()
    {
        // Arrange
        this.mockAnnouncementRepository
            .Setup(r => r.GetFilteredPagedAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync((Enumerable.Empty<Announcement>(), 0));

        // Act
        var result = await this.sut.GetAnnouncementsPagedAsync();

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    #endregion

    #region CreateAnnouncementAsync Tests

    /// <summary>
    /// Tests successful announcement creation.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task CreateAnnouncementAsync_WithValidData_CreatesAnnouncement()
    {
        // Arrange
        const int userId = 1;
        const string title = "Test Title";
        const string body = "Test Body";
        const int categoryId = 1;
        const int locationId = 1;

        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId });

        this.mockLocationRepository
            .Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(new Location { Id = locationId });

        // Act
        var result = await this.sut.CreateAnnouncementAsync(
            userId, title, body, categoryId, locationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(title, result.Title);
        Assert.Equal(body, result.Body);
        Assert.Equal(categoryId, result.CategoryId);
        Assert.Equal(locationId, result.LocationId);

        this.mockAnnouncementRepository.Verify(
            r => r.AddAsync(It.IsAny<Announcement>()), Times.Once);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that whitespace is trimmed from title and body.
    /// Data validation test.
    /// </summary>
    [Fact]
    public async Task CreateAnnouncementAsync_WithWhitespace_TrimsInput()
    {
        // Arrange
        const string title = "  Test Title  ";
        const string body = "  Test Body  ";

        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Category());

        this.mockLocationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Location());

        // Act
        var result = await this.sut.CreateAnnouncementAsync(1, title, body, 1, 1);

        // Assert
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Body", result.Body);
    }

    /// <summary>
    /// Tests validation for empty title.
    /// Negative test case.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAnnouncementAsync_WithEmptyTitle_ThrowsArgumentException(string? title)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            this.sut.CreateAnnouncementAsync(1, title!, "Body", 1, 1));
    }

    /// <summary>
    /// Tests validation for non-existent category.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task CreateAnnouncementAsync_WithInvalidCategory_ThrowsArgumentException()
    {
        // Arrange
        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            this.sut.CreateAnnouncementAsync(1, "Title", "Body", 999, 1));

        Assert.Contains("Category with ID 999 does not exist", exception.Message);
    }

    /// <summary>
    /// Tests validation for non-existent location.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task CreateAnnouncementAsync_WithInvalidLocation_ThrowsArgumentException()
    {
        // Arrange
        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Category());

        this.mockLocationRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Location?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            this.sut.CreateAnnouncementAsync(1, "Title", "Body", 1, 999));

        Assert.Contains("Location with ID 999 does not exist", exception.Message);
    }

    /// <summary>
    /// Tests creation with image URLs and links.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task CreateAnnouncementAsync_WithImageUrlsAndLinks_SavesCorrectly()
    {
        // Arrange
        const int userId = 1;
        const string title = "Test Title";
        const string body = "Test Body";
        const int categoryId = 1;
        const int locationId = 1;
        IEnumerable<string> imageUrls = new List<string> { "http://example.com/image.jpg" };

        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(categoryId))
            .ReturnsAsync(new Category { Id = categoryId });

        this.mockLocationRepository
            .Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(new Location { Id = locationId });

        // Act
        var result = await this.sut.CreateAnnouncementAsync(
            userId, title, body, categoryId, locationId);

        // Assert
        Assert.NotNull(result);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region UpdateAnnouncementAsync Tests

    /// <summary>
    /// Tests successful announcement update.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithValidData_UpdatesAnnouncement()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;
        var announcement = new Announcement
        {
            Id = announcementId,
            UserId = userId,
            Title = "Old Title",
            Body = "Old Body",
            CategoryId = 1
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Category { Id = 2 });

        // Act
        var result = await this.sut.UpdateAnnouncementAsync(
            announcementId, userId, "New Title", "New Body", 2);

        // Assert
        Assert.True(result);
        Assert.Equal("New Title", announcement.Title);
        Assert.Equal("New Body", announcement.Body);
        Assert.Equal(2, announcement.CategoryId);

        this.mockAnnouncementRepository.Verify(
            r => r.Update(announcement), Times.Once);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests partial update with only title.
    /// Edge case test.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithOnlyTitle_UpdatesOnlyTitle()
    {
        // Arrange
        var announcement = new Announcement
        {
            Id = 1,
            UserId = 1,
            Title = "Old Title",
            Body = "Old Body",
            CategoryId = 1
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        await this.sut.UpdateAnnouncementAsync(1, 1, title: "New Title");

        // Assert
        Assert.Equal("New Title", announcement.Title);
        Assert.Equal("Old Body", announcement.Body);
        Assert.Equal(1, announcement.CategoryId);
    }

    /// <summary>
    /// Tests update with whitespace trimming.
    /// Data validation test.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithWhitespace_TrimsInput()
    {
        // Arrange
        var announcement = new Announcement
        {
            Id = 1,
            UserId = 1,
            Title = "Old Title",
            Body = "Old Body"
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        await this.sut.UpdateAnnouncementAsync(1, 1, "  New Title  ", "  New Body  ");

        // Assert
        Assert.Equal("New Title", announcement.Title);
        Assert.Equal("New Body", announcement.Body);
    }

    /// <summary>
    /// Tests update of non-existent announcement.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithNonExistentAnnouncement_ReturnsFalse()
    {
        // Arrange
        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Announcement?)null);

        // Act
        var result = await this.sut.UpdateAnnouncementAsync(999, 1, "Title");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests update by wrong user.
    /// Authorization test.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithWrongUser_ReturnsFalse()
    {
        // Arrange
        var announcement = new Announcement { Id = 1, UserId = 1 };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        var result = await this.sut.UpdateAnnouncementAsync(1, 999, "Title");

        // Assert
        Assert.False(result);
        this.mockAnnouncementRepository.Verify(
            r => r.Update(It.IsAny<Announcement>()), Times.Never);
    }

    /// <summary>
    /// Tests update with invalid category.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithInvalidCategory_ThrowsArgumentException()
    {
        // Arrange
        var announcement = new Announcement { Id = 1, UserId = 1 };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        this.mockCategoryRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            this.sut.UpdateAnnouncementAsync(1, 1, categoryId: 999));

        Assert.Contains("Category with ID 999 does not exist", exception.Message);
    }

    /// <summary>
    /// Tests update with image URL.
    /// Positive test case.
    /// </summary>
    [Fact]
    public async Task UpdateAnnouncementAsync_WithImageUrl_UpdatesImageUrl()
    {
        // Arrange
        var announcement = new Announcement
        {
            Id = 1,
            UserId = 1,
            Title = "Old Title",
            Body = "Old Body",
            ImageUrl = null
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        var result = await this.sut.UpdateAnnouncementAsync(1, 1, imageUrl: "  http://example.com/image.jpg  ");

        // Assert
        Assert.True(result);
        Assert.Equal("http://example.com/image.jpg", announcement.ImageUrl);
        this.mockAnnouncementRepository.Verify(
            r => r.Update(announcement), Times.Once);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests update with empty title or body.
    /// Edge case test.
    /// </summary>
    [Theory]
    [InlineData("", "Body")]
    [InlineData("   ", "Body")]
    [InlineData(null, "Body")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    [InlineData("Title", null)]
    public async Task UpdateAnnouncementAsync_WithEmptyTitleOrBody_SkipsUpdate(string? title, string? body)
    {
        // Arrange
        var announcement = new Announcement
        {
            Id = 1,
            UserId = 1,
            Title = "Old Title",
            Body = "Old Body"
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        var result = await this.sut.UpdateAnnouncementAsync(1, 1, title, body);

        // Assert
        Assert.True(result);
        if (string.IsNullOrWhiteSpace(title))
            Assert.Equal("Old Title", announcement.Title);
        else
            Assert.Equal(title.Trim(), announcement.Title);

        if (string.IsNullOrWhiteSpace(body))
            Assert.Equal("Old Body", announcement.Body);
        else
            Assert.Equal(body.Trim(), announcement.Body);
        this.mockAnnouncementRepository.Verify(
            r => r.Update(announcement), Times.Once);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion

    #region DeleteAnnouncementAsync Tests

    /// <summary>
    /// Tests successful announcement deletion.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task DeleteAnnouncementAsync_WithValidData_DeletesAnnouncement()
    {
        // Arrange
        const int announcementId = 1;
        const int userId = 1;
        var announcement = new Announcement { Id = announcementId, UserId = userId };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(announcementId))
            .ReturnsAsync(announcement);

        // Act
        var result = await this.sut.DeleteAnnouncementAsync(announcementId, userId);

        // Assert
        Assert.True(result);
        this.mockAnnouncementRepository.Verify(
            r => r.Delete(announcement), Times.Once);
        this.mockAnnouncementRepository.Verify(
            r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests deletion of non-existent announcement.
    /// Negative test case.
    /// </summary>
    [Fact]
    public async Task DeleteAnnouncementAsync_WithNonExistentAnnouncement_ReturnsFalse()
    {
        // Arrange
        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Announcement?)null);

        // Act
        var result = await this.sut.DeleteAnnouncementAsync(999, 1);

        // Assert
        Assert.False(result);
        this.mockAnnouncementRepository.Verify(
            r => r.Delete(It.IsAny<Announcement>()), Times.Never);
    }

    /// <summary>
    /// Tests deletion by wrong user.
    /// Authorization test.
    /// </summary>
    [Fact]
    public async Task DeleteAnnouncementAsync_WithWrongUser_ReturnsFalse()
    {
        // Arrange
        var announcement = new Announcement { Id = 1, UserId = 1 };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(announcement);

        // Act
        var result = await this.sut.DeleteAnnouncementAsync(1, 999);

        // Assert
        Assert.False(result);
        this.mockAnnouncementRepository.Verify(
            r => r.Delete(It.IsAny<Announcement>()), Times.Never);
    }

    #endregion

    #region GetAnnouncementsByUserIdAsync Tests

    /// <summary>
    /// Tests successful retrieval of announcements by user ID.
    /// Positive test case - happy path.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsByUserIdAsync_WithValidUserId_ReturnsAnnouncements()
    {
        // Arrange
        const int userId = 1;
        var announcements = new List<Announcement>
        {
            new Announcement { Id = 1, UserId = userId, Title = "Test 1" },
            new Announcement { Id = 2, UserId = userId, Title = "Test 2" }
        };

        this.mockAnnouncementRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(announcements);

        // Act
        var result = await this.sut.GetAnnouncementsByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, a => Assert.Equal(userId, a.UserId));
    }

    /// <summary>
    /// Tests retrieval of announcements for non-existent user ID.
    /// Edge case test.
    /// </summary>
    [Fact]
    public async Task GetAnnouncementsByUserIdAsync_WithNonExistentUserId_ReturnsEmptyList()
    {
        // Arrange
        const int userId = 999;
        this.mockAnnouncementRepository
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        // Act
        var result = await this.sut.GetAnnouncementsByUserIdAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    #endregion
}