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
        event EventHandler<Exception> ErrorEvent;
        event EventHandler<string> InfoEvent;
        event EventHandler<bool> ServerClientStarted;
        event EventHandler<DeviceModel> DeviceDetected;

        bool IsServer { get; }
        bool IsClient { get; }

        void CreateServer();

        void CreateClient();

        Task SendToServer(string text);

        void SendToClients(string text);



        
        //event EventHandler<DeviceEventArgs> DeviceConnected;

        //event EventHandler<string> InfoEvent;

        //Task ScanForDevices();

        //Task StopScanningForDevices();

        //Task ConnectToDevice(Guid id);
    }
}
