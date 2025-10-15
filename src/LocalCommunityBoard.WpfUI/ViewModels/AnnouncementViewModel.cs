// <copyright file="AnnouncementViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.ViewModels;

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;

public class AnnouncementViewModel : LocalCommunityBoard.Domain.Entities.Announcement, INotifyPropertyChanged
{
    private static readonly ConcurrentDictionary<string, BitmapImage> imageCache = new();
    private BitmapImage? image;

    public event PropertyChangedEventHandler? PropertyChanged;

    public BitmapImage? Image
    {
        get => this.image;
        private set
        {
            this.image = value;
            this.OnPropertyChanged();
        }
    }

    public async Task LoadImageAsync()
    {
        if (string.IsNullOrWhiteSpace(this.ImageUrl))
        {
            this.Image = GetPlaceholder();
            return;
        }

        if (imageCache.TryGetValue(this.ImageUrl, out var cached))
        {
            this.Image = cached;
            return;
        }

        try
        {
            Console.WriteLine($"[LoadImage] URL: {this.ImageUrl}");

            if (!Uri.TryCreate(this.ImageUrl, UriKind.Absolute, out var uri))
            {
                this.Image = GetPlaceholder();
                return;
            }

            // Download image asynchronously
            using var httpClient = new System.Net.Http.HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(uri);

            await Task.Run(() =>
            {
                using var stream = new MemoryStream(imageBytes);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.DecodePixelWidth = 230;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                imageCache[this.ImageUrl] = bitmap;
                this.Image = bitmap;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageLoadError] {this.ImageUrl}: {ex.Message}");
            this.Image = GetPlaceholder();
        }
    }

    /// <summary>
    /// Returns a placeholder image from local assets (no hardcoded path).
    /// </summary>
    private static BitmapImage GetPlaceholder()
    {
        try
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var uriString = $"pack://application:,,,/{assemblyName};component/Assets/placeholder.png";

            if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            {
                return new BitmapImage();
            }

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = uri;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return new BitmapImage();
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
