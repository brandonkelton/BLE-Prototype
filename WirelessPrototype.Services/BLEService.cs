using MvvmCross;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WirelessPrototype.Models;

namespace WirelessPrototype.Services
{
    public class BLEService : IBLEService
    {
        public event EventHandler<DeviceAddedEventArgs> DeviceDetected;

        private IBluetoothLE _ble;
        private IAdapter _adapter;

        public BLEService()
        {
            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            SetupEventHandling();
        }

        private void SetupEventHandling()
        {
            _adapter.DeviceDiscovered += (s, a) =>
            {
                DeviceAddedEventArgs args = new DeviceAddedEventArgs()
                {
                    Device = new DeviceModel() { Id = a.Device.Id, Name = a.Device.Name }
                };

                OnDeviceDetected(args);
            };

            _adapter.DeviceAdvertised += (s, a) =>
            {
                DeviceAddedEventArgs args = new DeviceAddedEventArgs()
                {
                    Device = new DeviceModel() { Id = a.Device.Id, Name = a.Device.Name }
                };

                OnDeviceDetected(args);
            };

            _ble.StateChanged += (s, e) => OnStateChanged(e);
        }

        public async Task ScanForDevices()
        {
            _adapter.ScanMode = ScanMode.LowLatency;
            _adapter.ScanTimeout = 60000;

            await _adapter.StartScanningForDevicesAsync();
        }

        private void OnDeviceDetected(DeviceAddedEventArgs e)
        {
            DeviceDetected?.Invoke(this, e);
        }

        private void OnStateChanged(BluetoothStateChangedArgs args)
        {
            Debug.WriteLine($"Old State: {args.OldState.ToString()}, New State: {args.NewState.ToString()}");
        }
    }

    public class DeviceAddedEventArgs : EventArgs
    {
        public DeviceModel Device { get; set; }
    }
}
