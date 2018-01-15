using System;
using System.Threading.Tasks;
using Akka.Actor;

namespace Lighthouse.NetCoreApp
{
    public class LighthouseService
    {
        private readonly String _ipAddress;
        private readonly Int32? _port;

        private ActorSystem _lighthouseSystem;

        public LighthouseService() : this(null, null) { }

        public LighthouseService(String ipAddress, Int32? port)
        {
            this._ipAddress = ipAddress;
            this._port      = port;
        }

        public void Start() { this._lighthouseSystem = LighthouseHostFactory.LaunchLighthouse(this._ipAddress, this._port); }

        public async Task StopAsync() { await this._lighthouseSystem.Terminate(); }
    }
}
