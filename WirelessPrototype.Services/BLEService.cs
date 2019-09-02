﻿using MvvmCross;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WirelessPrototype.Services
{
    public class BLEService : IBLEService
    {
        public event EventHandler<DeviceEventArgs> DeviceDetected;
        public event EventHandler<DeviceEventArgs> DeviceConnected;

        private readonly IBluetoothLE _ble;
        private readonly IAdapter _adapter;

        public BLEService()
        {
            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            SetupEventHandling();
        }

        public IReadOnlyList<IDevice> ConnectedDevices => _adapter.ConnectedDevices;

        public async Task ScanForDevices()
        {
            if (_adapter.IsScanning) return;

            _adapter.ScanMode = ScanMode.LowLatency;
            _adapter.ScanTimeout = 10000;

            await _adapter.StartScanningForDevicesAsync();
        }

        public async Task StopScanningForDevices()
        {
            await _adapter.StopScanningForDevicesAsync();
        }

        public async Task ConnectToDevice(Guid id)
        {
            try
            {
                var device = _adapter.DiscoveredDevices.ToList().FirstOrDefault(d => d.Id.Equals(id));
                if (device != null) await _adapter.ConnectToDeviceAsync(device);
            } catch (DeviceConnectionException e)
            {
                // Handle bad connection
            }
        }

        private void SetupEventHandling()
        {
            _ble.StateChanged += OnStateChanged;
            _adapter.DeviceDiscovered += OnDeviceDetected;
            //_adapter.DeviceAdvertised += OnDeviceAdvertised;
            //_adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
            //_adapter.DeviceDisconnected += OnDeviceDisconnected;
            _adapter.DeviceConnected += OnDeviceConnected;
            //_adapter.DeviceConnectionLost += OnDeviceConnectionLost;
        }

        private void OnDeviceDetected(object sender, DeviceEventArgs e)
        {
            if (_adapter.DiscoveredDevices.ToList().FirstOrDefault(d => d.Id.Equals(e.Device.Id)) == null)
            {
                DeviceDetected?.Invoke(this, e);
            }
            
        }

        private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        {
            Debug.WriteLine($"Old State: {e.OldState}, New State: {e.NewState}");
        }

        private void OnDeviceConnected(object sender, DeviceEventArgs e)
        {
            DeviceConnected?.Invoke(this, e);
        }
    }
}
