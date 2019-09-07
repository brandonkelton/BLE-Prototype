using Plugin.BluetoothLE;
using Plugin.BluetoothLE.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IGattService = Plugin.BluetoothLE.Server.IGattService;

namespace WirelessPrototype.Services
{
    public class BLEService : IBLEService, IDisposable
    {
        public event EventHandler<Exception> ErrorEvent;
        public event EventHandler<string> InfoEvent;
        public event EventHandler<bool> ServerClientStarted;

        private IGattServer _server = null;
        private IDisposable _serverSubscription = null;
        private IDisposable _notifyBroadcastSubscription = null;
        private IDisposable _deviceSubscriptionChangedSubscription = null;
        private IDisposable _characteristicReadReceived = null;
        private IDisposable _characteristicWriteReceived = null;
        private IDisposable _scanSubscription = null;
        private IDisposable _clientWriteSub = null;
        private IDisposable _clientNotifySub = null;

        private readonly string _serverName = "PrototypeServer";
        private readonly Guid _serverUUID = Guid.NewGuid();
        private readonly Guid _readWriteServiceUUID = Guid.NewGuid();
        private readonly Guid _notifyServiceUUID = Guid.NewGuid();

        private Plugin.BluetoothLE.IGattCharacteristic _writeCharacteristic = null;
        private Plugin.BluetoothLE.Server.IGattCharacteristic _serverReadWriteCharacteristic = null;
        private Plugin.BluetoothLE.Server.IGattCharacteristic _serverNotifyCharacteristic = null;

        public bool IsServer => _server != null;
        public bool IsClient { get; private set; }

        public void CreateServer()
        {
            if (CrossBleAdapter.Current.Status == AdapterStatus.PoweredOn)
            {
                _serverSubscription = CrossBleAdapter.Current.CreateGattServer().Subscribe(
                server =>
                {
                    _server = server;
                    _server.AddService(_serverUUID, true, service => ConfigureService(service));
                },
                error =>
                {
                    RaiseErrorEvent(error);
                },
                () => RaiseServerClientStarted(true));
            }
            else
            {
                var exception = new Exception("Bluetooth is OFF");
                RaiseErrorEvent(exception);
            }
        }

        public void CreateClient()
        {
            IsClient = true;
            RaiseServerClientStarted(true);

            var scanConfig = new ScanConfig
            {
                ScanType = BleScanType.LowLatency,
                ServiceUuids = new List<Guid> { _notifyServiceUUID, _readWriteServiceUUID }
            };

            _scanSubscription = CrossBleAdapter.Current.Scan(scanConfig).Subscribe(scanResult =>
            {
                if (scanResult.AdvertisementData.LocalName == _serverName)
                {
                    var connectConfig = new ConnectionConfig();
                    scanResult.Device.Connect();

                    scanResult.Device.WhenAnyCharacteristicDiscovered().Subscribe(characteristic =>
                    {
                        if (characteristic.CanRead() && characteristic.CanWrite())
                        {
                            _writeCharacteristic = characteristic;
                            _clientWriteSub = characteristic.Read().Subscribe(result =>
                            {
                                var text = Encoding.UTF8.GetString(result.Data);
                                RaiseInfoEvent("Read: " + text);
                            });
                        }
                        if (characteristic.CanNotifyOrIndicate())
                        {
                            _clientNotifySub = characteristic.EnableNotifications().Subscribe(result =>
                            {
                                var text = Encoding.UTF8.GetString(result.Data);
                                RaiseInfoEvent("Notification: " + text);
                            });
                        }
                    });
                }
            });
        }

        public async Task SendToServer(string text)
        {
            var sendBytes = Encoding.UTF8.GetBytes(text);
            var result = await _writeCharacteristic.Write(sendBytes);
            var receivedText = Encoding.UTF8.GetString(result.Data);
            RaiseInfoEvent("Received: " + receivedText);
        }

        public void SendToClients(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            //Can specify specific devices as an optional paramter
            _serverNotifyCharacteristic.Broadcast(bytes);
        }

        private void ConfigureService(IGattService service)
        {
            _serverReadWriteCharacteristic = service.AddCharacteristic
            (
                _readWriteServiceUUID,
                CharacteristicProperties.Read | CharacteristicProperties.Write | CharacteristicProperties.WriteNoResponse,
                GattPermissions.Read | GattPermissions.Write
            );

            _serverNotifyCharacteristic = service.AddCharacteristic
            (
                _notifyServiceUUID,
                CharacteristicProperties.Indicate | CharacteristicProperties.Notify,
                GattPermissions.Read | GattPermissions.Write
            );

            _deviceSubscriptionChangedSubscription = 
                _serverNotifyCharacteristic.WhenDeviceSubscriptionChanged().Subscribe(e =>
                {
                    var @event = e.IsSubscribed ? "Subscribed" : "Unsubcribed";
                    
                    if (_notifyBroadcastSubscription == null)
                    {
                        _notifyBroadcastSubscription = Observable
                            .Interval(TimeSpan.FromSeconds(1))
                            .Where(x => _serverNotifyCharacteristic.SubscribedDevices.Count > 0)
                            .Subscribe(_ =>
                            {
                                var dt = "Notification: A Subscription Changed";
                                var bytes = Encoding.UTF8.GetBytes(dt);
                                _serverNotifyCharacteristic.Broadcast(bytes);
                            });
                    }
                });


            _characteristicReadReceived = _serverReadWriteCharacteristic.WhenReadReceived().Subscribe(x =>
            {
                var textSendingToClient = "READ RECEIVED: Welcome to TND!!!";

                // you must set a reply value
                x.Value = Encoding.UTF8.GetBytes(textSendingToClient);
                x.Status = GattStatus.Success; // you can optionally set a status, but it defaults to Success
            });
            _characteristicWriteReceived = _serverReadWriteCharacteristic.WhenWriteReceived().Subscribe(x =>
            {
                var textReceivedFromClient = Encoding.UTF8.GetString(x.Value, 0, x.Value.Length);
                RaiseInfoEvent(textReceivedFromClient);
            });

            var adData = new AdvertisementData
            {
                LocalName = _serverName,
                ServiceUuids = _server.Services.Select(s => s.Uuid).ToList()
            };
            
            CrossBleAdapter.Current.Advertiser.Start(adData);
        }

        private void RaiseErrorEvent(Exception e)
        {
            ErrorEvent?.Invoke(this, e);
        }

        private void RaiseInfoEvent(string info)
        {
            InfoEvent?.Invoke(this, info);
        }

        private void RaiseServerClientStarted(bool isStarted)
        {
            ServerClientStarted?.Invoke(this, isStarted);
        }

        public void Dispose()
        {
            if (_serverSubscription != null) _serverSubscription.Dispose();
            if (_server != null) _server.Dispose();
            if (_notifyBroadcastSubscription != null) _notifyBroadcastSubscription.Dispose();
            if (_deviceSubscriptionChangedSubscription != null) _deviceSubscriptionChangedSubscription.Dispose();
            if (_characteristicReadReceived != null) _characteristicReadReceived.Dispose();
            if (_characteristicWriteReceived != null) _characteristicWriteReceived.Dispose();
            if (_scanSubscription != null) _scanSubscription.Dispose();
            if (_clientNotifySub != null) _clientNotifySub.Dispose();
            if (_clientWriteSub != null) _clientWriteSub.Dispose();
        }



        //public event EventHandler<IDevice> DeviceDetected;
        //public event EventHandler<DeviceEventArgs> DeviceConnected;
        //public event EventHandler<Exception> ErrorEvent;
        //public event EventHandler<string> InfoEvent;

        //private readonly IBluetoothLE _ble;
        //private readonly IAdapter _adapter;

        //public BLEService()
        //{
        //    _ble = CrossBluetoothLE.Current;
        //    _adapter = CrossBluetoothLE.Current.Adapter;

        //    SetupEventHandling();
        //}

        //public IReadOnlyList<IDevice> ConnectedDevices => _adapter.ConnectedDevices;

        //public async Task ScanForDevices()
        //{
        //    if (_adapter.IsScanning) return;

        //    _adapter.ScanMode = ScanMode.LowLatency;
        //    _adapter.ScanTimeout = 10000;

        //    try
        //    {
        //        await _adapter.StartScanningForDevicesAsync();
        //    }
        //    catch (Exception e)
        //    {
        //        RaiseErrorEvent(e);
        //    }
        //}

        //public async Task StopScanningForDevices()
        //{
        //    try
        //    {
        //        await _adapter.StopScanningForDevicesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        RaiseErrorEvent(ex);
        //    }

        //}

        //public async Task ConnectToDevice(Guid id)
        //{
        //    try
        //    {
        //        var device = _adapter.DiscoveredDevices.ToList().FirstOrDefault(d => d.Id.Equals(id));
        //        if (device != null) await _adapter.ConnectToDeviceAsync(device);
        //    }
        //    catch (DeviceConnectionException e)
        //    {
        //        RaiseErrorEvent(e);
        //    }
        //}

        ////public async Task SendTextToAllDevices(string text)
        ////{
        ////    try
        ////    {
        ////        foreach (var device in _adapter.ConnectedDevices)
        ////        {
        ////            device.
        ////        }
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        RaiseErrorEvent(e);
        ////    }
        ////}

        //private void SetupEventHandling()
        //{
        //    _ble.StateChanged += OnStateChanged;
        //    _adapter.DeviceDiscovered += OnDeviceDetected;
        //    _adapter.DeviceAdvertised += OnDeviceAdvertised;
        //    //_adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
        //    //_adapter.DeviceDisconnected += OnDeviceDisconnected;
        //    _adapter.DeviceConnected += OnDeviceConnected;
        //    //_adapter.DeviceConnectionLost += OnDeviceConnectionLost;
        //}

        //private void OnDeviceDetected(object sender, DeviceEventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(e.Device?.Name))
        //    {
        //        try
        //        {
        //            DeviceDetected?.Invoke(this, e.Device);
        //        }
        //        catch (Exception ex)
        //        {
        //            RaiseErrorEvent(ex);
        //        }

        //    }
        //}

        //private void OnDeviceAdvertised(object sender, DeviceEventArgs e)
        //{
        //    if (!String.IsNullOrEmpty(e.Device?.Name))
        //    {
        //        try
        //        {
        //            DeviceDetected?.Invoke(this, e.Device);
        //        }
        //        catch (Exception ex)
        //        {
        //            RaiseErrorEvent(ex);
        //        }

        //    }
        //}

        //private void RaiseErrorEvent(Exception e)
        //{
        //    ErrorEvent?.Invoke(this, e);
        //}

        //private void OnStateChanged(object sender, BluetoothStateChangedArgs e)
        //{
        //    // Debug.WriteLine($"Old State: {e.OldState}, New State: {e.NewState}");
        //}

        //private void OnDeviceConnected(object sender, DeviceEventArgs e)
        //{
        //    try
        //    {
        //        DeviceConnected?.Invoke(this, e);

        //        var services = await e.Device.GetServicesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        RaiseErrorEvent(ex);
        //    }

        //}
    }
}
