using DokanNet;
using DokanNet.Logging;
using Serilog;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ADBUSBDrive
{
    class Program
    {
        public static DeviceMonitor Monitor;
        public static List<VirtualDrive> Devices = new List<VirtualDrive>();
        static void Main(string[] args)
        {
            //at startup, initialize logger
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true).CreateLogger();
            //after initializing the logger, test the log by saying "Application Started"
            Log.Information("Application Started");
            //then, log the version of Dokan
            Log.Information($"Using driver version {Dokan.DriverVersion} and Dokan version {Dokan.Version}");


            //then initialize the devices monitor
            Monitor = new DeviceMonitor(new AdbSocket(new IPEndPoint(IPAddress.Loopback, AdbClient.AdbServerPort)));
            Monitor.DeviceConnected += Monitor_DeviceConnected;
            Monitor.DeviceDisconnected += Monitor_DeviceDisconnected;
            Monitor.DeviceChanged += Monitor_DeviceChanged;
            Monitor.Start();
            Application.Run();
        }

        private static void Monitor_DeviceConnected(object sender, DeviceDataEventArgs e)
        {
            Log.Information("Device connected", e.Device);
            char letter = DriveHelpers.GetNextAvailableDriveLetter();
            VirtualDrive drive = new VirtualDrive(e.Device, Monitor.Socket, letter);
            Devices.Add(drive);
            new Thread(new ThreadStart(() =>
            {
                drive.Mount(letter + ":\\", /*DokanOptions.DebugMode | DokanOptions.StderrOutput | */DokanOptions.RemovableDrive, new NullLogger());
            })).Start();
        }

        private static void Monitor_DeviceDisconnected(object sender, DeviceDataEventArgs e)
        {
            Log.Information("Device disconnected", e.Device);
            List<VirtualDrive> drivesToRemove = new List<VirtualDrive>();
            foreach(VirtualDrive d in Devices)
            {
                if (e.Device.Serial.Equals(d.AndroidDevice.Serial))
                {
                    if (Dokan.Unmount(d.DriveLetter))
                    {
                        Log.Information("Successfully unmounted drive " + d.DriveLetter);
                    }
                    else
                    {
                        Log.Warning("Error while unmounting " + d.DriveLetter);
                    }
                    drivesToRemove.Add(d);
                }
            }
            foreach(VirtualDrive d in drivesToRemove)
            {
                Devices.Remove(d);
            }
        }

        private static void Monitor_DeviceChanged(object sender, DeviceDataEventArgs e)
        {
            Log.Information("Device changed", e.Device);
        }
    }
}
