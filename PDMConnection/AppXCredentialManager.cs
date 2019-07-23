using System;

using Teamcenter.Schemas.Soa._2006_03.Exceptions;
using Teamcenter.Soa;
using Teamcenter.Soa.Client;
using Teamcenter.Soa.Exceptions;

namespace PDMConnection.ClientX {

    public class AppXCredentialManager : CredentialManager {
        private String name = null;
        private String password = null;
        private String group = "";
        private String role = "";
        private String discriminator = "SoaAppX";

        public int CredentialType {
            get { return SoaConstants.CLIENT_CREDENTIAL_TYPE_STD; }
        }
        public String[] GetCredentials(InvalidCredentialsException e) {
            Console.WriteLine(e.Message);
            return PromptForCredentials();
        }
        public String[] GetCredentials(InvalidUserException e) {
            // User has not logged in yet, ask the user if this occurs
            if (name == null) {
                return PromptForCredentials();
            }
            string[] tokens = { name, password, group, role, discriminator };
            return tokens;
        }

        public String[] PromptForCredentials() {
            try {
                Console.WriteLine("Please enter user credentials (return to quit):");
                Console.Write("User Name: ");
                name = Console.ReadLine();

                if (name.Length == 0) {
                    throw new CanceledOperationException("");
                }
                Console.Write("Password: ");
                password = Console.ReadLine();
            } catch (InvalidOperationException e) {
                String message = "Failed to get the name and password.\n" + e.Message;
                Console.WriteLine(message);
                throw new CanceledOperationException(message);
            }

            String[] tokens = { name, password, group, role, discriminator };
            return tokens;
        }

        public void SetGroupRole(String group, String role) {
            this.group = group;
            this.role  = role;
        }
        public void SetUserPassword(String user, String password, String discriminator) {
            this.name          = user;
            this.password      = password;
            this.discriminator = discriminator;
        }
    }
}