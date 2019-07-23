using System;

using Teamcenter.Soa.Client.Model;
using Teamcenter.Soa.Exceptions;

namespace PDMConnection.ClientX {

    public class AppXModelEventListener : ModelEventListener {

        override public void LocalObjectChange(ModelObject[] objects) {
            if (objects.Length == 0) {
                return;
            }
            System.Console.WriteLine("");
            System.Console.WriteLine("Modified Objects handle in com.teamcenter.clientx.AppXUpdateObjectListener.modelObjectChnage");
            System.Console.WriteLine("The following objects have been update in the client data model:");

            for (int i = 0; i < objects.Length; i++) {
                String uid = objects[i].Uid;
                String type = objects[i].GetType().Name;
                String name = "";
                if (objects[i].GetType().Name.Equals("WorkspaceObject")) {
                    ModelObject wo = objects[i];
                    try {
                        name = wo.GetProperty("object_string").StringValue;
                    } catch (NotLoadedException /*e*/) {
                        // Ignored
                    } 
                }
                System.Console.WriteLine("    " + uid + " " + type + " " + name);
            }
        }

        public override void LocalObjectDelete(string[] uids) {
            if (uids.Length == 0) {
                return;
            }

            System.Console.WriteLine("");
            System.Console.WriteLine("Delete Objects handled in com.teamcenter.clientx.AppXDeletedObjectListener.modelObjectDelete");
            System.Console.WriteLine("The following objects have been deleted from the server an removed from the client data model:");
            for (int i = 0; i < uids.Length; i++) {
                System.Console.WriteLine("    " + uids[i]);
            }
        }
    }
}