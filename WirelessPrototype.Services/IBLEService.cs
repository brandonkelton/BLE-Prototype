using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace WirelessPrototype.Services
{
    public interface IBLEService
    {
        event EventHandler<DeviceEventArgs> DeviceDetected;
        event EventHandler<DeviceEventArgs> DeviceConnected;
        event EventHandler<Exception> ErrorEvent;

        Task ScanForDevices();

        Task StopScanningForDevices();

        Task ConnectToDevice(Guid id);
    }
}
