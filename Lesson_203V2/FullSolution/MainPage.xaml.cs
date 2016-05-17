using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Lesson_203V2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //A class which wraps the barometric sensor
        BME280Sensor BME280;

        //Create a constant for pressure at sea level. 
        //This is based on your local sea level pressure (Unit: Hectopascal)
        const float seaLevelPressure = 1022.00f;

        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Altitude { get; set; }
        public float Humidity { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        // This method will be called by the application framework when the page is first loaded
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            Debug.WriteLine("MainPage::OnNavigatedTo");

            //MakePinWebAPICall();
            
            Temperature = 0;
            Pressure = 0;
            Altitude = 0;
            Humidity = 0;

            try
            {
                BME280 = new BME280SpiSensor();
                await BME280.Initialize();

                for (int x=0; x <= 10; x++)
                {
                    await getTemperature();

                    Debug.WriteLine("Temperature: " + Temperature.ToString() + " deg C");
                    Debug.WriteLine("Humidity: " + Humidity.ToString() + " %");
                    Debug.WriteLine("Pressure: " + Pressure.ToString() + " Pa");
                    Debug.WriteLine("Altitude: " + Altitude.ToString() + " m");
                }


            }
            catch(Exception ex)
            {
                Debug.WriteLine("OnNavigateTo: " + ex.Message + " Stack Trace: " + ex.StackTrace);
            }
        }

        public async Task getTemperature()
        {
            try
            {
                if (BME280 != null)
                {
                    Temperature = await BME280.ReadTemperature();
                    Pressure = await BME280.ReadPreasure();
                    Altitude = await BME280.ReadAltitude(seaLevelPressure);
                    Humidity = await BME280.ReadHumidity();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine("getTemperature: " + ex.Message + " Stack Trace: " + ex.StackTrace);
            }
        }

        /// <summary>
        // This method will put your pin on the world map of makers using this lesson.
        // This uses imprecise location (for example, a location derived from your IP 
        // address with less precision such as at a city or postal code level). 
        // No personal information is stored.  It simply
        // collects the total count and other aggregate information.
        // http://www.microsoft.com/en-us/privacystatement/default.aspx
        // Comment out the line below to opt-out
        /// </summary>
        public void MakePinWebAPICall()
        {
            try
            {
                HttpClient client = new HttpClient();

                // Comment this line to opt out of the pin map.
                client.GetStringAsync("http://adafruitsample.azurewebsites.net/api?Lesson=203");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Web call failed: " + e.Message);
            }
        }

        //private void ReadMeasurements_Click(object sender, RoutedEventArgs e)
        //{
        //    getTemperature();
        //    this.Readings.Text = "Temperature: " + Temperature.ToString() + " deg C\n" + 
        //                         "Humidity: " + Humidity.ToString() + " %\n" +
        //                         "Pressure: " + Pressure.ToString() + " Pa\n" +
        //                         "Altitude: " + Altitude.ToString() + " m\n";
        //}

        //private async void rbI2C_Checked(object sender, RoutedEventArgs e)
        //{
        //    BME280 = new BME280I2cSensor();

        //    try
        //    {
        //        this.Readings.Text = "Initializing BME280 on I2C bus!";
        //        await BME280.Initialize();
        //    }
        //    catch(Exception ex)
        //    {
        //        this.Readings.Text = ex.Message;
        //    }

        //}

        //private async void rbSPI0_Checked(object sender, RoutedEventArgs e)
        //{
        //    BME280 = new BME280SpiSensor();

        //    try
        //    {
        //        this.Readings.Text = "Initializing BME280 on SPI0 bus!";
        //        await BME280.Initialize();
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Readings.Text = ex.Message;
        //    }
        //}

        //private async void rbSPI1_Checked(object sender, RoutedEventArgs e)
        //{
        //    BME280 = new BME280SpiSensor(SPI_Controllers.SPI1);

        //    try
        //    {
        //        this.Readings.Text = "Initializing BME280 on SPI1 bus!";
        //        await BME280.Initialize();
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Readings.Text = ex.Message;
        //    }
        //}

    }
}
