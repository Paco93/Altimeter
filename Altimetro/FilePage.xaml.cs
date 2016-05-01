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

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using Windows.Storage.FileProperties;
using Windows.Storage.Provider;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Altimetro
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FilePage : Page
    {
        public List<StorageFile> FilesList { get; private set; }
        public List<string> FilesListS { get; private set; }
        private List<StorageFile> selectedStorageItems;
        //IReadOnlyList<StorageFile> selectedStorageItems;
        private Windows.ApplicationModel.DataTransfer.DataTransferManager dataTransferManager;
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
            this.dataTransferManager = Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView();
            this.dataTransferManager.DataRequested += new TypedEventHandler<Windows.ApplicationModel.DataTransfer.DataTransferManager, Windows.ApplicationModel.DataTransfer.DataRequestedEventArgs>(this.OnDataRequested);
    
            GetFilesList();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister the current page as a share source.
            this.dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            // Call the scenario specific function to populate the datapackage with the data to be shared.
            if (GetShareContent(e.Request))
            {
                // Out of the datapackage properties, the title is required. If the scenario completed successfully, we need
                // to make sure the title is valid since the sample scenario gets the title from the user.
                if (String.IsNullOrEmpty(e.Request.Data.Properties.Title))
                {
                    e.Request.FailWithDisplayText("Error");
                }
            }
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
            selectedIndex = lv.SelectedIndex;
            selectedStorageItems = new List<StorageFile>();
            if (selectedIndex > -1 && FilesListS.Count > 0)
            {
                selectedStorageItems.Add((StorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync(FilesListS.ElementAt(selectedIndex)));
            }
        }

    
        bool GetShareContent(Windows.ApplicationModel.DataTransfer.DataRequest request)
        {
            bool succeeded = false;
            if (selectedStorageItems != null && selectedStorageItems.Count > 0)
            {
                Windows.ApplicationModel.DataTransfer.DataPackage requestData = request.Data;
                requestData.Properties.Title = "File";
                requestData.Properties.Description = "pippo"; // The description is optional.
               // requestData.Properties.ContentSourceApplicationLink = ApplicationLink;
                requestData.SetStorageItems(this.selectedStorageItems);
                succeeded = true;
            }
            else
            {
                request.FailWithDisplayText("Select the files you would like to share and try again.");
            }
            return succeeded;
        }

        void ShowUIButton_Click(object sender, RoutedEventArgs e)
        { 
            // If the user clicks the share button, invoke the share flow programatically.
            DataTransferManager.ShowShareUI();
        }

        Uri ApplicationLink
        {
            get
            {
                return GetApplicationLink(GetType().Name);
            }
        }

        public static Uri GetApplicationLink(string sharePageName)
        {
            return new Uri("ms-sdk-sharesourcecs:navigate?page=" + sharePageName);
        }

        private async void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = null;
            if (selectedStorageItems!=null && selectedStorageItems.Count > 0)
               file = selectedStorageItems.ElementAt(0);
            if (file != null)
            {
                try
                {
                    string fileContent = await FileIO.ReadTextAsync(file);
                    string fileView = fileContent + App.fileBuffer;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        FileContent.Text =fileView ;
                    });


                }
                catch (FileNotFoundException)
                {
                }
            }
            else
            {
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = null;
            if (selectedStorageItems != null && selectedStorageItems.Count > 0)
                file = selectedStorageItems.ElementAt(0);
            if (file != null)
            {
                if (file.Path == App.file.Path)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        bool tmp = App.save2File;
                        App.save2File = false;
                        await FileIO.WriteTextAsync(App.file, ""); //actually empty the file
                        FileContent.Text = "";
                        App.fileBuffer.Clear();
                        App.save2File = tmp;
                    });
                    return;
                }
                await file.DeleteAsync();
                GetFilesList();
            }
        }

        private async  void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker();
            {

                savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                savePicker.SuggestedFileName = "New Document";
                savePicker.FileTypeChoices.Add("CSV Files", new List<string>() { ".csv" });

                StorageFile savefile = await savePicker.PickSaveFileAsync();
                if (savefile != null)
                {
                    StorageFile file = selectedStorageItems.ElementAt(0);
                    if (file != null)
                    {
                        string fileContent = await FileIO.ReadTextAsync(file);


                        // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                        CachedFileManager.DeferUpdates(savefile);
                        // write to file
                        await FileIO.WriteTextAsync(savefile, fileContent);
                        // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                        // Completing updates may require Windows to ask for user input.
                        FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(savefile);
                        //if (status == FileUpdateStatus.Complete)
                        //{
                        //    OutputTextBlock.Text = "File " + file.Name + " was saved.";
                        //}
                        //else
                        //{
                        //    OutputTextBlock.Text = "File " + file.Name + " couldn't be saved.";
                        //}
                    }
                }

        };

        }
    }
}
