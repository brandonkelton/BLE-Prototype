using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WirelessPrototype.Models;
using WirelessPrototype.Services;
using Xamarin.Forms;

namespace WirelessPrototype.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        public ObservableCollection<DeviceModel> DetectedDevices { get; set; } = new ObservableCollection<DeviceModel>();
        public ObservableCollection<DeviceModel> ConnectedDevices { get; set; } = new ObservableCollection<DeviceModel>();
        public ICommand ScanForDevicesCommand { private set; get; }
        public ICommand ConnectToDeviceCommand { private get; set; }

        private readonly IBLEService _bleService;

        public MainPageViewModel()
        {
            _bleService = DependencyService.Get<IBLEService>();
            _bleService.DeviceDetected += OnDeviceDetected;
            _bleService.DeviceConnected += OnDeviceConnected;

            SetupButtonCommands();            
        }

        public async Task ScanForDevices()
        {
            await _bleService.ScanForDevices();
        }

        public async Task ConnectToDevice(Guid id)
        {
            await _bleService.ConnectToDevice(id);
        }

        private void SetupButtonCommands()
        {
            ScanForDevicesCommand = new Command(async () => await ScanForDevices());
            ConnectToDeviceCommand = new Command<Guid>(async id => await ConnectToDevice(id));
        }

        private void OnDeviceDetected(object sender, DeviceEventArgs e)
        {
            if (e.Device != null && !String.IsNullOrEmpty(e.Device.Name))
            {
                var device = new DeviceModel
                {
                    Id = e.Device.Id,
                    Name = e.Device.Name
                };

                DetectedDevices.Add(device);
            }
        }

        private void OnDeviceConnected(object sender, DeviceEventArgs e)
        {
            if (e.Device != null)
            {
                var device = new DeviceModel
                {
                    Id = e.Device.Id,
                    Name = e.Device.Name
                };

                ConnectedDevices.Add(device);
            }
        }
    }
}
