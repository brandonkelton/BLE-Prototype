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
        public ObservableCollection<IDevice> Devices { get; set; } = new ObservableCollection<IDevice>();
        public ICommand ScanForDevicesCommand { private set; get; }
        private readonly IBLEService _bleService;

        public MainPageViewModel()
        {
            _bleService = DependencyService.Get<IBLEService>();
            _bleService.DeviceDetected += DeviceDetected;
            SetupButtonCommands();            
        }

        public async Task ScanForDevices()
        {
            await _bleService.ScanForDevices();
        }

        public async Task ConnectToDevice(Guid id)
        {
            var device = Devices.ToList().FirstOrDefault(d => d.Id.Equals(id));
            if (device != null)
            {
                await _bleService.ConnectToDevice(device);
            }
        }

        private void SetupButtonCommands()
        {
            ScanForDevicesCommand = new Command(async () => await ScanForDevices());
        }

        private void DeviceDetected(object sender, DeviceEventArgs e)
        {
            if (e.Device != null && !String.IsNullOrEmpty(e.Device.Name))
            {
                Devices.Add(e.Device);
            }
        }
    }
}
