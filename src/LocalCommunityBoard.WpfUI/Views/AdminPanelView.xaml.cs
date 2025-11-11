// <copyright file="AdminPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the administrative panel view for managing users.
/// </summary>
/// <remarks>This control is a part of the application's user interface and is responsible for displaying and
/// managing user data. It initializes required services and loads user information asynchronously when the control is
/// instantiated.</remarks>
public partial class AdminPanelView : UserControl
{
    private const string ErrorText = "Error";
    private readonly IUserService userService;
    private readonly IReportService reportService;
    private readonly ICommentService commentService;

    public AdminPanelView()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        this.reportService = App.Services.GetRequiredService<IReportService>();
        this.commentService = App.Services.GetRequiredService<ICommentService>();
        _ = this.LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await this.LoadUsersAsync();
        await this.LoadReportedAnnouncementsAsync();
        await this.LoadReportedCommentsAsync();
    }

    private async Task LoadUsersAsync()
    {
        var users = await this.userService.GetAllUsersAsync();
        this.UsersGrid.ItemsSource = users;
    }

    private async Task LoadReportedAnnouncementsAsync()
    {
        var reports = await this.reportService.GetReportsByStatusAsync(ReportStatus.Open);
        var announcementReports = reports
            .Where(r => r.TargetType == TargetType.Announcement)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        this.ReportedAnnouncementsGrid.ItemsSource = announcementReports;
    }

    private async Task LoadReportedCommentsAsync()
    {
        var reports = await this.reportService.GetReportsByStatusAsync(ReportStatus.Open);
        var commentReports = reports
            .Where(r => r.TargetType == TargetType.Comment)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        this.ReportedCommentsGrid.ItemsSource = commentReports;
    }

    private void EditUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        // Navigate to edit view
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            var editView = new EditUserView(user, this.userService, async () =>
            {
                // This callback will refresh the user list after save
                await this.LoadUsersAsync();
            });

            mainWindow.MainContent.Content = editView;
        }
    }

    private async void BlockUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        var confirm = MessageBox.Show($"Block user '{user.Username}'?", "Confirm block", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var result = await this.userService.BlockUserAsync(user.Id);
            if (!result)
            {
                MessageBox.Show("Unable to block user. This may be an administrator or the user does not exist.", ErrorText, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            await this.LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during blocking: {ex.Message}", ErrorText, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void UnblockUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Unblock user '{user.Username}'?",
            "Confirm unblock",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var result = await this.userService.UnblockUserAsync(user.Id);
            if (!result)
            {
                MessageBox.Show(
                    "Не вдалось розблокувати користувача. Ймовірно користувач не існує.",
                    ErrorText,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // оновлюємо таблицю після успішної зміни статусу
            await this.LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка при розблокуванні: {ex.Message}",
                ErrorText,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void EditAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Edit announcement feature not implemented yet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void DeleteAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not Report report)
        {
            return;
        }

        if (report.TargetType != TargetType.Announcement)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Видалити оголошення (ID={report.TargetId}) для звіту (ID={report.Id})?",
            "Підтвердження видалення",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        btn.IsEnabled = false;

        try
        {
            var deleted = await this.reportService.DeleteAnnouncementByReportAsync(report.Id);

            if (!deleted)
            {
                MessageBox.Show(
                    "Оголошення не було видалено (можливо, воно вже видалене).",
                    "Інформація",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                try
                {
                    await this.reportService.UpdateReportStatusAsync(report.Id, ReportStatus.Closed);
                }
                catch
                {
                }
            }

            await this.LoadReportedAnnouncementsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка при видаленні оголошення: {ex.Message}",
                ErrorText,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            btn.IsEnabled = true;
        }
    }

    private void EditComment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not Report report)
        {
            return;
        }

        if (report.TargetType != TargetType.Comment)
        {
            return;
        }

        try
        {
            var comment = this.commentService.GetByIdAsync(report.TargetId).Result;

            if (comment == null)
            {
                MessageBox.Show(
                    "Comment not found. It may have been deleted.",
                    ErrorText,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var editWindow = new EditCommentWindow(comment, this.commentService, async () =>
            {
                // Refresh the reported comments grid after save
                await this.LoadReportedCommentsAsync();
            });

            editWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error while opening edit window: {ex.Message}",
                ErrorText,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void DeleteComment_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not Report report)
        {
            return;
        }

        if (report.TargetType != TargetType.Comment)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete comment (ID={report.TargetId}) for report (ID={report.Id})?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        btn.IsEnabled = false;
        try
        {
            var deleted = await this.reportService.DeleteCommentByReportAsync(report.Id);
            if (!deleted)
            {
                MessageBox.Show(
                    "Comment was not deleted (it may already be removed).",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                // Mark the report as closed to reflect that action was taken.
                try
                {
                    await this.reportService.UpdateReportStatusAsync(report.Id, ReportStatus.Closed);
                }
                catch
                {
                    // Non-fatal: if updating the report status fails, continue to refresh UI.
                }
            }

            // Refresh the comments reports grid (fix: was previously refreshing announcements).
            await this.LoadReportedCommentsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error while deleting comment: {ex.Message}",
                ErrorText,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            btn.IsEnabled = true;
        }
    }
}
