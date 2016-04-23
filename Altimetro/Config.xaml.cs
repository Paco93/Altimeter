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
            Temp.TextChanged += Temp_TextChanged;
            CalibAlt.TextChanged += CalibAlt_TextChanged;
            CalibrationValue.TextChanged += CalibPressure_TextChanged;
            LapseRate.TextChanged += LapseRate_TextChanged;
            CalibrationValue.Text = App.CalibPressure.ToString("F3");
        }

        private void Temp_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = Temp.Text;
            App.temp = Convert.ToDouble(t) + 273.15;
        }

        private void CalibAlt_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = CalibAlt.Text;
            App.calibAlt = Convert.ToDouble(t);
        }

        private void CalibPressure_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = CalibrationValue.Text;
            App.CalibPressure = Convert.ToDouble(t);
        }

        private void LapseRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            string t = LapseRate.Text;
            App.Lb = Convert.ToDouble(t)/1000;

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
                    ((App)Application.Current).chartDecimation = 5;
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
    }
}
