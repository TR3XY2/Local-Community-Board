// <copyright file="TopBar.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Controls;

using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.WpfUI.Views;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// TopBar control that handles login/logout UI state.
/// </summary>
public partial class TopBar : UserControl
{
    private readonly UserSession? session;

    public TopBar()
    {
        this.InitializeComponent();

        if (App.Services != null)
        {
            this.session = App.Services.GetRequiredService<UserSession>();
            this.UpdateUI();
        }
        else
        {
            this.LoginButton.Visibility = Visibility.Visible;
            this.SignupButton.Visibility = Visibility.Visible;
            this.LogoutButton.Visibility = Visibility.Collapsed;
            this.ProfileButton.Visibility = Visibility.Collapsed;
        }
    }

    public event RoutedEventHandler? HomeRequested;

    public event RoutedEventHandler? ProfileRequested;

    public event RoutedEventHandler? CreatePostRequested;

    public event RoutedEventHandler? AdminPanelRequested;

    private void UpdateUI()
    {
        bool loggedIn = this.session?.IsLoggedIn == true;
        bool isAdmin = this.session?.CurrentUser?.Role?.Name == "Admin" || this.session?.CurrentUser?.RoleId == 2;

        this.LoginButton.Visibility = loggedIn ? Visibility.Collapsed : Visibility.Visible;
        this.SignupButton.Visibility = loggedIn ? Visibility.Collapsed : Visibility.Visible;
        this.LogoutButton.Visibility = loggedIn ? Visibility.Visible : Visibility.Collapsed;
        this.ProfileButton.Visibility = loggedIn ? Visibility.Visible : Visibility.Collapsed;
        this.CreatePostButton.Visibility = loggedIn ? Visibility.Visible : Visibility.Collapsed;
        this.AdminPanelButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Login_Click(object sender, RoutedEventArgs e)
    {
        var window = new LoginWindow();
        if (window.ShowDialog() == true)
        {
            this.UpdateUI();
        }
    }

    private void Signup_Click(object sender, RoutedEventArgs e)
    {
        new SignUpWindow().ShowDialog();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        this.session?.Logout();
        this.UpdateUI();
        this.HomeRequested?.Invoke(this, new RoutedEventArgs());
    }

    private void Profile_Click(object sender, RoutedEventArgs e)
    {
        this.ProfileRequested?.Invoke(this, new RoutedEventArgs());
    }

    private void Home_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        this.HomeRequested?.Invoke(this, new RoutedEventArgs());
    }

    private void CreatePost_Click(object sender, RoutedEventArgs e)
    {
        this.CreatePostRequested?.Invoke(this, e);
    }

    private void AdminPanel_Click(object sender, RoutedEventArgs e)
    {
        this.AdminPanelRequested?.Invoke(this, e);
    }
}
