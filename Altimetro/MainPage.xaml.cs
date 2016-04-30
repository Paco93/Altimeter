using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Sensors;
// Il modello di elemento per la pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x410


using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.Storage;

namespace Altimetro
{

    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Barometer barom;
        StorageFolder localFolder = null;
        StorageFile file = null;
        public static MainPage Current;
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnMainPageLoaded;
            Current = this;
        }

       

        async void OnMainPageLoaded(object sender, RoutedEventArgs args)
        { 
            barom = Barometer.GetDefault();
            localFolder = ApplicationData.Current.LocalFolder; 
              //KnownFolders.GetFolderForUserAsync(null /* current user */, KnownFolderId.DocumentsLibrary);
            string filename = "Altimeter_" + DateTime.Today.Year.ToString() +"_"+ DateTime.Today.Month.ToString() +"_"+ DateTime.Today.Day.ToString() +".dat";
            try
            {

                file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);
            }
            catch(Exception e)
            {
                try
                {
                    file = (StorageFile)await localFolder.TryGetItemAsync(filename);
                }
                catch(Exception x) { }
            }

            if (barom == null)
            {
//                ErrorMsg.Visibility = Visibility.Visible;
                await new Windows.UI.Popups.MessageDialog("Barometer is not available").ShowAsync();

                return;
            }
            uint rep = barom.MinimumReportInterval;
            if (rep < 1000)
                barom.ReportInterval = 1000;
            else
                barom.ReportInterval = rep;
            BarometerReading red = barom.GetCurrentReading();
            CurrPression.Text = red.StationPressureInHectopascals.ToString("F3");
            
            barom.ReadingChanged += OnReadingChanged;

            ((ScatterSeries) LineChart.Series[0]).ItemsSource = App.items;    
        }

        async void OnReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
        {
            double curPress = args.Reading.StationPressureInHectopascals;
            double rat = App.CalibPressure / curPress;
            const double R = 8.31432;
            const double g = 9.80665;
            const double M = 0.0289644;
//          double r = Math.Log( rat);
//          double alt1 = R*  temp/ M * r / g;
            double alt = (Math.Pow(rat, R * App.Lb / (g * M)) - 1) * App.temp / App.Lb + App.calibAlt;
            string altit = alt.ToString("F1");
            string curPressString = curPress.ToString("F3");
            string userContent = DateTime.Today.TimeOfDay.ToString() + "\t" + curPressString + "\t" + altit+ "\n";
           

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CurrPression.Text = curPressString;
                Altit.Text = altit;
                ((App)Application.Current).chartCounter++;
                if ((((App)Application.Current).chartCounter % App.chartDecimation ) == 0)
                {
                    if (App.items.Count > 100)
                    {
                        App.items.RemoveAt(0);
                    }
                    App.items.Add(new ScatterValueItem() { Name = ((App)Application.Current).chartCounter, Value = alt });
                    if (file != null)
                    {
                        using (Windows.Storage.Streams.DataWriter dataWriter = new Windows.Storage.Streams.DataWriter())
                        {
                                dataWriter.WriteString(userContent);                        
                        }
                    }
                }
        
            });
        }

        

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Config), null);
        }

        private void GoToHelp_Click(object sender, RoutedEventArgs e)
        {

            this.Frame.Navigate(typeof(Help), null);
        }

        private void GoTo_Files(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(FilePage), null);
        }
    }
    public class ScatterValueItem
    {
        private double _name;

        public double Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private double _value;

        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }
        public ScatterValueItem()
        {
        }
    }

}
