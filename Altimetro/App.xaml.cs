using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.Storage;


namespace Altimetro
{
    /// <summary>
    /// Fornisci un comportamento specifico dell'applicazione in supplemento alla classe Application predefinita.
    /// </summary>
    sealed partial class App : Application
    {
        static internal StorageFolder localFolder = null;
        static internal StorageFile file = null;
        static internal volatile bool save2File=false;
         
        static internal double CalibPressure;  //Pressure at Calibration Location (initially at sea level 

        static internal double temp;  //temperature at the calibration point 
        static internal double calibAlt; //Amtitude of calibaration location 
        static internal double Lb; //Lapse rate  °C/m  
        static internal System.Collections.ObjectModel.ObservableCollection<ScatterValueItem> items;

        static internal int chartDecimation = 5;
        internal uint chartCounter;

//        static internal List<string> fileBuffer;
        static internal System.Text.StringBuilder fileBuffer;
        /// <summary>
        /// Inizializza l'oggetto Application singleton. Si tratta della prima riga del codice creato
        /// creato e, come tale, corrisponde all'equivalente logico di main() o WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            App.calibAlt = 0;
            App.temp = 288.15;//Standard temperature
            App.Lb = 0.0065; //in °C/m   

            items = new System.Collections.ObjectModel.ObservableCollection<ScatterValueItem>();
   //         items.Add(new ScatterValueItem() { Name = 0, Value = 0 });
            chartCounter = 0;
            CalibPressure = 1013.25; //Standard atmosphere @ sea level;
            localFolder = ApplicationData.Current.LocalFolder;
            fileBuffer = new System.Text.StringBuilder();
        }

        /// <summary>
        /// Richiamato quando l'applicazione viene avviata normalmente dall'utente. All'avvio dell'applicazione
        /// verranno usati altri punti di ingresso per aprire un file specifico.
        /// </summary>
        /// <param name="e">Dettagli sulla richiesta e sul processo di avvio.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;

            // Non ripetere l'inizializzazione dell'applicazione se la finestra già dispone di contenuto,
            // assicurarsi solo che la finestra sia attiva
            if (rootFrame == null)
            {
                // Creare un frame che agisca da contesto di navigazione e passare alla prima pagina
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                    object value = Convert.ToDouble(localSettings.Values["CalibrationPressure"]);
                    if (value == null)
                        CalibPressure = 1013.25;                        
                    else
                        CalibPressure = Convert.ToDouble(value);

                    value = Convert.ToDouble(localSettings.Values["CalibrationAltitude"]);
                    if (value == null)
                        calibAlt = 0;
                    else
                        calibAlt = Convert.ToDouble(value);


                     value = Convert.ToDouble(localSettings.Values["CalibrationTemperature"]);
                    if (value == null)
                        temp = 15;
                    else
                        temp = Convert.ToDouble(value);

                    value = Convert.ToDouble(localSettings.Values["CalibrationLapse"]);
                    if (value == null)
                        Lb = 0.0065;
                    else
                        Lb = Convert.ToDouble(value);

                    value = Convert.ToInt32(localSettings.Values["SamplingTime"]);
                    if (value == null)
                        chartDecimation = 5;
                    else
                        chartDecimation = Convert.ToInt32(value);

                    value = Convert.ToBoolean(localSettings.Values["Save2File"]);
                    if (value == null)
                        save2File = false;
                    else
                        save2File = Convert.ToBoolean(value);
                }

                string filename = "Altimeter_" + DateTime.Today.Year.ToString() + "_" + DateTime.Today.Month.ToString() + "_" + DateTime.Today.Day.ToString() + ".csv";
                try
                {

                    App.file = await App.localFolder.CreateFileAsync(filename, CreationCollisionOption.FailIfExists);
                }
                catch (Exception exc)
                {
                    try
                    {
                        App.file = (StorageFile)await App.localFolder.TryGetItemAsync(filename);
                    }
                    catch (Exception x) { }
                }

                // Posizionare il frame nella finestra corrente
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Quando lo stack di esplorazione non viene ripristinato, passare alla prima pagina
                    // e configurare la nuova pagina passando le informazioni richieste come parametro
                    // parametro
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Assicurarsi che la finestra corrente sia attiva
                Window.Current.Activate();
            }
        }

        public string FolderName
        {
            get { return Package.Current.Id.Name; }
        }

        /// <summary>
        /// Chiamato quando la navigazione a una determinata pagina ha esito negativo
        /// </summary>
        /// <param name="sender">Frame la cui navigazione non è riuscita</param>
        /// <param name="e">Dettagli sull'errore di navigazione.</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Richiamato quando l'esecuzione dell'applicazione viene sospesa. Lo stato dell'applicazione viene salvato
        /// senza che sia noto se l'applicazione verrà terminata o ripresa con il contenuto
        /// della memoria ancora integro.
        /// </summary>
        /// <param name="sender">Origine della richiesta di sospensione.</param>
        /// <param name="e">Dettagli relativi alla richiesta di sospensione.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: salvare lo stato dell'applicazione e arrestare eventuali attività eseguite in background

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Create a simple setting

            localSettings.Values["CalibrationPressure"] = CalibPressure;
            localSettings.Values["CalibrationAltitude"] = calibAlt;
            localSettings.Values["CalibrationLapse"] = Lb;
            localSettings.Values["CalibrationTemperature"] = temp;
            localSettings.Values["SamplingTime"] = chartDecimation;
            localSettings.Values["Save2File"] = save2File;


            if (fileBuffer.Length > 0)
            {
                await FileIO.AppendTextAsync(App.file, App.fileBuffer.ToString());
                fileBuffer.Clear();
            }
            deferral.Complete();
        }

    }
}
