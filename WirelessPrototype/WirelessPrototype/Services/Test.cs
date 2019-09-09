using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WirelessPrototype.Models;

namespace WirelessPrototype.Services
{
    class Test
    {
        List<IStat> stats = new List<IStat>();

        public void Go()
        {
            stats.Add(new HealthStat { Value = 1 });
            stats.Add(new CombatStat { Value = 1 });

            var healthStat = stats.OfType<HealthStat>();
        }
    }
}
