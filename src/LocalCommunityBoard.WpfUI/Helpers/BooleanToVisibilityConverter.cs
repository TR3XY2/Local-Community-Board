// <copyright file="BooleanToVisibilityConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Helpers
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEmpty = string.IsNullOrEmpty(value?.ToString());
            return isEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
