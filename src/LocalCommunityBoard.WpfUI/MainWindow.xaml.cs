// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Windows;
using LocalCommunityBoard.WpfUI.Views;

/// <summary>
/// Represents the main window of the application, providing the primary user interface and navigation functionality.
/// </summary>
/// <remarks>This class is responsible for initializing the main window and handling navigation between different
/// views,  such as the home view and the profile view. It subscribes to events from the top bar to trigger
/// navigation.</remarks>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        this.NavigateHome();
    }

    private void TopBar_HomeRequested(object sender, RoutedEventArgs e)
        => this.NavigateHome();

    private void TopBar_ProfileRequested(object sender, RoutedEventArgs e)
        => this.NavigateProfile();

    private void NavigateHome()
    {
        this.MainContent.Content = new MainView();
    }

    private void NavigateProfile()
    {
        this.MainContent.Content = new ProfileView();
    }
}
