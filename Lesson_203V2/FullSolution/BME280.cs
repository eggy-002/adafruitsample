using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Devices.Spi;

namespace Lesson_203V2
{
    public class BME280_CalibrationData
    {
        //BME280 Registers
        public UInt16 dig_T1 { get; set; }
        public Int16 dig_T2 { get; set; }
        public Int16 dig_T3 { get; set; }

        public UInt16 dig_P1 { get; set; }
        public Int16 dig_P2 { get; set; }
        public Int16 dig_P3 { get; set; }
        public Int16 dig_P4 { get; set; }
        public Int16 dig_P5 { get; set; }
        public Int16 dig_P6 { get; set; }
        public Int16 dig_P7 { get; set; }
        public Int16 dig_P8 { get; set; }
        public Int16 dig_P9 { get; set; }

        public byte dig_H1 { get; set; }
        public Int16 dig_H2 { get; set; }
        public byte dig_H3 { get; set; }
        public Int16 dig_H4 { get; set; }
        public Int16 dig_H5 { get; set; }
        public SByte dig_H6 { get; set; }

    }

    //String for the friendly name of the SPI bus
    public enum SPI_Controllers { SPI0 = 0, SPI1 }

    public abstract class BME280Sensor
    {
        //The BME280 register addresses according the the datasheet: 
        //http://www.adafruit.com/datasheets/BST-BME280-DS001-11.pdf
        protected const byte BME280_I2C_Address = 0x77;
        protected const byte BME280_Signature = 0x60;

        //t_fine carries fine temperature as global value
        Int32 t_fine = Int32.MinValue;

        protected enum eRegisters : byte
        {
            BME280_REGISTER_DIG_T1 = 0x88,
            BME280_REGISTER_DIG_T2 = 0x8A,
            BME280_REGISTER_DIG_T3 = 0x8C,

            BME280_REGISTER_DIG_P1 = 0x8E,
            BME280_REGISTER_DIG_P2 = 0x90,
            BME280_REGISTER_DIG_P3 = 0x92,
            BME280_REGISTER_DIG_P4 = 0x94,
            BME280_REGISTER_DIG_P5 = 0x96,
            BME280_REGISTER_DIG_P6 = 0x98,
            BME280_REGISTER_DIG_P7 = 0x9A,
            BME280_REGISTER_DIG_P8 = 0x9C,
            BME280_REGISTER_DIG_P9 = 0x9E,

            BME280_REGISTER_DIG_H1 = 0xA1,
            BME280_REGISTER_DIG_H2 = 0xE1,
            BME280_REGISTER_DIG_H3 = 0xE3,
            BME280_REGISTER_DIG_H4 = 0xE4,
            BME280_REGISTER_DIG_H5 = 0xE5,
            BME280_REGISTER_DIG_H6 = 0xE7,

            BME280_REGISTER_CHIPID = 0xD0,
            BME280_REGISTER_VERSION = 0xD1,
            BME280_REGISTER_SOFTRESET = 0xE0,

            BME280_REGISTER_CAL26 = 0xE1,  // R calibration stored in 0xE1-0xF0

            BME280_REGISTER_CONTROLHUMID = 0xF2,
            BME280_REGISTER_CONTROL = 0xF4,
            BME280_REGISTER_CONFIG = 0xF5,

            BME280_REGISTER_PRESSUREDATA_MSB = 0xF7,
            BME280_REGISTER_PRESSUREDATA_LSB = 0xF8,
            BME280_REGISTER_PRESSUREDATA_XLSB = 0xF9, // bits <7:4>

            BME280_REGISTER_TEMPDATA_MSB = 0xFA,
            BME280_REGISTER_TEMPDATA_LSB = 0xFB,
            BME280_REGISTER_TEMPDATA_XLSB = 0xFC, // bits <7:4>

            BME280_REGISTER_HUMIDDATA_MSB = 0xFD,
            BME280_REGISTER_HUMIDDATA_LSB = 0xFE,
        };

        //Create new calibration data for the sensor
        protected BME280_CalibrationData CalibrationData;

        //Variable to check if device is initialized
        protected bool init = false;

        //Method to initialize the BME280 sensor
        public abstract /*async*/ Task Initialize();

        protected abstract /*async*/ Task Begin();

        //Method to write to the humidity control register
        protected abstract /*async*/ Task WriteControlRegisterHumidity();

        //Method to write to the control register
        protected abstract /*async*/ Task WriteControlRegister();

        //Method to read a 16-bit value from a register and return it in little endian format
        protected abstract UInt16 ReadUInt16_LittleEndian(byte register);

        //Method to read an 8-bit value from a register
        protected abstract byte ReadByte(byte register);

        //Method to read the calibration data from the registers
        protected virtual async Task<BME280_CalibrationData> ReadCoefficeints()
        {
            // 16 bit calibration data is stored as Little Endian, the helper method will do the byte swap.
            CalibrationData = new BME280_CalibrationData();

            // Read temperature calibration data
            CalibrationData.dig_T1 = ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T1);
            CalibrationData.dig_T2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T2);
            CalibrationData.dig_T3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_T3);

            // Read presure calibration data
            CalibrationData.dig_P1 = ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P1);
            CalibrationData.dig_P2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P2);
            CalibrationData.dig_P3 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P3);
            CalibrationData.dig_P4 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P4);
            CalibrationData.dig_P5 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P5);
            CalibrationData.dig_P6 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P6);
            CalibrationData.dig_P7 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P7);
            CalibrationData.dig_P8 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P8);
            CalibrationData.dig_P9 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_P9);

            // Read humidity calibration data
            CalibrationData.dig_H1 = ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H1);
            CalibrationData.dig_H2 = (Int16)ReadUInt16_LittleEndian((byte)eRegisters.BME280_REGISTER_DIG_H2);
            CalibrationData.dig_H3 = ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H3);
            CalibrationData.dig_H4 = (Int16)((ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H4) << 4) | (ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H4 + 1) & 0xF));
            CalibrationData.dig_H5 = (Int16)((ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H5 + 1) << 4) | (ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H5) >> 4));
            CalibrationData.dig_H6 = (sbyte)ReadByte((byte)eRegisters.BME280_REGISTER_DIG_H6);

            await Task.Delay(1);
            return CalibrationData;
        }

        //Method to return the temperature in DegC. Resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
        protected virtual double BME280_compensate_T_double(Int32 adc_T)
        {
            double var1, var2, T;

            //The temperature is calculated using the compensation formula in the BME280 datasheet
            var1 = ((adc_T / 16384.0) - (CalibrationData.dig_T1 / 1024.0)) * CalibrationData.dig_T2;
            var2 = ((adc_T / 131072.0) - (CalibrationData.dig_T1 / 8192.0)) * CalibrationData.dig_T3;

            t_fine = (Int32)(var1 + var2);

            T = (var1 + var2) / 5120.0;
            return T;
        }

        //Method to returns the pressure in Pa, in Q24.8 format (24 integer bits and 8 fractional bits).
        //Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
        protected virtual Int64 BME280_compensate_P_Int64(Int32 adc_P)
        {
            Int64 var1, var2, p;

            //The pressure is calculated using the compensation formula in the BME280 datasheet
            var1 = t_fine - 128000;
            var2 = var1 * var1 * (Int64)CalibrationData.dig_P6;
            var2 = var2 + ((var1 * (Int64)CalibrationData.dig_P5) << 17);
            var2 = var2 + ((Int64)CalibrationData.dig_P4 << 35);
            var1 = ((var1 * var1 * (Int64)CalibrationData.dig_P3) >> 8) + ((var1 * (Int64)CalibrationData.dig_P2) << 12);
            var1 = (((((Int64)1 << 47) + var1)) * (Int64)CalibrationData.dig_P1) >> 33;
            if (var1 == 0)
            {
                Debug.WriteLine("BME280_compensate_P_Int64 Jump out to avoid / 0");
                return 0; //Avoid exception caused by division by zero
            }
            //Perform calibration operations as per datasheet: http://www.adafruit.com/datasheets/BST-BME280-DS001-11.pdf
            p = 1048576 - adc_P;
            p = (((p << 31) - var2) * 3125) / var1;
            var1 = ((Int64)CalibrationData.dig_P9 * (p >> 13) * (p >> 13)) >> 25;
            var2 = ((Int64)CalibrationData.dig_P8 * p) >> 19;
            p = ((p + var1 + var2) >> 8) + ((Int64)CalibrationData.dig_P7 << 4);
            return p;
        }

        // Returns humidity in %RH as unsigned 32 bit integer in Q22.10 format (22 integer and 10 fractional bits).
        // Output value of “47445” represents 47445/1024 = 46.333 %RH
        protected virtual UInt32 bme280_compensate_H_int32(Int32 adc_H)
        {
            Int32 v_x1_u32r;
            v_x1_u32r = (t_fine - ((Int32)76800));
            v_x1_u32r = (((((adc_H << 14) - (((Int32)CalibrationData.dig_H4) << 20) - (((Int32)CalibrationData.dig_H5) * v_x1_u32r)) +
                        ((Int32)16384)) >> 15) * (((((((v_x1_u32r * ((Int32)CalibrationData.dig_H6)) >> 10) * (((v_x1_u32r *
                        ((Int32)CalibrationData.dig_H3)) >> 11) + ((Int32)32768))) >> 10) + ((Int32)2097152)) *
                        ((Int32)CalibrationData.dig_H2) + 8192) >> 14));
            v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * ((Int32)CalibrationData.dig_H1)) >> 4));
            v_x1_u32r = (v_x1_u32r < 0 ? 0 : v_x1_u32r);
            v_x1_u32r = (v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r);
            return (UInt32)(v_x1_u32r >> 12);
        }

        public virtual async Task<float> ReadTemperature()
        {
            //Make sure the device is initialized
            if (!init) await Begin();

            //Read the MSB, LSB and bits 7:4 (XLSB) of the temperature from the BME280 registers
            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_TEMPDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the temperature in degC
            double temp = BME280_compensate_T_double(t);

            //Return the temperature as a float value
            return (float)temp;
        }

        public virtual async Task<float> ReadPreasure()
        {
            //Make sure the device is initialized
            if (!init) await Begin();

            //Read the temperature first to load the t_fine value for compensation
            if (t_fine == Int32.MinValue)
            {
                await ReadTemperature();
            }

            //Read the MSB, LSB and bits 7:4 (XLSB) of the pressure from the BME280 registers
            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_LSB);
            byte txlsb = ReadByte((byte)eRegisters.BME280_REGISTER_PRESSUREDATA_XLSB); // bits 7:4

            //Combine the values into a 32-bit integer
            Int32 t = (tmsb << 12) + (tlsb << 4) + (txlsb >> 4);

            //Convert the raw value to the pressure in Pa
            Int64 pres = BME280_compensate_P_Int64(t);

            //Return the pressure as a float value
            return ((float)pres) / 256;
        }

        public virtual async Task<float> ReadHumidity()
        {
            if (!init) await Begin();

            byte tmsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_MSB);
            byte tlsb = ReadByte((byte)eRegisters.BME280_REGISTER_HUMIDDATA_LSB);
            Int32 uncompensated = (tmsb << 8) + tlsb;
            UInt32 humidity = bme280_compensate_H_int32(uncompensated);

            return ((float)humidity) / 1000;
        }

        //Method to take the sea level pressure in Hectopascals(hPa) 
        //as a parameter and calculate the altitude using current pressure.
        public virtual async Task<float> ReadAltitude(float seaLevel)
        {
            //Make sure the device is initialized
            if (!init) await Begin();

            //Read the pressure first
            float pressure = await ReadPreasure();
            //Convert the pressure to Hectopascals(hPa)
            pressure /= 100;

            //Calculate and return the altitude using the international barometric formula
            return 44330.0f * (1.0f - (float)Math.Pow((pressure / seaLevel), 0.1903f));
        }
    }

    public class BME280I2cSensor : BME280Sensor
    {
        //String for the friendly name of the I2C bus 
        const string I2CControllerName = "I2C1";

        //Create an I2C device
        private I2cDevice bme280I2c = null;

        //Method to initialize the BME280 sensor
        public override async Task Initialize()
        {
            Debug.WriteLine("BME280I2C::Initialize");

            try
            {
                //Instantiate the I2CConnectionSettings using the device address of the BME280
                I2cConnectionSettings settings = new I2cConnectionSettings(BME280_I2C_Address);
                //Set the I2C bus speed of connection to fast mode
                settings.BusSpeed = I2cBusSpeed.FastMode;
                //Use the I2CBus device selector to create an advanced query syntax string
                string aqs = I2cDevice.GetDeviceSelector(I2CControllerName);
                //Use the Windows.Devices.Enumeration.DeviceInformation class to create a collection using the advanced query syntax string
                DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);
                //Instantiate the the BME280 I2C device using the device id of the I2CBus and the I2CConnectionSettings
                bme280I2c = await I2cDevice.FromIdAsync(dis[0].Id, settings);
                //Check if device was found
                if (bme280I2c == null)
                {
                    Debug.WriteLine("Device not found");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }

        }

        protected override async Task Begin()
        {
            Debug.WriteLine("BME280I2C::Begin");
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CHIPID };
            byte[] ReadBuffer = new byte[] { 0xFF };

            //Read the device signature
            bme280I2c.WriteRead(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BME280_I2C Signature: " + ReadBuffer[0].ToString());

            //Verify the device signature
            if (ReadBuffer[0] != BME280_Signature)
            {
                Debug.WriteLine("BME280_I2C::Begin Signature Mismatch.");
                return;
            }

            //Set the initialize variable to true
            init = true;

            //Read the coefficients table
            CalibrationData = await ReadCoefficeints();

            //Write control register
            await WriteControlRegister();

            //Write humidity control register
            await WriteControlRegisterHumidity();
        }

        //Method to write 0x03 to the humidity control register
        protected override async Task WriteControlRegisterHumidity()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROLHUMID, 0x03 };
            bme280I2c.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to write 0x3F to the control register
        protected override async Task WriteControlRegister()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROL, 0x3F };
            bme280I2c.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to read a 16-bit value from a register and return it in little endian format
        protected override UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280I2c.WriteRead(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);
            return value;
        }

        //Method to read an 8-bit value from a register
        protected override byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bme280I2c.WriteRead(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

    }

    public class BME280SpiSensor : BME280Sensor
    {
        private string spi_controller_name;
        private Int32 spi_chip_select_line;

        //Create an SPI device
        private  SpiDevice bme280spi = null;

        public BME280SpiSensor()
        {
            spi_controller_name = "SPI0";
            spi_chip_select_line = 0;
        }

        public BME280SpiSensor(SPI_Controllers spiController)
        {
            switch (spiController)
            {
                case SPI_Controllers.SPI1:
                    spi_controller_name = "SPI1";
                    spi_chip_select_line = 1;
                    break;
                default:
                    spi_controller_name = "SPI0";
                    spi_chip_select_line = 0;
                    break;
            }
        }

        //Method to initialize the BME280 sensor
        public override async Task Initialize()
        {
            Debug.WriteLine("BME280Spi::Initialize");

            try
            {
                var settings = new SpiConnectionSettings(spi_chip_select_line);
                settings.ClockFrequency = 4000000; // Max frequency of BME280 is 10000000
                settings.Mode = SpiMode.Mode0;
                settings.DataBitLength = 8;

                string spiAqs = SpiDevice.GetDeviceSelector(spi_controller_name);
                Debug.WriteLine("spiAqs" + spiAqs.ToString());

                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
                Debug.WriteLine(deviceInfo[0].Id.ToString());


                bme280spi = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);

                if (bme280spi == null)
                {
                    throw new Exception("Unable to initialize SPI device!");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e.Message + "\n" + e.StackTrace);
                throw;
            }

        }

        protected override async Task Begin()
        {
            Debug.WriteLine("BME280Spi::Begin");
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CHIPID };
            byte[] ReadBuffer = new byte[] { 0x00 };

            //Read the device signature
            bme280spi.TransferSequential(WriteBuffer, ReadBuffer);
            Debug.WriteLine("BME280 Signature: " + ReadBuffer[0].ToString());

            //Verify the device signature
            if (ReadBuffer[0] != BME280_Signature)
            {
                Debug.WriteLine("BME280::Begin Signature Mismatch.");
                bme280spi.Dispose();
                bme280spi = null;
                throw new Exception("BMESpi: Signature Mismatch!");
            }

            //Set the initialize variable to true
            init = true;

            //Read the coefficients table
            CalibrationData = await ReadCoefficeints();

            //Write control register
            await WriteControlRegister();

            //Write humidity control register
            await WriteControlRegisterHumidity();
        }

        //Method to write 0x03 to the humidity control register
        protected override async Task WriteControlRegisterHumidity()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROLHUMID, 0x03 };
            bme280spi.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to write 0x3F to the control register
        protected override async Task WriteControlRegister()
        {
            byte[] WriteBuffer = new byte[] { (byte)eRegisters.BME280_REGISTER_CONTROL, 0x3F };
            bme280spi.Write(WriteBuffer);
            await Task.Delay(1);
            return;
        }

        //Method to read a 16-bit value from a register and return it in little endian format
        protected override UInt16 ReadUInt16_LittleEndian(byte register)
        {
            UInt16 value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00, 0x00 };

            writeBuffer[0] = register;

            bme280spi.TransferSequential(writeBuffer, readBuffer);
            int h = readBuffer[1] << 8;
            int l = readBuffer[0];
            value = (UInt16)(h + l);
            return value;
        }

        //Method to read an 8-bit value from a register
        protected override byte ReadByte(byte register)
        {
            byte value = 0;
            byte[] writeBuffer = new byte[] { 0x00 };
            byte[] readBuffer = new byte[] { 0x00 };

            writeBuffer[0] = register;

            bme280spi.TransferSequential(writeBuffer, readBuffer);
            value = readBuffer[0];
            return value;
        }

    }

}
