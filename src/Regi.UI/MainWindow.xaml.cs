using Regi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
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

namespace Regi.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Projects.DataContext = new List<object>
            {
                new { Bar = "Wumbo" }
            };
        }

        public StartupConfig RegiConfiguration { get; set; }

        public ObservableCollection<string> Test { get; set; } = new ObservableCollection<string> { "wumbo" };

        private void Select_File(object sender, RoutedEventArgs e)
        {
            //var task = Task.Run(async () =>
            //{
            //    await Task.Delay(2000);

            //    Dispatcher.Invoke(() => ConfigurationOutput.Text = "Hello?");
            //});

            //ConfigurationOutput.Text = "You clicked a button!";

            var dlg = new Microsoft.Win32.OpenFileDialog();

            var result = dlg.ShowDialog();

            if (result != true)
            {
                return;
            }

            //ConfigurationOutput.Text = dlg.FileName;


            var task = Task.Run(async () =>
            {
                using var fileStream = dlg.OpenFile();

                Dispatcher.Invoke(() =>
                {
                    //ConfigurationOutput.Text = fileStream.Length.ToString();
                    //ConfigurationOutput.Text = "About to do the thing...";
                });

                var startupConfig = await JsonSerializer.DeserializeAsync<StartupConfig>(fileStream, Constants.DefaultSerializerOptions);

                Dispatcher.Invoke(() =>
                {
                    //ConfigurationOutput.Text = "Did it!";

                    //ConfigurationOutput.Text = startupConfig.Apps.ToString();
                });
            });


            //var deserializeTask = fileStream.read
            //var deserializeTask = JsonSerializer.DeserializeAsync<StartupConfig>(fileStream).AsTask();

            //deserializeTask.ContinueWith((t) =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {

            //        ConfigurationOutput.Text = t.Result.Apps.Count.ToString(); // $"App Count: {config.Apps?.Count}";

            //    });
            //});
            //var config = await JsonSerializer.DeserializeAsync<StartupConfig>(fileStream);

            //var task = Task.Run(async () =>
            //{
            //});

        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectFileButton.IsEnabled = false;
            //ConfigurationOutput.Text = "Starting long task";

            //var task = Task.Run(() =>
            //{
            //    Thread.Sleep(2000);
            //});

            //task.ContinueWith((t) =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        SelectFileButton.IsEnabled = true;
            //        ConfigurationOutput.Text = "Job's done.";
            //    });
            //});
        }

        int test = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ConfigurationOutput.Text = "It works!" + test++;
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((List<object>)Projects.DataContext).Add(new { Bar = "Please work!" });
        }
    }
}
