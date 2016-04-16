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

namespace Altimetro
{

    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Barometer barom;
      
        static internal double CalibPressure;
      
        static internal double temp;
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
            CalibPressure = 1013.25; //Standard atmosphere @ sea level;
            barom.ReadingChanged += OnReadingChanged;
            temp = 288.15;//Standard temperature
        }

        async void OnReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
        {
           
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CurrPression.Text = args.Reading.StationPressureInHectopascals.ToString("F3");
                double rat = CalibPressure / args.Reading.StationPressureInHectopascals;
                double r = Math.Log( rat);
                const double R = 8.31432;
                const double g = 9.80665;
                const double M = 0.0289644;
             //   double alt = R*  temp/ M * r / g;

                //Alternative formula
                const double Lb = 0.0065; //in °C/m   
                double  alt = (Math.Exp((R * Lb / (g * M)) * r) - 1) * temp / Lb;
                Altit.Text = alt.ToString("F1");

            });
        }

        

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Config), null);
        }
    }
}
