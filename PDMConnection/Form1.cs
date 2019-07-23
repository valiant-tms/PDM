using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDMConnection {
    public partial class Form1 : Form {
        // SAP variables
        String sapSystem = "EDV";
        SAPConnection sapConnection = null;

        // PDM Variables
        int pdmInterval = 30;

        // Other variables
        bool success = true;

        // Tables
        DataSet pdmList = new DataSet();
        DataTable tcAttributes = new DataTable();
        public Form1() {
            InitializeComponent();
            this.Visible = true;
            this.info.Text += DateTime.Now + " Retrieving Communications configuration from SAP" + Environment.NewLine;
            getSAPConnection().initializeConfiguration();
            this.info.Text += DateTime.Now + " Interval set to " + getSAPConnection().getPDMInterval() + " Minutes" + Environment.NewLine;
            startCommunications(null, null);
            createTimer(this);
        }

        private void createTimer(ISynchronizeInvoke syncObject) {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.SynchronizingObject = syncObject;
            timer.Elapsed += startCommunications;
            int i = getSAPConnection().getPDMInterval();
            timer.Interval = i * 1000 * 60;
            timer.Enabled = true;
        }
        private SAPConnection getSAPConnection() {
            if (sapConnection == null ) {
                sapConnection = new SAPConnection(sapSystem);
            }
            return sapConnection;

        }

         private void startCommunications(object source, ElapsedEventArgs e) {
            this.info.Text += DateTime.Now + " Starting Communications" + Environment.NewLine;
            this.info.Text += DateTime.Now + " Connecting to SAP System for first time " + Environment.NewLine;
            this.info.Text += DateTime.Now + " Retrieving PDM'S to connect to" + Environment.NewLine;
            setPDMList(getSAPConnection().getPDMs());
            connectToPDMs();
            this.info.Text += "----------------------------------------------------------------" + Environment.NewLine;
        }

        private DataSet getPdmList() {
            return pdmList;
        }

        private void setPDMList(DataSet pdmList) {
            this.pdmList = pdmList;
        }

        private void connectToPDMs() {
            DataTable pdms = new DataTable();

            pdms = getPdmList().Tables["Table1"];
            this.info.Text += DateTime.Now + " Number of PDM's found = " + pdms.Rows.Count + Environment.NewLine;

            // Go through each of the PDM's
            foreach (DataRow pdm in pdms.Rows) {
                this.info.Text += DateTime.Now + " Connecting to " + pdm["NAME"].ToString();
                if (pdm["PTYPE"].Equals("TC")) {
                    this.info.Text += DateTime.Now + " Retrieving Attributes to get from PDM" + Environment.NewLine;
                    setTCAttributes(getSAPConnection().getAttributes(pdm["PDMID"].ToString()));
                    TeamCenterPDM tc = new TeamCenterPDM(pdm, tcAttributes);
                    tc.process(getSAPConnection());
                } 

                if (success) {
                    // Will need to check if this is stil required with new bom extract method

                    //                    updateSAPWithTimeStamp();
                }
            }
        }      

        private void setTCAttributes(DataTable attributes) {
            this.tcAttributes = attributes;
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            this.info.Refresh();
            this.info.ScrollToCaret();
        }
    }
}
