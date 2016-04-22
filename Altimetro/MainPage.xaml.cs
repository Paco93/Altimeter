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
      
        static internal double CalibPressure;  //Pressure at Calibration Location (initially at sea level 
      
        static internal double temp;  //temperature at the calibration point 
        static internal double calibAlt; //Amtitude of calibaration location 
        static internal double Lb ; //Lapse rate  °C/m   
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
            calibAlt = 0;
            barom.ReadingChanged += OnReadingChanged;
            temp = 288.15;//Standard temperature
            Lb = 0.0065; //in °C/m   
        }

        async void OnReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
        {
           
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                double curPress = args.Reading.StationPressureInHectopascals;
                CurrPression.Text = curPress.ToString("F3");
                double rat = CalibPressure / curPress;
 //               double r = Math.Log( rat);
                const double R = 8.31432;
                const double g = 9.80665;
                const double M = 0.0289644;
 //               double alt1 = R*  temp/ M * r / g;

                //Alternative formula
                
                double  alt =  (Math.Pow(rat ,R * Lb / (g * M) ) - 1) * temp / Lb + calibAlt;
                Altit.Text = alt.ToString("F1");
            });
        }

        

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Config), null);
        }
    }
}
