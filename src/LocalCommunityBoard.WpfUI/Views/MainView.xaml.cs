// <copyright file="MainView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    /// <summary>
    /// Interaction logic for MainView.xaml.
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            this.InitializeComponent();
        }

        private void CleaFilters_Click(object sender, RoutedEventArgs e)
        {
            this.CityInput.Text = string.Empty;
            this.DistrictInput.Text = string.Empty;
            this.StreetInput.Text = string.Empty;

            this.NewCheck.IsChecked = false;
            this.PostsCheck.IsChecked = false;
            this.EventsCheck.IsChecked = false;
        }
    }
}
