using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using WirelessPrototype.Models;
using WirelessPrototype.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WirelessPrototype.Views
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<DeviceModel> Devices { get; set; } = new ObservableCollection<DeviceModel>();
        private readonly IBLEService _bleService;

        public MainPage()
        {
            InitializeComponent();
            _bleService = DependencyService.Resolve<IBLEService>();
            _bleService.DeviceDetected += DeviceDetected;
            _bleService.ScanForDevices();
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