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
        public ObservableCollection<DeviceModel> DetectedDevices { get; private set; } = new ObservableCollection<DeviceModel>();
        // public ObservableCollection<DeviceModel> ConnectedDevices { get; set; } = new ObservableCollection<DeviceModel>();
        //public ICommand ScanForDevicesCommand { private set; get; }
        //public ICommand ConnectToDeviceCommand { private set; get; }

        public ICommand StartServerCommand { get; private set; }
        public ICommand StartClientCommand { get; private set; }
        public ICommand SendMessageCommand { get; private set; }

        private readonly IBLEService _bleService;

        public MainPageViewModel()
        {
            _bleService = DependencyService.Get<IBLEService>();
            _bleService.DeviceDetected += OnDeviceDetected;
            
            //_bleService.DeviceConnected += OnDeviceConnected;
            _bleService.ErrorEvent += OnErrorEvent;
            _bleService.InfoEvent += OnInfoEvent;
            _bleService.ServerClientStarted += OnServerClientStarted;

            SetupButtonCommands();            
        }

        public bool IsServer => _bleService != null && _bleService.IsServer;
        public bool IsClient => _bleService != null && _bleService.IsClient;

        private async Task CreateServer()
        {
            await _bleService.CreateServer();
        }

        private void CreateClient()
        {
            _bleService.CreateClient();
        }

        private async Task SendToServer(string text)
        {
            await _bleService.SendToServer(text);
        }

        private void SendToClients(string text)
        {
            _bleService.SendToClients(text);
        }

        //public async Task ScanForDevices()
        //{
        //    await _bleService.ScanForDevices();
        //}

        //public async Task ConnectToDevice(Guid id)
        //{
        //    await _bleService.ConnectToDevice(id);
        //}

        private string errorDetail;
        public string ErrorDetail
        {
            get { return errorDetail; }
            set { SetProperty(ref errorDetail, value); }
        }

        private string infoDetail;
        public string InfoDetail
        {
            get { return infoDetail; }
            set { SetProperty(ref infoDetail, value); }
        }

        private string sendMessageText;
        public string SendMessageText
        {
            get { return sendMessageText; }
            set { SetProperty(ref sendMessageText, value); }
        }

        private bool canStartServerClient = true;
        public bool CanStartServerClient
        {
            get { return canStartServerClient; }
            set { SetProperty(ref canStartServerClient, value); }
        }

        private bool allowMessaging;
        public bool AllowMessaging
        {
            get { return allowMessaging; }
            set { SetProperty(ref allowMessaging, value); }
        }

        private void SetupButtonCommands()
        {
            //ScanForDevicesCommand = new Command(async () => await ScanForDevices());
            //ConnectToDeviceCommand = new Command<Guid>(async id => await ConnectToDevice(id));
            StartServerCommand = new Command(async () => await CreateServer());
            StartClientCommand = new Command(() => CreateClient());
            SendMessageCommand = new Command(async () =>
            {
                if (IsServer)
                {
                    await SendToServer(SendMessageText);
                }
                else
                {
                    SendToClients(SendMessageText);
                }
            });
        }

        private void OnDeviceDetected(object sender, DeviceModel model)
        {
            if (DetectedDevices.Any(d => d.Id == model.Id || d.Name == model.Name))
                return;

            DetectedDevices.Add(model);
        }

        //private void OnDeviceConnected(object sender, DeviceEventArgs e)
        //{
        //    if (e.Device != null)
        //    {
        //        var device = new DeviceModel
        //        {
        //            Id = e.Device.Id,
        //            Name = e.Device.Name
        //        };

        //        ConnectedDevices.Add(device);
        //    }
        //}

        private void OnErrorEvent(object sender, Exception e)
        {
            ErrorDetail = e.Message + "\r\n" + e.StackTrace;
        }

        private void OnInfoEvent(object sender, string info)
        {
            InfoDetail = info + "\r\n" + InfoDetail;
        }

        private void OnServerClientStarted(object sender, bool isStarted)
        {
            CanStartServerClient = !isStarted;
            AllowMessaging = isStarted;
        }
    }
}
