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
using System.Globalization;

namespace Altimetro
{

    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Barometer barom;
 
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
        
            if (barom == null)
            {
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
            string altit = alt.ToString("F1", CultureInfo.InvariantCulture);
            string curPressString = curPress.ToString("F3", CultureInfo.InvariantCulture);
         
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CurrPression.Text = curPressString;
                Altit.Text = altit;

            });
            if (App.save2File == false)
            {
                ((App)Application.Current).chartCounter++;
                if ((((App)Application.Current).chartCounter % App.chartDecimation) == 0)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (App.items.Count > 100)
                        {
                            App.items.RemoveAt(0);
                        }
                        App.items.Add(new ScatterValueItem() { Name = ((App)Application.Current).chartCounter, Value = alt });

                    });

                }
            }
            else if  (App.file != null )
            {
                string userContent = DateTime.Now.TimeOfDay.TotalSeconds.ToString("F1", CultureInfo.InvariantCulture) + ";" + curPressString + ";" + altit + "\n";
                App.fileBuffer.Append(userContent);
                if (App.fileBuffer.Length > 10000)
                {
                    await FileIO.AppendTextAsync(App.file, App.fileBuffer.ToString());
                    App.fileBuffer.Clear();
                }

                //using (var randomAccessStream = await App.file.OpenAsync(FileAccessMode.ReadWrite))
                //{
                //    using (var outputStream = randomAccessStream.GetOutputStreamAt(0))
                //    {
                //        using (StreamWriter streamWriter =
                //           new StreamWriter(outputStream.AsStreamForWrite()))
                //        {
                //                await streamWriter.WriteLineAsync(userContent);                
                //        }
                //    }
                //}
            }

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
