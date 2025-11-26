// <copyright file="EditUserView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.WpfUI.ViewModels;

/// <summary>
/// Represents a view for editing user information by administrators.
/// </summary>
public partial class EditUserView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EditUserView"/> class.
    /// </summary>
    /// <param name="user">The user to edit.</param>
    /// <param name="userService">The user service for database operations.</param>
    /// <param name="onSaveCallback">Async callback to execute after successful save.</param>
    public EditUserView(User user, IUserService userService, Func<Task>? onSaveCallback = null)
    {
        this.InitializeComponent();

        this.DataContext = new EditUserViewModel(
            user,
            userService,
            onSaveCallback);
    }

    // PasswordBox не підтримує нормальний binding, тому прокидуємо значення в ViewModel вручну.
    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (this.DataContext is EditUserViewModel vm)
        {
            vm.NewPassword = this.NewPasswordBox.Password;
        }
    }
}
