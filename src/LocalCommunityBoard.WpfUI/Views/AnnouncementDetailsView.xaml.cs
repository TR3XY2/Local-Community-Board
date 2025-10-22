// <copyright file="AnnouncementDetailsView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.WpfUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a user control for displaying the details of an announcement, including its comments and functionality
/// for posting new comments.
/// </summary>
/// <remarks>This control is designed to display the details of a specific announcement, including its associated
/// comments. It allows users to post new comments if they are logged in. Comments are loaded asynchronously after the
/// control is initialized. The control relies on dependency-injected services for comment management and user session
/// handling.</remarks>
public partial class AnnouncementDetailsView : UserControl
{
    private const string ErrorText = "An error occurred while loading the announcement details.";
    private readonly ICommentService commentService;
    private readonly IReportService reportService;
    private readonly UserSession session;
    private readonly AnnouncementViewModel announcement;

    public AnnouncementDetailsView(AnnouncementViewModel announcement)
    {
        this.InitializeComponent();
        this.DataContext = announcement;

        this.commentService = App.Services.GetRequiredService<ICommentService>();
        this.reportService = App.Services.GetRequiredService<IReportService>();
        this.session = App.Services.GetRequiredService<UserSession>();
        this.announcement = announcement;

        // Load comments asynchronously after UI is ready
        _ = this.LoadCommentsAsync();

        // ADDED: ��� ����������� � ��������� �������� ����� � ���� ������
        this.Loaded += async (_, __) =>
        {
            await this.RefreshLikesAsync();
            await this.UpdateLikeVisualAsync();
        };
    }

    private async Task LoadCommentsAsync()
    {
        try
        {
            var comments = await this.commentService.GetCommentsForAnnouncementAsync(this.announcement.Id);
            this.CommentsList.ItemsSource = comments;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load comments: {ex.Message}", ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void PostComment_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
        {
            MessageBox.Show("You must be logged in to post a comment.", ErrorText, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var body = this.CommentBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(body))
        {
            MessageBox.Show("Comment cannot be empty.", ErrorText, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            await this.commentService.AddCommentAsync(this.session.CurrentUser.Id, this.announcement.Id, body);
            this.CommentBox.Text = string.Empty;
            await this.LoadCommentsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to post comment: {ex.Message}", ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ADDED
    private async void LikeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser is null)
        {
            MessageBox.Show("You must be logged in to like.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var reactionService = App.Services.GetRequiredService<IReactionService>();

            // toggle like в БД
            await reactionService.ToggleLikeAsync(this.announcement.Id, this.session.CurrentUser.Id);

            // оновити лічильник і візуальний стан
            await this.RefreshLikesAsync();
            await this.UpdateLikeVisualAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to toggle like: {ex.Message}", ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ADDED: підтягнути кількість лайків
    private async Task RefreshLikesAsync()
    {
        try
        {
            var reactionService = App.Services.GetRequiredService<IReactionService>();
            var count = await reactionService.GetLikesCountAsync(this.announcement.Id);
            this.LikeCountText.Text = count.ToString();
        }
        catch
        {
            // Intentionally ignored: UI update is non-critical if like count fails to load.
        }
    }

    // ADDED: “запам’ятати” стан для поточного юзера (підсвічення кнопки)
    private async Task UpdateLikeVisualAsync()
    {
        var userId = this.session.CurrentUser?.Id ?? 0;
        if (userId == 0)
        {
            this.LikeBtn.IsEnabled = false;
            return;
        }

        try
        {
            var reactionService = App.Services.GetRequiredService<IReactionService>();
            var hasLiked = await reactionService.HasUserLikedAsync(this.announcement.Id, userId);

            // легка індикація активного лайка
            this.LikeBtn.Opacity = hasLiked ? 1.0 : 0.85;
        }
        catch
        {
            // Intentionally ignored: UI update is non-critical if like state fails to load.
        }
    }

    private async void ReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
        {
            MessageBox.Show("You must be logged in to report a comment.", ErrorText, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (sender is Button button && button.Tag is int commentId)
        {
            // Prompt for reason
            string reason = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter reason for reporting this comment:",
                "Report Comment",
                "Inappropriate content");

            if (string.IsNullOrWhiteSpace(reason))
            {
                return;
            }

            try
            {
                await this.reportService.ReportCommentAsync(this.session.CurrentUser.Id, commentId, reason);
                MessageBox.Show("Comment reported successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to report comment: {ex.Message}", ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
