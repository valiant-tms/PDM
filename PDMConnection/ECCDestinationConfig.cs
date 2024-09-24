using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAP.Middleware.Connector; // your sap connector

namespace PDMConnection {
    public class ECCDestinationConfig : IDestinationConfiguration {

        public bool ChangeEventsSupported() {
            return false;
        }

        public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;

        public RfcConfigParameters GetParameters(string destinationName) {

            RfcConfigParameters parms = new RfcConfigParameters();

            if (destinationName.Equals("EDV")) {
                parms.Add(RfcConfigParameters.AppServerHost, "192.168.17.45");
                parms.Add(RfcConfigParameters.SystemNumber, "01");
                parms.Add(RfcConfigParameters.SystemID, "EDV");
                parms.Add(RfcConfigParameters.User, "BCBATCH");
                parms.Add(RfcConfigParameters.Password, "VALIANT");
                parms.Add(RfcConfigParameters.Client, "350");
                parms.Add(RfcConfigParameters.Language, "EN");
                parms.Add(RfcConfigParameters.PoolSize, "5");
            }
            return parms;

        }
    }
}