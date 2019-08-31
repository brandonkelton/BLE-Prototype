using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<DeviceModel> Devices { get; set; } = new ObservableCollection<DeviceModel>();
        public ICommand ScanForDevicesCommand { private set; get; }
        private readonly IBLEService _bleService;

        public MainPageViewModel()
        {
            _bleService = DependencyService.Get<IBLEService>();
            _bleService.DeviceDetected += DeviceDetected;
            SetupButtonCommands();            
        }

        private void SetupButtonCommands()
        {
            ScanForDevicesCommand = new Command(execute: async () => await ScanForDevices());
        }

        public async Task ScanForDevices()
        {
            await _bleService.ScanForDevices();
        }

        private void DeviceDetected(object sender, DeviceAddedEventArgs args)
        {
            if (args.Device != null)
            {
                Devices.Add(args.Device);
            }
        }
    }
}
