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

// Il modello di elemento Pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace Altimetro
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class Config : Page
    {
        Barometer barom;
        static internal bool calibFlag = false;
        int calibCycles;
        Brush old;
        double tmpCalibPress;
        public Config()
        {
            this.InitializeComponent();
            barom = Barometer.GetDefault();
           


            double tt = App.temp - 273.15;
            Temp.Text = tt.ToString();

            double t = App.Lb * 1000;
            LapseRate.Text = t.ToString();
            CalibAlt.Text = App.calibAlt.ToString();
            CalibrationValue.Text = App.CalibPressure.ToString("F3");
            SamplingValue.Text = App.chartDecimation.ToString();
        }

        private void Temp_LostFocus(object sender, RoutedEventArgs e)
        {
            string t = Temp.Text;
            if (t == "")
                return;
            double v = Convert.ToDouble(t);
            if (v > -50 && v < 50)
                App.temp = Convert.ToDouble(t) + 273.15;
        }

        private void CalibAlt_LostFocus(object sender, RoutedEventArgs e)
        {
            string t = CalibAlt.Text;
            if (t == "")
                return;
            double v = Convert.ToDouble(t);
            if (v > -1000 && v<30000)
                App.calibAlt = v;
        }

        private void CalibrationValue_LostFocus(object sender, RoutedEventArgs e)
        {
            string t = CalibrationValue.Text;
            if (t == "")
                return;
            double v = Convert.ToDouble(t);
            if(v>100)    
                App.CalibPressure = v;
        }

        private void LapseRate_LostFocus(object sender, RoutedEventArgs e)
        {
            string t = LapseRate.Text;
            if (t == "")
                return;
            double v =Convert.ToDouble(t);
            if(v>2 && v<15)
                App.Lb = v/1000;
        }

        
        private void SamplingValue_LostFocus(object sender, RoutedEventArgs e)
        {
            string t = SamplingValue.Text;
            if (t == "")
                return;
            int cd = Convert.ToInt32(t);
            if (cd >= 1 & cd <= 3600)
                App.chartDecimation = cd;
            else if (cd < 1)
                App.chartDecimation = 1;
            else if (cd > 3600)
                App.chartDecimation = 3600;
        }

        private void AppBar_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        async private void OnCalibrate(object sender, RoutedEventArgs e)
        {
            if (calibFlag)
                return;
            calibFlag = true;
            barom.ReportInterval = 100;

            BarometerReading calibration;
            calibration = barom.GetCurrentReading();

            App.CalibPressure = calibration.StationPressureInHectopascals;

            tmpCalibPress = 0;
            calibCycles = 0;
            barom.ReadingChanged += OnCalibration;
            old = Calibrate.Background;
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Calibrate.Background = new SolidColorBrush(Colors.Red);
            });
        }

        async void OnCalibration(Barometer sender, BarometerReadingChangedEventArgs args)
        {
            //CurrPression.Text = args.Reading.StationPressureInHectopascals.ToString();
            //double r = Math.Log(args.Reading.StationPressureInHectopascals / calibration.StationPressureInHectopascals);
            //double temperat = 290;
            //double alt = 8.3144598 * temperat / 0.02897 * r / 9.81;
            tmpCalibPress += args.Reading.StationPressureInHectopascals;
            calibCycles++;
            if (calibCycles == 50)
            {
                barom.ReadingChanged -= OnCalibration;
                tmpCalibPress /= 50;
                calibFlag = false;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Calibrate.Background = old;
                    App.CalibPressure = tmpCalibPress;
                    CalibrationValue.Text = tmpCalibPress.ToString();
                    barom.ReportInterval = 1000;
                    App.items.Clear();
                    App.chartDecimation = 5;
                    ((App)Application.Current).chartCounter = 0;
                });
            }
        }

        private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
                e.Handled = true;
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
                this.Frame.Navigate(typeof(MainPage), null);
        }
        
        protected override void  OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            App.Lb = 0.0065;
            App.temp = 273.15+15;
            App.calibAlt = 0;
            App.chartDecimation = 5;
            App.CalibPressure = 1013.25;

            double tt = App.temp - 273.15;
            Temp.Text = tt.ToString();

            double t = App.Lb * 1000;
            LapseRate.Text = t.ToString();
            CalibAlt.Text = App.calibAlt.ToString();
            CalibrationValue.Text = App.CalibPressure.ToString("F3");
            SamplingValue.Text = App.chartDecimation.ToString( );
        }

        private void Temp_LostFocus_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
