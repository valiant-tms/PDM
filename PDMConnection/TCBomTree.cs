using System;
using System.Data;
using System.Collections.Generic;

using Teamcenter.Soa.Client;
using Teamcenter.Soa.Common;
using Teamcenter.Services.Strong.Core;
using Teamcenter.Soa.Exceptions;
using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Client.Model.Strong;
using Teamcenter.Services.Strong.Cad._2007_01.StructureManagement;


namespace PDMConnection {

    class TCBomTree {
        private String ItemUID;
        private String ItemRevUID;

        private Teamcenter.Soa.Client.Connection connection = null;
        private DataTable attributes = null;
        private DataManagementService dmService = null;
        private StructureManagementService smService = null;
        private ServiceData serviceData = null;

        private Item item = null;
//        private ItemRevision itemRevision = null;
        private DataTable BOMTable = null;

        public String ItemUID1 { get => ItemUID; set => ItemUID = value; }
        public String ItemRevUID1 { get => ItemRevUID; set => ItemRevUID = value; }

        public TCBomTree(Teamcenter.Soa.Client.Connection connection, DataTable attributes) {
            setConnection(connection);
            setAttributes(attributes);

            initialize();
        }

        private DataTable getAttributes() {
            return attributes;
        }
        private DataTable getBOMTable() {
            if (BOMTable == null) {
                BOMTable = new DataTable();
            }
            return BOMTable;
        }

        public DataTable getBOMStructure() {
            try {
                ModelObject[] itemRevs = getItem().Revision_list;
                ModelObject[] bomViews = getItem().Bom_view_tags;

                foreach (ModelObject bomView in itemRevs) {
                    foreach (ModelObject itemRev in itemRevs) {
                        if (ItemRevUID == itemRev.Uid) {
                            CreateBOMWindowsInfo bomWinInfo = new CreateBOMWindowsInfo();
                            bomWinInfo.Item    = getItem();
                            bomWinInfo.ItemRev = (ItemRevision)itemRev;
                            bomWinInfo.BomView = bomView as BOMView;

                            CreateBOMWindowsResponse bomResponse = getSMService().CreateBOMWindows(new CreateBOMWindowsInfo[] { bomWinInfo });
                            if (bomResponse.Output.Length > 0) {
                                BOMLine bomLine = bomResponse.Output[0].BomLine;
                                expandBOMLines(bomLine);
                            }
                        }
                        break;
                    }
                }
            } catch (NotLoadedException e) {
                    
            }
            return getBOMTable();
        }

        private void expandBOMLines(BOMLine bomLine) {
            ExpandPSOneLevelInfo levelInformation = new ExpandPSOneLevelInfo();
            ExpandPSOneLevelPref levelPreferences = new ExpandPSOneLevelPref();

            levelInformation.ParentBomLines = new BOMLine[] { bomLine };
            levelInformation.ExcludeFilter = "None";
            levelPreferences.ExpItemRev = false;
            levelPreferences.Info = new RelationAndTypesFilter[0];

            ExpandPSOneLevelResponse levelResponse = getSMService().ExpandPSOneLevel(levelInformation, levelPreferences);
            ExpandPSOneLevelOutput lo = levelResponse.Output[0];
            try {
                LoadProperties(bomLine);
                fillBomTable(bomLine);
 //               ModelObject[] childs = null; ;
 //               childs = bomLine.Bl_child_item;
                Console.WriteLine(bomLine.Bl_item_item_id);
            } catch (NotLoadedException e) {

            }

            if (levelResponse.Output.Length > 0) {
                foreach (ExpandPSOneLevelOutput levelOutput in levelResponse.Output) {
                    foreach ( ExpandPSData psData in levelOutput.Children) {
                        expandBOMLines(psData.BomLine);
                    }
                }
            }
        }

        private void fillBomTable(BOMLine bomLine) {
            Console.WriteLine(bomLine.Bl_item_item_id);
            DataRow row = getBOMTable().NewRow();

            try {
                if (bomLine.Bl_parent is null) {
                    row["PARENT_ID"] = "";
                } else {
                    row["PARENT_ID"] = ((BOMLine)bomLine.Bl_parent).Bl_item_fnd0objectId;
                }
            } catch (Exception exc) {
                row["PARENT_ID"] = "";
            }

            foreach (DataRow attribute in getAttributes().Rows) {
                try {
                    switch (attribute["ATTRI"].ToString().ToUpper()) {
                        case "BL_CLONE_STABLE_OCCURRENCE_ID":
                            row["BL_CLONE_STABLE_OCCURRENCE_ID"] = bomLine.Bl_clone_stable_occurrence_id;
                            break;
                        case "BL_OCC_VMT9_LH_OPP_QTY":
                           row["BL_OCC_VMT9_LH_OPP_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_LH_Opp_Qty");
                           break;
                        case "BL_OCC_VMT9_LH_SHN_QTY":
                            row["BL_OCC_VMT9_LH_SHN_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_LH_Shn_Qty");
                            break;
                        case "BL_OCC_VMT9_RH_SHN_QTY":
                            row["BL_OCC_VMT9_RH_SHN_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_RH_Shn_Qty");
                            break;
                        case "BL_OCC_VMT9_RH_OPP_QTY":
                            row["BL_OCC_VMT9_RH_OPP_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_RH_Opp_Qty");
                            break;
                        case "BL_OCC_VMT9_S_OPP_QTY":
                            row["BL_OCC_VMT9_S_OPP_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_S_Opp_Qty");
                            break;
                        case "BL_OCC_VMT9_S_SHN_QTY":
                            row["BL_OCC_VMT9_S_SHN_QTY"] = bomLine.GetPropertyDisplayableValue("bl_occ_vmt9_S_Shn_Qty");
                            break;
                        case "BL_FORMATTED_PARENT_NAME":
                            row["BL_FORMATTED_PARENT_NAME"] = bomLine.Bl_formatted_parent_name;
                            break;
                        case "BL_IS_LAST_CHILD":
                            row["BL_IS_LAST_CHILD"] = bomLine.Bl_is_last_child;
                            break;
                        case "BL_ITEM_FND0OBJECTID":
                            row["BL_ITEM_FND0OBJECTID"] = bomLine.Bl_item_fnd0objectId;
                            break;
                        case "BL_ITEM_ITEM_ID":
                            row["BL_ITEM_ITEM_ID"] = bomLine.Bl_item_item_id;
                            break;
                        case "BL_OCC_NOTES_REF":
                            row["bl_OCC_NOTES_REF"] = bomLine.Bl_occ_notes_ref;
                            break;
                        case "AIE_OCC_ID":
                            row["AIE_OCC_ID"] = bomLine.AIE_OCC_ID;
                            break;
                        case "BL_NOTE_TYPES":
                            row["BL_NOTE_TYPES"] = bomLine.Bl_note_types;
                            break;
                        case "BL_OCCURRENCE":
                           Teamcenter.Soa.Client.Model.Property PP = bomLine.Bl_occurrence.GetProperty("VMT9_Shn_Tool_LH_Qty");
                            break;
                        case "BL_OCC_OCC_THREAD":
                            String a = "";
                            break;
                        case "BL_ITEM_OBJECT_NAME":
                            row["BL_ITEM_OBJECT_NAME"] = bomLine.Bl_occ_occ_type;
                            break;
                        case "BL_ITEM_UOM_TAG":
                            row["BL_ITEM_UOM_TAG"] = bomLine.Bl_item_uom_tag;
                            break;
                        case "BL_LEVEL_STARTING_0":
                            row["BL_LEVEL_STARTING_0"] = bomLine.Bl_level_starting_0;
                            break;
                        case "BL_LINE_NAME":
                            row["BL_LINE_NAME"] = bomLine.Bl_line_name;
                            break;
                        case "BL_QUANTITY":
                            if (bomLine.Bl_quantity.Equals("")) {
                                row["BL_QUANTITY"] = "1";
                            } else {
                                row["BL_QUANTITY"] = bomLine.Bl_quantity;
                            }
                            break;
                        case "BL_PARENT":
                            row["BL_PARENT"] = bomLine.Bl_parent;
                            break;
                        case "BL_REV_FND0OBJECTID":
                            row["BL_REV_FND0OBJECTID"] = bomLine.Bl_rev_fnd0objectId;
                            break;
                        case "BL_REV_ITEM_REVISION_ID":
                            row["BL_REV_ITEM_REVISION_ID"] = bomLine.Bl_rev_item_revision_id;
                            break;
                        case "BL_SEQUENCE_NO":
                            row["BL_SEQUENCE_NO"] = bomLine.Bl_sequence_no;
                            break;
                        case "ITEM_ID":
                            row["ITEM_ID"] = bomLine.Bl_item_item_id;
                            break;
                        case "OBJECT_NAME":
                            row["OBJECT_NAME"] = bomLine.Bl_item_object_name;
                            break;
                        case "ITEM_REVISION_ID":
                            row["ITEM_REVISION_ID"] = bomLine.Bl_rev_item_revision_id;
                            break;
                        case "OBJECT_TYPE":
                            row["OBJECT_TYPE"] = bomLine.Bl_item_object_type;
                            break;
                        default:
//                            Console.WriteLine(attribute["ATTRI"].ToString());
                            break;
                    }
                } catch (Exception e) {
                    Console.WriteLine("Error loading attibute: " + attribute["ATTRI"].ToString().ToUpper() + " for " + bomLine.Bl_item_item_id);
                }
            }

            getBOMTable().Rows.Add(row);
        }

        private Teamcenter.Soa.Client.Connection getConnection() {
            return connection;
        }

        private Item getItem() {
            return item;
        }

        //       private ItemRevision getItemRevision() {
        //            return itemRevision;
        //       }

        private DataManagementService getDMService() {
            return DataManagementService.getService(getConnection());
        }

        private FileManagementService getFMServices() {
            return Teamcenter.Services.Strong.Core.FileManagementService.getService(getConnection());
        }

        private ServiceData getServiceData() {
            if (serviceData == null) {
                serviceData = getDMService().LoadObjects(new string[] { ItemUID1 });
            }
            return serviceData;
        }

        private Teamcenter.Services.Strong.Cad.StructureManagementService getSMService() {
            return Teamcenter.Services.Strong.Cad.StructureManagementService.getService(getConnection());
        }

        private void initialize() {
            setDMService(DataManagementService.getService(getConnection()));
            setSMService(StructureManagementService.getService(getConnection()));

            getBOMTable().TableName = "BOM_LIST_TABLE";
            setColumns();
        }

        public bool loadItem() {
            if (getServiceData().sizeOfPlainObjects() > 0) {
                ModelObject modelObject = getServiceData().GetPlainObject(0);
                if (modelObject is Item) {
                    setItem((Item)modelObject);
                    getDMService().GetProperties(new ModelObject[] { modelObject }, new string[] {
                        "bom_view_tags", "revision_list" });

                    return true;
                }
            }
            return false;
        }

        private void LoadProperties(BOMLine bomLine) {
            List<String> bomAttributes1 = new List<String> { };

            int bomAttributeIndex = 0;
            foreach (DataRow attribute in getAttributes().Rows) {
                if (attribute["ATYPE"].ToString().Equals("BOMLINE")) {
                    String bomAttribute = attribute["ATTRI"].ToString();
                    bomAttributes1.Add(bomAttribute);
                    bomAttributeIndex++;
                }
            }
            String[] bomAttributes = new string[bomAttributeIndex];

            for (int aIndex = 0; aIndex < bomAttributeIndex; aIndex++) {
                bomAttributes[aIndex] = bomAttributes1[aIndex].ToString();
            }

            getDMService().GetProperties(new ModelObject[] { bomLine }, bomAttributes);
        }

        private void setAttributes(DataTable attributes) {
            this.attributes = attributes;
        }

        private void setColumns() {
            DataColumn column;

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "PARENT_ID";
            column.ReadOnly = false;
            column.Unique = false;
            getBOMTable().Columns.Add(column);

            foreach (DataRow attribute in getAttributes().Rows) {
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = attribute["ATTRI"].ToString().ToUpper();
                column.ReadOnly = false;
                column.Unique = false;
                getBOMTable().Columns.Add(column);
            }
        }

        private void setConnection(Teamcenter.Soa.Client.Connection connection) {
            this.connection = connection;
        }

        private void setDMService(DataManagementService dmService) {
            this.dmService = dmService;
        }

        private void setItem(Item item) {
            this.item = item;
        }

        private void setSMService(StructureManagementService smService) {
            this.smService = smService;
        }
    }
}