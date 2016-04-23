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
namespace Altimetro
{

    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Barometer barom;

        public MainPage()
        {
            this.InitializeComponent();
            barom = Barometer.GetDefault();
            if (barom == null)
            {
                ErrorMsg.Visibility = Visibility.Visible;
                //Flyout error = new Flyout();
                //error.Content = ErrorMsg;
                //error.ShowAt(ErrorMsg);
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
           
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                double curPress = args.Reading.StationPressureInHectopascals;
                CurrPression.Text = curPress.ToString("F3");
                double rat = App.CalibPressure / curPress;
 //               double r = Math.Log( rat);
                const double R = 8.31432;
                const double g = 9.80665;
                const double M = 0.0289644;
 //               double alt1 = R*  temp/ M * r / g;

                //Alternative formula
                
                double  alt =  (Math.Pow(rat ,R * App.Lb / (g * M) ) - 1) * App.temp / App.Lb + App.calibAlt;
                Altit.Text = alt.ToString("F1");
                ((App)Application.Current).chartCounter++;
                if ((((App)Application.Current).chartCounter % ((App)Application.Current).chartDecimation ) == 0)
                {
                    if (App.items.Count > 100)
                    {
                        ((App)Application.Current).chartDecimation = 30;
                        App.items.RemoveAt(0);
                    }
                    App.items.Add(new ScatterValueItem() { Name = ((App)Application.Current).chartCounter, Value = alt });
                }
                //    myGrapg.Value = alt;
        
            });
        }

        

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Config), null);
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
