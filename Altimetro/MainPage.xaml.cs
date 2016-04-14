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
        BarometerReading calibration;
        double CalibPressure;
        bool calibFlag = false;
        int calibCycles;
        Brush old;
        static internal double temp;
        public MainPage()
        {
            this.InitializeComponent();
            barom = Barometer.GetDefault();
            uint rep = barom.MinimumReportInterval;
            if (rep < 1000)
                barom.ReportInterval = 1000;
            else
                barom.ReportInterval = rep;
            calibration = barom.GetCurrentReading();
            CalibrationValue.Text = calibration.StationPressureInHectopascals.ToString("F3");
            CalibPressure = calibration.StationPressureInHectopascals;
            BarometerReading red = barom.GetCurrentReading();
            CurrPression.Text = red.StationPressureInHectopascals.ToString("F3");
            barom.ReadingChanged += OnReadingChanged;
            temp = 290;
        }

        async void OnReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
        {
           
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                CurrPression.Text = args.Reading.StationPressureInHectopascals.ToString("F3");
                if (calibFlag)
                    return;
                double r = Math.Log( CalibPressure / args.Reading.StationPressureInHectopascals );
               
                double alt = 8.3144598 *  temp/ 0.02897 * r / 9.81;
                CalibrationValue.Text = CalibPressure.ToString("F3");
                Altit.Text = alt.ToString("F1");

            });
        }

        async private void OnCalibrate(object sender, RoutedEventArgs e)
        {
            if (calibFlag)
                return;
            calibFlag = true;
            CalibPressure = 0;
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
            CalibPressure += args.Reading.StationPressureInHectopascals;
            calibCycles++;
            if (calibCycles == 10)
            {
                barom.ReadingChanged -= OnCalibration;
                CalibPressure /= 10;
                calibFlag = false;
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Calibrate.Background = old;
                });
            }
        }

        private void GoToConfigButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Config), null);
        }
    }
}
