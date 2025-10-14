// <copyright file="MainWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI;

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LocalCommunityBoard.WpfUI.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
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
        this.MainContent.Content = new HomeView();
    }

    private void NavigateProfile()
    {
        this.MainContent.Content = new ProfileView();
    }
}
