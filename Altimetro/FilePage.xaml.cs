using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Altimetro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilePage : SharePage
    {
        public List<StorageFile> FilesList { get; private set; }
        public List<string> FilesListS { get; private set; }
        private List<StorageFile> selectedStorageItems;

        int selectedIndex;
        public FilePage()
        {
            this.InitializeComponent();
        }

        private void AppBar_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            GetFilesList();
        }

        public async void GetFilesList()
        {
            Windows.Storage.StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            IReadOnlyList<Windows.Storage.StorageFile> files = await folder.GetFilesAsync();
            
            FilesListS = new List<string>();
            foreach (var v in files)
            { FilesListS.Add(v.Name); }
            llsFiles.ItemsSource = FilesListS;
            selectedIndex = -1;
        }

        private async void Files_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = (ListView)sender;
            selectedIndex =lv.SelectedIndex;
            selectedStorageItems = new List<StorageFile>();
            selectedStorageItems.Add ( (StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(FilesListS.ElementAt(selectedIndex)));
        }

        private void AppBar_Send(object sender, RoutedEventArgs e)
        {
            string s= FilesListS.ElementAt(selectedIndex);
           
        }

        protected override bool GetShareContent(Windows.ApplicationModel.DataTransfer.DataRequest request)
        {
            bool succeeded = false;

            if (selectedStorageItems != null)
            {
                Windows.ApplicationModel.DataTransfer.DataPackage requestData = request.Data;
                requestData.Properties.Title = "File";
                requestData.Properties.Description = "pippo"; // The description is optional.
                requestData.Properties.ContentSourceApplicationLink = ApplicationLink;
                requestData.SetStorageItems(this.selectedStorageItems);
                succeeded = true;
            }
            else
            {
                request.FailWithDisplayText("Select the files you would like to share and try again.");
            }
            return succeeded;
        }

    }
}
