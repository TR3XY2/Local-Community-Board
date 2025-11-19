// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Windows;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.WpfUI.Views;

/// <summary>
/// Represents the main window of the application, providing the primary user interface and navigation functionality.
/// </summary>
/// <remarks>
/// This class is responsible for initializing the main window and handling navigation between different
/// views, such as the home view and the profile view. It subscribes to events from the top bar to trigger
/// navigation.
/// </remarks>
public partial class MainWindow : Window
{
    private readonly IAnnouncementService announcementService;
    private readonly UserSession userSession;

    public MainWindow(IAnnouncementService announcementService, UserSession userSession)
    {
        this.InitializeComponent();

        // Dependency-injected service from App.xaml.cs
        this.announcementService = announcementService;

        this.TopBar.CreatePostRequested += this.TopBar_CreatePostRequested;

        this.TopBar.AdminPanelRequested += (_, _) => this.MainContent.Content = new AdminPanelView();

        this.NavigateHome();
        this.userSession = userSession;
    }

    public void NavigateHome()
    {
        this.MainContent.Content = new MainView(this.announcementService);
    }

    private void TopBar_HomeRequested(object sender, RoutedEventArgs e)
        => this.NavigateHome();

    private void TopBar_ProfileRequested(object sender, RoutedEventArgs e)
        => this.NavigateProfile();

    private void TopBar_CreatePostRequested(object sender, RoutedEventArgs e)
    {
        // Перехід на сторінку створення оголошення — передаємо announcementService
        this.MainContent.Content = new Views.CreatePostView(this.announcementService, this.userSession);
    }

    private void NavigateProfile()
    {
        this.MainContent.Content = new ProfileView();
    }
}
