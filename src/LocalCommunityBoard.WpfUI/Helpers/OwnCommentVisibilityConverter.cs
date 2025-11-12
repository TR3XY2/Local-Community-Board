// <copyright file="OwnCommentVisibilityConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Helpers;

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LocalCommunityBoard.Application.Services;
using Microsoft.Extensions.DependencyInjection;

public class OwnCommentVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int commentUserId && App.Services.GetRequiredService<UserSession>().CurrentUser?.Id == commentUserId)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
