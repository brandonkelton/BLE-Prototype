using MvvmCross;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using WirelessPrototype.Models;

namespace WirelessPrototype.Services
{
    public class BLEService : IBLEService
    {
        public event EventHandler<DeviceAddedEventArgs> DeviceDetected;

        public async Task ScanForDevices()
        {
            // var ble = Mvx.IoCProvider.GetSingleton<IBluetoothLE>();
            var adapter = Mvx.IoCProvider.GetSingleton<IAdapter>();

            adapter.DeviceDiscovered += (s, a) =>
            {
                DeviceAddedEventArgs args = new DeviceAddedEventArgs()
                {
                    Device = new DeviceModel() { Id = a.Device.Id, Name = a.Device.Name }
                };

                OnDeviceDetected(args);
            };
                

            await adapter.StartScanningForDevicesAsync();
        }

        private void OnDeviceDetected(DeviceAddedEventArgs e)
        {
            DeviceDetected?.Invoke(this, e);
        }
    }

    public class DeviceAddedEventArgs : EventArgs
    {
        public DeviceModel Device { get; set; }
    }
}
