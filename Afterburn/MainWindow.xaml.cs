﻿using Afterburn.Messages;
using Afterburn.Model;
using Afterburn.ViewModel;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;

namespace Afterburn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string title = "";

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            Messenger.Default.Register<SaveMessage>(this, (m) =>
            {
                var dialog = new SaveFileDialog();
                dialog.FileName = string.IsNullOrWhiteSpace(title) 
                    ? "New Project" : title;
                dialog.DefaultExt = ".afterburn";
                dialog.Filter = "Afterburn files (.afterburn)|*.afterburn";
                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var filename = dialog.FileName;
                    title = Path.GetFileNameWithoutExtension(filename);
                    this.Title = "AFTERBURN - " + title;
                    var data = new FileFactory().Create((MainViewModel)this.DataContext);
                    var json = JsonConvert.SerializeObject(data);

                    System.IO.File.WriteAllText(filename, json);
                }
            });
            Messenger.Default.Register<LoadMessage>(this, (m) =>
            {
                var dialog = new OpenFileDialog();
                dialog.DefaultExt = ".afterburn";
                dialog.Filter = "Afterburn files (.afterburn)|*.afterburn";

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var filename = dialog.FileName;
                    title = Path.GetFileNameWithoutExtension(filename);
                    this.Title = "AFTERBURN - " + title;
                    var data = System.IO.File.ReadAllText(filename);

                    var file = JsonConvert.DeserializeObject<AfterburnFile>(data);

                    var main = SimpleIoc.Default.GetInstance<ViewModelLocator>().Main;
                    main.LoadState(file);
                }
            });
        }

        private void OpenFlyout(object sender, System.Windows.RoutedEventArgs e)
        {
            flyout.IsOpen = !flyout.IsOpen;
        }
    }
}