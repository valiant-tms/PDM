using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa.Client.Model.Strong;
using Teamcenter.Soa.Common;
using Teamcenter.Soa.Exceptions;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Client;
using Teamcenter.Services.Strong.Core._2006_03.DataManagement;
using Teamcenter.Services.Strong.Core._2008_06.DataManagement;
using Teamcenter.Services.Loose.Core._2006_03.FileManagement;

namespace PDMConnection.ClientX {
    class UploadDownloadFsc3 {
        DataManagementService dservice;
        FileManagementService fmservice;
        FileManagementUtility fmsFileManagement;
        Folder homeFolder;
        Item item;
        ItemRevision itemRev;
        Dataset dataset;

        String fileName = "Test.pdf";
        String filePath = "c:\\WORK\\";

        public UploadDownloadFsc3(User user) {
//            String[] FMS_Bootstrap_Urls = new string[] { host };
//            String cacheDir = "c:\\work\\";

            dservice = DataManagementService.getService(PDMConnection.ClientX.Session.getConnection());
            fmservice = FileManagementService.getService(PDMConnection.ClientX.Session.getConnection());
            fmsFileManagement = new FileManagementUtility(PDMConnection.ClientX.Session.getConnection(), null, null, new[] { "" }, "C:\\WORK\\");

            try {
                homeFolder = user.Home_folder;
            } catch (NotLoadedException e) {
                Console.WriteLine(e.StackTrace);
            }

            setObjectPolicy();
        }

        public void CreateItemItemRevDataset(String itemId, String itemRevId) {
 //           ModelObjectFileManagment
        }
        public void DownloadAttachedFile() {
            try {
                ModelObject[] objs = itemRev.IMAN_reference;
                if (objs.Length > 0 && objs[0] is Text) {
                    Teamcenter.Soa.Client.Model.Property refListProperty = objs[0].GetProperty("ref_list");
                    ModelObject[] refObjs = refListProperty.ModelObjectArrayValue;

                    if (refObjs.Length > 0 && refObjs[0] is ImanFile) {
                        GetFileResponse fileResp = fmsFileManagement.GetFiles(refObjs);
                        FileInfo[] files = fileResp.GetFiles();
                        foreach (FileInfo fileInfo in files) {
                            String name = Environment.GetEnvironmentVariable("HOMEDRIVE") + Environment.GetEnvironmentVariable("HOMEPATH") + "\\Desktop\\" + fileInfo.Name;

                            fileInfo.MoveTo(name);
                        }

                    }
                }
            } catch (NotLoadedException e) {
                Console.WriteLine(e.StackTrace);
            }
        }

        protected void setObjectPolicy() {
            SessionService session = SessionService.getService(PDMConnection.ClientX.Session.getConnection());
            ObjectPropertyPolicy policy = new ObjectPropertyPolicy();

            policy.AddType(new PolicyType("ItemRevision", new string[] { "IMAN_reference" }));
            policy.AddType(new PolicyType("Dataset", new string[] { "ref_list" }));

            session.SetObjectPropertyPolicy(policy);
        }
    }
}
