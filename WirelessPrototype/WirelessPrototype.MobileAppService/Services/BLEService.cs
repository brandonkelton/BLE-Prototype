using MvvmCross;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WirelessPrototype.Models;

namespace WirelessPrototype.MobileAppService.Services
{
    public class BLEService : IBLEService
    {
        public ObservableCollection<DeviceModel> DeviceList = new ObservableCollection<DeviceModel>();

        public async Task ScanForDevices()
        {
            // var ble = Mvx.IoCProvider.GetSingleton<IBluetoothLE>();
            var adapter = Mvx.IoCProvider.GetSingleton<IAdapter>();

            adapter.DeviceDiscovered += (s, a) => 
                DeviceList.Add(new DeviceModel() { Id = a.Device.Id, Name = a.Device.Name });

            await adapter.StartScanningForDevicesAsync();
        }
    }
}
