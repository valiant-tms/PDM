using System;
using System.Data;

// Libraries for TeamCenter
using Teamcenter.Soa.Client.Model;

// Definition 
using User = Teamcenter.Soa.Client.Model.Strong.User;

namespace PDMConnection {
    public class TeamCenterPDM {
        String serverHost = "";

        DataRow pdm = null;
        DataTable attributes = null;
        private DataTable bomItems = null;

        public TeamCenterPDM() {
        }

        public TeamCenterPDM(DataRow pdm, DataTable attributes) {
            setPDM(pdm);
            setAttributes(attributes);
        }

        private DataTable getAttributes() {
            if (attributes == null) {
                attributes = new DataTable();
            }
            return attributes;
        }

        public DataTable getBOMItems() {
            return bomItems;
        }

        private DataRow getPDM() {
            return pdm;
        }

        public void process(SAPConnection sapConnection) {
            DataTable projects = sapConnection.getJobList(); 
            serverHost = getPDM()["HOST"].ToString();

            try {
                ClientX.Session session = new ClientX.Session(serverHost);
                User user = session.login();
                foreach (DataRow project in projects.Rows) {
                    bomItems = session.getObjects(project["ITEMID"].ToString(), project["REVID"].ToString(), getAttributes());
                    sapConnection.send2SAP(project["PSPNR"].ToString(), getAttributes(), bomItems);
                }
            } catch(SystemException e) {
                Console.WriteLine(e.StackTrace);
            }

        }

        private void setAttributes(DataTable attributes) {
            this.attributes = attributes;
        }

        private void setPDM(DataRow pdm) {
            this.pdm = pdm;
        }
    }
}