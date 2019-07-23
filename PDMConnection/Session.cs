using System;
using System.Data;
using System.Collections.Generic;
using Teamcenter.Services.Strong.Core._2006_03.Session;

/*
using Teamcenter.Services.Loose.Core._2006_03.FileManagement;
using Teamcenter.Schemas.Soa._2006_03.Exceptions;
using Teamcenter.Services.Strong.Core._2006_03.Session;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa;
using Teamcenter.Soa.Common;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Client;
using Teamcenter.Soa.Exceptions;
using Teamcenter.Services.Strong.Core._2006_03.DataManagement;
using Teamcenter.Services.Strong.Core._2008_06.DataManagement;
*/

using Teamcenter.Schemas.Soa._2006_03.Exceptions;
using Teamcenter.Services.Strong.Core._2006_03.Session;
//using Teamcenter.Services.Strong.Core._2008_06.Session;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Services.Strong.Core._2008_06.DataManagement;
using Teamcenter.Soa;
using Teamcenter.Soa.Client;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Exceptions;
using Teamcenter.Soa.Common;


using User = Teamcenter.Soa.Client.Model.Strong.User;
using Revision = Teamcenter.Soa.Client.Model.Strong.ItemRevision;

namespace PDMConnection.ClientX {

    public class Session {

        private static Teamcenter.Soa.Client.Connection connection;
        private static AppXCredentialManager credentialManager;
        private User user = null;
        private String host = "";

        public Session(String host) {
            credentialManager = new AppXCredentialManager();
            String proto = null;
            String envNamesTccs = null;
            this.host = host;
            if (host.StartsWith("http")) {
                proto = SoaConstants.HTTP;
            } else
            if (host.StartsWith("tccs")) {
                proto = SoaConstants.TCCS;
                int envNamesStart = host.IndexOf('/') + 2;
                envNamesTccs = host.Substring(envNamesStart, host.Length - envNamesStart);
            }

            connection = new Teamcenter.Soa.Client.Connection(host, new System.Net.CookieCollection(), credentialManager, SoaConstants.REST, proto, false);
            if (proto == SoaConstants.TCCS) {
                connection.SetOption(Teamcenter.Soa.Client.Connection.TCCS_ENV_NAME, envNamesTccs);
            }
            connection.ExceptionHandler = new AppXExceptionHandler();
            connection.ModelManager.AddPartialErrorListener(new AppXPartialErrorListener());
            connection.ModelManager.AddModelEventListener(new AppXModelEventListener());
            Teamcenter.Soa.Client.Connection.AddRequestListener(new AppXRequestListener());

        }

        public static Teamcenter.Soa.Client.Connection getConnection() {
            return connection;
        }

        private Teamcenter.Services.Strong.Core.DataManagementService getDMService() {
            return Teamcenter.Services.Strong.Core.DataManagementService.getService(getConnection());
        }

        private Teamcenter.Services.Strong.Core.FileManagementService getFMServices() {
            return Teamcenter.Services.Strong.Core.FileManagementService.getService(getConnection());
        }

        public DataTable getObjects(String itemId, String revisionId, DataTable attributes) {
            DataTable bomList = new DataTable();
            Revision[] revisions = new Revision[0];

            try {
                String targetItemId = itemId;
                String targetRevId = revisionId;

                if (targetItemId != "" && targetItemId.Length > 6) {
                    GetItemAndRelatedObjectsResponse response = getItem(targetItemId, targetRevId);
                    TCBomTree bomTree = new TCBomTree(getConnection(), attributes);
                    bomTree.ItemUID1 = response.Output[0].Item.Uid;
                    bomTree.ItemRevUID1 = response.Output[0].ItemRevOutput[0].ItemRevision.Uid;
                    bomTree.loadItem();

                    bomList = bomTree.getBOMStructure();
                }

                setObjectPolicy(attributes);
                 foreach (DataRow row in bomList.Rows) {
                    Revision rev = getItem(row["BL_ITEM_ITEM_ID"].ToString(), row["BL_REV_ITEM_REVISION_ID"].ToString()).Output[0].ItemRevOutput[0].ItemRevision;
                     foreach (DataRow attribute in attributes.Rows) {
                        Console.WriteLine(attribute["ATTRI"].ToString().ToUpper());
                        try {
                            row[attribute["ATTRI"].ToString().ToUpper()] = rev.GetPropertyDisplayableValue(attribute["ATTRI"].ToString());
                        } catch {
                            if (attribute["ATYPE"].ToString().ToUpper().Equals("ITEMREV")) {
                                row[attribute["ATTRI"].ToString().ToUpper()] = "";
                            }
                        }
                    }
                }
            } catch (NotLoadedException e) {

            }
            
            return bomList;
        }

        private GetItemAndRelatedObjectsResponse getItem(String itemId, String revisionId) {
            GetItemAndRelatedObjectsInfo[] itemArray = new GetItemAndRelatedObjectsInfo[1];
            GetItemAndRelatedObjectsInfo itemRevInput = new GetItemAndRelatedObjectsInfo();

            AttrInfo[] attrInfoArray = new AttrInfo[1];
            AttrInfo attrInfo = new AttrInfo();
            attrInfo.Name = "item_id";
            attrInfo.Value = itemId;
            attrInfoArray[0] = attrInfo;

            ItemInfo itemInfo = new ItemInfo();
            itemInfo.Ids = attrInfoArray;
            itemInfo.ClientId = "iteminfo1";
            itemInfo.UseIdFirst = true;

            RevInfo revInfo = new RevInfo();
            revInfo.Id = revisionId;
            revInfo.NRevs = 1;
            revInfo.ClientId = "revInfo1";
            revInfo.UseIdFirst = true;
            revInfo.Processing = "Ids";

            DatasetInfo dsInfo = new DatasetInfo();
            dsInfo.ClientId = "dsInfo1";

            dsInfo.Filter = new DatasetFilter();
            dsInfo.Filter.Processing = "None";

            itemRevInput.ItemInfo = itemInfo;
            itemRevInput.RevInfo = revInfo;
            itemArray[0] = itemRevInput;
            GetItemAndRelatedObjectsInfo[] itemAndRelObjInfo = new GetItemAndRelatedObjectsInfo[1];
            itemAndRelObjInfo[0] = new GetItemAndRelatedObjectsInfo();
            itemAndRelObjInfo[0].ItemInfo = itemInfo;
            itemAndRelObjInfo[0].RevInfo = revInfo;
            itemAndRelObjInfo[0].DatasetInfo = dsInfo;
            itemAndRelObjInfo[0].ClientId = "itemAndRelObj1";
            GetItemAndRelatedObjectsResponse response = getDMService().GetItemAndRelatedObjects(itemAndRelObjInfo);

            if (response.Output[0] == null) {
                return null;
            }

            if (itemId.Equals("VMT-0000006840")) {
 //               try {
//                  UploadDownloadFsc3 upDown = new UploadDownloadFsc3(user);
 //                   upDown.CreateItemItemRevDataset(itemId, revisionId);
//                    upDown.AddNamedReference();
 //                   upDown.DownloadAttachedFile();

/*                    Teamcenter.Soa.Client.FileManagementUtility fmu = new Teamcenter.Soa.Client.FileManagementUtility(getConnection());
                    ModelObject mo = (ModelObject)itemAndRelObjInfo[0];
                    GetFileResponse res = fmu.GetFileToLocation(mo, "C://WORK/TCDOWNLOAD.FILE.PDF", null, null);

                    System.IO.FileInfo[] files = res.GetFiles();

                    Console.WriteLine("fileLen: " + files.Length);
                    Console.WriteLine("fileName: " + files[0].FullName);

                    for (int j = 0; j < files.Length; j++) {
                        String path = files[j].FullName;
                        Console.WriteLine("filePath: " + path);
                    }
                } catch( Exception e) {
                    Console.WriteLine(e.Message);
                } */
            }

            return response;
        }

        public User login() {
            Teamcenter.Services.Strong.Core.SessionService sessionService = Teamcenter.Services.Strong.Core.SessionService.getService(getConnection());
            try {
                String[] credentials = new string[5];

                while (true) {
                    try {
                        LoginResponse resp = sessionService.Login(Credentials.getUserID(), Credentials.getPassword(), credentials[2], credentials[3], credentials[4]);
                        user = resp.User;
                        return resp.User;
                    } catch (InvalidCredentialsException e ) {
                        credentials = credentialManager.GetCredentials(e);
                    }
                }
            } catch (CanceledOperationException /*e*/) {

            }

            return null;
        }

        public static void setObjectPolicy(DataTable attributes) {
            SessionService sessService = SessionService.getService(getConnection());
            ObjectPropertyPolicy policy = new ObjectPropertyPolicy();

            List<String> bomAttributes1 = new List<String> { };
            List<String> revAttributes1 = new List<string> { };

            int bomAttributeIndex = 0;
            int revAttributeIndex = 0;

            foreach (DataRow attribute in attributes.Rows) {
                if (attribute["ATYPE"].ToString().Equals("BOMLINE")) {
                    String bomAttribute = attribute["ATTRI"].ToString();
                    bomAttributes1.Add(bomAttribute);
                    bomAttributeIndex++;
                } else {
                    String revAttribute = attribute["ATTRI"].ToString();
                    revAttributes1.Add(revAttribute);
                    revAttributeIndex++;
                }
            }

            String[] bomAttributes = new String[bomAttributeIndex];
            String[] revAttributes = new String[revAttributeIndex];

            for (int ai = 0; ai < bomAttributeIndex; ai++) {
                bomAttributes[ai] = bomAttributes1[ai].ToString();
            }
            for (int ai = 0; ai < revAttributeIndex; ai++) {
                revAttributes[ai] = revAttributes1[ai].ToString();
            }

            policy.AddType(new PolicyType("BOMLine", bomAttributes));
            policy.AddType(new PolicyType("ModelObject", new string[] { "item_id", "object_type", "object_name", "object_properties" }));
            policy.AddType(new PolicyType("Item", new string[] { "object_type", "object_name", "bom_view_tags", "revision_list", "item_revision", "vmt9_design_type_cmp", "VMT9_Shn_Tool_RH_Qtys" }));
            policy.AddType(new PolicyType("ItemRevision", revAttributes));
            policy.AddType(new PolicyType("", new string[] { "" }, new string[] { PolicyProperty.WITH_PROPERTIES }));
            sessService.SetObjectPropertyPolicy(policy);
        }
    }
}