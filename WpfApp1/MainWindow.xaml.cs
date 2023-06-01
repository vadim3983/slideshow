using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Islidesshow;
using WpfApp1;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace ImageSlideshowApp;

public partial class MainWindow : Window
{
    private readonly List<ISlideshowEffect> _slideshowEffects;

    public MainWindow()
    {
        InitializeComponent();
        fileInfoTextBlock.Text = "No file selected";
        foreach (var drive in Directory.GetLogicalDrives())
        {
            var item = new TreeViewItem
            {
                Header = drive,
                Tag = drive
            };

            item.Items.Add(null);
            item.Expanded += Folder_Expanded;

            folderTreeView.Items.Add(item);
        }

        var path = @"plugins";

        _slideshowEffects = LoadPlugins(path).ToList();

        foreach (var effect in _slideshowEffects) slideshowEffectsComboBox.Items.Add(effect.Name);
    }

    private static IEnumerable<ISlideshowEffect> LoadPlugins(string path)
    {
        var dllFileNames = Directory.GetFiles(path, "*.dll");
        var assemblies = new List<Assembly>(dllFileNames.Length);
        Console.WriteLine(string.Join("\n", dllFileNames));

        assemblies.AddRange(dllFileNames.Select(dllFile => Assembly.LoadFrom(dllFile)));

        var pluginType = typeof(ISlideshowEffect);
        var pluginTypes = new List<Type>();
        foreach (var types in assemblies.Select(assembly => assembly.GetTypes()
                     .Where(t => t.IsClass && !t.IsAbstract && t.GetInterface(pluginType.FullName) != null)))
            pluginTypes.AddRange(types);

        var plugins = new List<ISlideshowEffect>(pluginTypes.Count);
        plugins.AddRange(pluginTypes.Select(type => (ISlideshowEffect)Activator.CreateInstance(type)));

        return plugins;
    }


    private void OnSlideshowWindowClosed(object sender, EventArgs e)
    {
        IsEnabled = true;
    }

    private async void StartSlideshow_Click(object sender, RoutedEventArgs e)
    {
        var selectedEffectName = sender switch
        {
            MenuItem menuItem => menuItem.Header as string,
            Button => slideshowEffectsComboBox.SelectedItem as string,
            _ => string.Empty
        };

        var selectedEffect = _slideshowEffects.FirstOrDefault(effect => effect.Name == selectedEffectName);

        if (selectedEffect != null)
        {
            var imagePaths = imageListView.Items.Cast<object>()
                .Select(item => (string)item.GetType().GetProperty("Path").GetValue(item))
                .ToList();

            if (imagePaths.Count > 0)
            {
                var window = new SlideshowWindow();
                window.Owner = this;
                window.Show();

                window.Closed += OnSlideshowWindowClosed;

                IsEnabled = false;

                await window.PlaySlideshow(selectedEffect, imagePaths);
            }
            else
            {
                MessageBox.Show("No image files found!");
            }
        }
        else
        {
            MessageBox.Show("Please select an effect from the dropdown before starting the slideshow.");
        }
    }


    private void Folder_Expanded(object sender, RoutedEventArgs e)
    {
        var item = (TreeViewItem)sender;
        if (item.Items.Count != 1 || item.Items[0] != null) return;

        item.Items.Clear();

        var fullPath = (string)item.Tag;
        try
        {
            var dirs = Directory.GetDirectories(fullPath);
            foreach (var dir in dirs)
            {
                var subItem = new TreeViewItem
                {
                    Header = Path.GetFileName(dir),
                    Tag = dir
                };

                subItem.Items.Add(null);
                subItem.Expanded += Folder_Expanded;

                item.Items.Add(subItem);
            }
        }
        catch
        {
        }
    }

    private void OnFolderSelected(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        var selectedItem = (TreeViewItem)folderTreeView.SelectedItem;
        var fullPath = (string)selectedItem.Tag;

        imageListView.Items.Clear();
        try
        {
            var files = Directory.GetFiles(fullPath).Where(file =>
                file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".bmp") || file.ToLower().EndsWith(".gif"));
            foreach (var file in files)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(file);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                imageListView.Items.Add(new { Name = Path.GetFileName(file), Path = file, Image = bitmap });
            }
        }
        catch (Exception)
        {
        }
    }


    private void OnImageSelected(object sender, SelectionChangedEventArgs e)
    {
        if (imageListView.SelectedItem is not { } selectedItem)
        {
            fileInfoTextBlock.Text = "No file selected";
            return;
        }

        var fileInfo = new FileInfo(((dynamic)selectedItem).Path);
        if (!fileInfo.Exists) return;

        var name = fileInfo.Name;
        var size = fileInfo.Length / 1024;
        var bitmap = new BitmapImage(new Uri(fileInfo.FullName));
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;

        fileInfoTextBlock.Text = $"Name: {name}\nSize: {size} KB\nWidth: {width}\nHeight: {height}";
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        var path = dialog.SelectedPath;
        var files = Directory.GetFiles(path).Where(file =>
            file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".bmp") || file.ToLower().EndsWith(".gif"));

        imageListView.Items.Clear();

        foreach (var file in files)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(file);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            imageListView.Items.Add(new { Name = Path.GetFileName(file), Path = file, Image = bitmap });
        }
    }


    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("This is an Image Slideshow Application.", "About");
    }
}