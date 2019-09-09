using Newtonsoft.Json;
using System;

namespace WirelessPrototype.Models
{
    public class DeviceModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
