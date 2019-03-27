﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
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
using Newtonsoft.Json.Linq;
using System.Net;

namespace Lab02_parser
{
    public partial class MainWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker();

        static HttpClient client = new HttpClient();

        ObservableCollection<Element> items = new ObservableCollection<Element> { };

        public ObservableCollection<Element> Items
        {
            get => items;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            //worker.DoWork += DownloadPackofData_Click;
            worker.ProgressChanged += Worker_ProgressChanged;
        }

        private async void DownloadJsonButton_Click(object sender, RoutedEventArgs e)
        {
            loading_spinner.Visibility = Visibility.Visible;
            status_TextBlock.Text = "Loading...";

            // https://en.wikipedia.org/w/api.php?format=json&action=query&generator=random&prop=images&grnnamespace=0
            var url = "http://api.icndb.com/jokes/random";
            var urlImg = "https://some-random-api.ml/dogimg";
            try
            {
                HttpResponseMessage res = await client.GetAsync(url);
                res.EnsureSuccessStatusCode();
                string responseBody = await res.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(responseBody);

                HttpResponseMessage resImg = await client.GetAsync(urlImg);
                resImg.EnsureSuccessStatusCode();
                string responseBodyImg = await resImg.Content.ReadAsStringAsync();
                JObject jsonImg = JObject.Parse(responseBodyImg);
                string parsed_joke = (string)json["value"]["joke"];
                int parsed_number = (int)json["value"]["id"];
                string parsed_image_url = (string)jsonImg["link"];

                items.Add(new Element { Text = parsed_joke, Number = parsed_number, ImageUrl = parsed_image_url });
                loading_spinner.Visibility = Visibility.Hidden;
                status_TextBlock.Text = "Done.";
            } catch(Exception err) {
                loading_spinner.Visibility = Visibility.Hidden;
                status_TextBlock.Text = "Error: "+ err;
            }
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DataProgressBar.Value = e.ProgressPercentage;
            DownloadPacks_TextBlock.Text = e.UserState as string;
        }

        private async void DownloadPackofData_Click(object sender, RoutedEventArgs e)
        {
            //BackgroundWorker worker = sender as BackgroundWorker;

            var url = "http://api.icndb.com/jokes/random";
            var urlImg = "https://some-random-api.ml/dogimg";
            try
            {
                int FinalNumber = int.Parse(this.NumberTextBox.Text);
                for (int i = 0; i < FinalNumber; i++)
                {
                    /*
                    if (worker.CancellationPending == true)
                    {
                        worker.ReportProgress(0, "Cancelled");
                        e.Cancel = true;
                        return;
                    }*/
                    
                        HttpResponseMessage res = await client.GetAsync(url);
                        res.EnsureSuccessStatusCode();
                        string responseBody = await res.Content.ReadAsStringAsync();
                        JObject json = JObject.Parse(responseBody);

                        HttpResponseMessage resImg = await client.GetAsync(urlImg);
                        resImg.EnsureSuccessStatusCode();
                        string responseBodyImg = await resImg.Content.ReadAsStringAsync();
                        JObject jsonImg = JObject.Parse(responseBodyImg);
                        string parsed_joke = (string)json["value"]["joke"];
                        int parsed_number = (int)json["value"]["id"];
                        string parsed_image_url = (string)jsonImg["link"];

                        items.Add(new Element { Text = parsed_joke, Number = parsed_number, ImageUrl = parsed_image_url });

                        int process_number = i + 1;
                    worker.ReportProgress((int)Math.Round((float)i * 100.0 / (float)FinalNumber), "Loading item " + process_number + "...");
                }

            }
            catch (Exception ex)
            {
                this.progressTextBlock.Text = "Error! " + ex.Message;
            }
            worker.ReportProgress(100, "Done");
            //if (worker.IsBusy != true)
            //  worker.RunWorkerAsync();

        }
        

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (worker.WorkerSupportsCancellation == true)
            {
                DownloadPacks_TextBlock.Text = "Cancelling...";
                worker.CancelAsync();
            }
        }
     }
}