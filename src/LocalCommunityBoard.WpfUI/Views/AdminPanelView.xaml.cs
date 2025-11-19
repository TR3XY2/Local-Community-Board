// <copyright file="AdminPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
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
    private readonly IAnnouncementService announcementService;
    private readonly UserSession session;

    public AdminPanelView()
    {
        this.InitializeComponent();

        this.session = App.Services.GetRequiredService<UserSession>();

        this.userService = App.Services.GetRequiredService<IUserService>();
        this.reportService = App.Services.GetRequiredService<IReportService>();
        this.commentService = App.Services.GetRequiredService<ICommentService>();
        this.announcementService = App.Services.GetRequiredService<IAnnouncementService>();
        this.Loaded += this.AdminPanelView_Loaded;

        _ = this.LoadDataAsync();
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj)
        where T : DependencyObject
    {
        if (depObj == null)
        {
            yield break;
        }

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T t)
            {
                yield return t;
            }

            foreach (T childOfChild in FindVisualChildren<T>(child))
            {
                yield return childOfChild;
            }
        }
    }

    private async Task LoadDataAsync()
    {
        await this.LoadUsersAsync();
        await this.LoadReportedAnnouncementsAsync();
        await this.LoadReportedCommentsAsync();
    }

    private void AdminPanelView_Loaded(object sender, RoutedEventArgs e)
    {
        this.UpdateButtons();
    }

    private void UpdateButtons()
{
    bool isSuperAdmin = this.session.CurrentUser?.RoleId == 3;

    foreach (var item in this.UsersGrid.Items)
    {
        if (this.UsersGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
        {
            var user = item as User;

            var giveAdminButton = this.FindButton(row, "GiveAdminButton");
            if (giveAdminButton != null && user != null)
            {
                giveAdminButton.Visibility =
                    (isSuperAdmin && user.RoleId == 1) ? Visibility.Visible : Visibility.Collapsed;
            }

            var demoteAdminButton = this.FindButton(row, "DemoteAdminButton");
            if (demoteAdminButton != null && user != null)
            {
                demoteAdminButton.Visibility =
                    (isSuperAdmin && user.RoleId == 2) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}

    private async Task LoadUsersAsync()
    {
        var users = await this.userService.GetAllUsersAsync();

        this.UsersGrid.ItemsSource = null;
        this.UsersGrid.ItemsSource = users;

        await Task.Delay(50);
        this.UpdateButtons();
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

    private async void GiveAdmin_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        var confirm = MessageBox.Show($"Promote '{user.Username}' to Admin?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            bool ok = await this.userService.PromoteToAdminAsync(user.Id);
            if (!ok)
            {
                MessageBox.Show("Cannot promote this user.", ErrorText);
                return;
            }

            await this.LoadUsersAsync();
            this.UpdateButtons();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", ErrorText);
        }
    }

    private Button? FindButton(DependencyObject container, string name)
    {
        var buttons = FindVisualChildren<Button>(container);
        return buttons.FirstOrDefault(b => b.Name == name);
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

    private async void EditAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not Report report)
        {
            return;
        }

        if (report.TargetType != TargetType.Announcement)
        {
            return;
        }

        var announcement = await this.reportService.GetAnnouncementByReportAsync(report.Id);
        if (announcement is null)
        {
            MessageBox.Show("Оголошення не знайдено або вже видалено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var editView = new EditAnnouncementView(
            announcement,
            this.announcementService,
            async () =>
            {
                await this.reportService.UpdateReportStatusAsync(report.Id, ReportStatus.Closed);
                await this.LoadReportedAnnouncementsAsync();
            });

        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.MainContent.Content = editView;
        }
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

    private async void DemoteAdmin_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        // додатковий захист (кнопка і так не відобразиться для не-Admin)
        if (user.RoleId != 2)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Remove Admin role from '{user.Username}'?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        btn.IsEnabled = false;
        try
        {
            bool ok = await this.userService.DemoteFromAdminAsync(user.Id);
            if (!ok)
            {
                MessageBox.Show("Cannot remove admin role from this user.", ErrorText);
                return;
            }

            await this.LoadUsersAsync();
            this.UpdateButtons();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", ErrorText);
        }
        finally
        {
            btn.IsEnabled = true;
        }
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.DataContext is not User user)
        {
            return;
        }

        // Extra safety check - prevent deletion of admins
        if (user.RoleId == 2 || user.RoleId == 3)
        {
            MessageBox.Show(
                "Cannot delete administrator accounts.",
                "Operation Not Allowed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Single confirmation
        var confirm = MessageBox.Show(
            $"Delete user '{user.Username}'?\n\n",
            "Confirm Account Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        btn.IsEnabled = false;

        try
        {
            var result = await this.userService.DeleteUserByAdminAsync(user.Id);

            if (result)
            {
                MessageBox.Show(
                    $"User '{user.Username}' has been deleted successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                await this.LoadUsersAsync();
            }
            else
            {
                MessageBox.Show(
                    "Failed to delete user. The user may not exist.",
                    ErrorText,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                ex.Message,
                "Operation Not Allowed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error while deleting user: {ex.Message}",
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
