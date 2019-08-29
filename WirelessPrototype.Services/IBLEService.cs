using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using WirelessPrototype.Models;

namespace WirelessPrototype.Services
{
    public interface IBLEService
    {
        event EventHandler<DeviceAddedEventArgs> DeviceDetected;

        Task ScanForDevices();
    }
}
