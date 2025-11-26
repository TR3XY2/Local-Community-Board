// <copyright file="SignUpWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views
{
    using System;
    using System.Windows;
    using LocalCommunityBoard.Application.Interfaces;
    using LocalCommunityBoard.WpfUI.ViewModels;

#pragma warning disable SA1601 // Partial elements should be documented
    public partial class SignUpWindow : Window
#pragma warning restore SA1601 // Partial elements should be documented
    {
        public SignUpWindow(IUserService userService)
        {
            this.InitializeComponent();

            this.DataContext = new SignUpViewModel(userService);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SignUpViewModel viewModel)
            {
                viewModel.Password = this.PasswordBox.Password;
            }
        }
    }
}
