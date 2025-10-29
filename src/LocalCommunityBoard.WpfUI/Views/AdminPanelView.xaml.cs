// <copyright file="AdminPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the administrative panel view for managing users.
/// </summary>
/// <remarks>This control is a part of the application's user interface and is responsible for displaying and
/// managing user data. It initializes required services and loads user information asynchronously when the control is
/// instantiated.</remarks>
public partial class AdminPanelView : UserControl
{
    private readonly IUserService userService;

    public AdminPanelView()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        _ = this.LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        var users = await this.userService.GetAllUsersAsync();
        this.UsersGrid.ItemsSource = users;
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
                MessageBox.Show("Не вдалося заблокувати користувача. Можливо це адмін або користувача не існує.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            await this.LoadUsersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка при блокуванні: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    "Error",
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
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
