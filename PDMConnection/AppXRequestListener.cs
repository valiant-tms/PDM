using System;

using Teamcenter.Soa.Client;

namespace PDMConnection.ClientX {
    
    public class AppXRequestListener : RequestListener {

        public void ServiceRequest(ServiceInfo info) {
            // will log the service name when done
        }

        public void ServiceResponse(ServiceInfo info) {
            Console.WriteLine(info.Id + ": " + info.Service + "." + info.Operation);
        }
    }
}