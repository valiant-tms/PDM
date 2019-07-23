using System;
using System.Data;

using SAP.Middleware.Connector;

namespace PDMConnection {
    public class SAPConnection {
        string system = "EDV"; // Default in case used by error
        bool destinationInitialized = false;
        int pdmInterval = 30;

        DataSet connectedPDMs = new DataSet();
        DataSet projects = new DataSet();
        DataTable attributes = new DataTable();

        public SAPConnection(String system) {
            setSystem(system);
        }

        public void initializeConfiguration() {
            ECCDestinationConfig config = new ECCDestinationConfig();

            if (!isDestinationInitialized()) {
                RfcDestinationManager.RegisterDestinationConfiguration(config);
                destinationInitialized = true;
            }

            RfcDestination destination = RfcDestinationManager.GetDestination(getSystem());
            RfcRepository repository = destination.Repository;

            IRfcFunction configurationFunction = repository.CreateFunction("ZBAPI_GET_PDM_CONFIG");
            configurationFunction.Invoke(destination);

            setPDMInterval(configurationFunction.GetInt("INTERVAL"));
        }

        public DataSet getPDMs() {
            PDMConnection.ECCDestinationConfig config = new PDMConnection.ECCDestinationConfig();

            if (!isDestinationInitialized()) {
                RfcDestinationManager.RegisterDestinationConfiguration(config);
                destinationInitialized = true;
            }

            RfcDestination destination = RfcDestinationManager.GetDestination(getSystem());
            RfcRepository repository = destination.Repository;

            IRfcFunction pdmsFunction = repository.CreateFunction("ZBAPI_GETPDMS");
            pdmsFunction.Invoke(destination);

            connectedPDMs.Tables.Clear();
            connectedPDMs.Tables.Clear();
            connectedPDMs.Tables.Add(ConvertToDotNetTable(pdmsFunction.GetTable("PDM_LIST")));

            return connectedPDMs;
        }

        public DataTable getAttributes(String pdmId) {
            PDMConnection.ECCDestinationConfig config = new PDMConnection.ECCDestinationConfig();

            if (!isDestinationInitialized()) {
                RfcDestinationManager.RegisterDestinationConfiguration(config);
                setDestinationInitialized(true);
            }

            RfcDestination destination = RfcDestinationManager.GetDestination(getSystem());
            RfcRepository repository = destination.Repository;

            IRfcFunction attributeFunction = repository.CreateFunction("ZBAPI_GET_PDM_ATTRIBUTES");
            attributeFunction.SetValue("pdmid", pdmId);
            attributeFunction.Invoke(destination);

//            attributesClear();
            attributes = ConvertToDotNetTable(attributeFunction.GetTable("ATTRIBUTES"));
            return attributes;
        }

        public DataTable getJobList() {
            PDMConnection.ECCDestinationConfig config = new PDMConnection.ECCDestinationConfig();

            if (!isDestinationInitialized()) {
                RfcDestinationManager.RegisterDestinationConfiguration(config);
                setDestinationInitialized(true);
            }

            RfcDestination destination = RfcDestinationManager.GetDestination(getSystem());
            RfcRepository repository = destination.Repository;

            IRfcFunction jobFunction = repository.CreateFunction("ZBAPI_GET_PDM_PROJECTS_LIST");
            jobFunction.Invoke(destination);

            projects.Tables.Clear();
            projects.Tables.Add(ConvertToDotNetTable(jobFunction.GetTable("PROJECTS")));

            return projects.Tables["Table1"];
        }

        public int getPDMInterval() {
            return pdmInterval;
        }

        private string getSystem() {
            return system;
        }

        private bool isDestinationInitialized() {
            return destinationInitialized;
        }

        private void setDestinationInitialized(bool destinationInitialized) {
            this.destinationInitialized = destinationInitialized;
        }

        private void setPDMInterval(int interval) {
            pdmInterval = interval;
        }

        private void setSystem(string system) {
            this.system = system;
        }

        private DataTable ConvertToDotNetTable(IRfcTable RFCTable) {
            DataTable dtTable = new DataTable();

            for (int item = 0; item < RFCTable.ElementCount; item++) {
                RfcElementMetadata metadata = RFCTable.GetElementMetadata(item);
                dtTable.Columns.Add(metadata.Name);
            }

            foreach (IRfcStructure row in RFCTable) {
                DataRow dr = dtTable.NewRow();
                for (int item = 0; item < RFCTable.ElementCount; item++) {
                    RfcElementMetadata metadata = RFCTable.GetElementMetadata(item);
                    if (metadata.DataType == RfcDataType.BCD && metadata.Name == "ABC") {
                        dr[item] = row.GetInt(metadata.Name);
                    } else {
                        dr[item] = row.GetString(metadata.Name);
                    }
                }
                dtTable.Rows.Add(dr);
            }
            return dtTable;
        }

        public void send2SAP(String jobNumber, DataTable attributes, DataTable bomItems) {
            PDMConnection.ECCDestinationConfig config = new PDMConnection.ECCDestinationConfig();

            if (!isDestinationInitialized()) {
                RfcDestinationManager.RegisterDestinationConfiguration(config);
                setDestinationInitialized(true);
            }

            RfcDestination destination = RfcDestinationManager.GetDestination(getSystem());
            RfcRepository repository = destination.Repository;

            IRfcFunction testfn = repository.CreateFunction("ZBAPI_PDM2SAP");
            testfn.SetValue("PSPNR", jobNumber);
            IRfcTable items = testfn.GetTable("ITEMS");
            String attrs = "";
            foreach (DataRow rows in bomItems.Rows) {

                foreach (DataRow attribute in attributes.Rows) {
                    attrs = attribute["ATTRI"].ToString();
                    if (!(rows[attrs] is DBNull)) {
                        items.Append();
                        String parent = rows["PARENT_ID"].ToString();
                        items.SetValue("PARENT_ID", parent);
                        items.SetValue("ITEM_ID", rows["BL_ITEM_FND0OBJECTID"]);
                        items.SetValue("BOM_ID", rows["BL_CLONE_STABLE_OCCURRENCE_ID"]);
                        items.SetValue("ATTRIBUTE", attrs);
                        items.SetValue("VALUE", rows[attrs]);
                    }
                }
            }
            testfn.Invoke(destination);

        }
    }
}