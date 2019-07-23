using System;

using Teamcenter.Soa.Client.Model;

namespace PDMConnection.ClientX {

    public class AppXPartialErrorListener : PartialErrorListener {


        public void HandlePartialError(ErrorStack[] stacks) {
            if (stacks.Length == 0 ) {
                return;
            }

            Console.WriteLine("");
            Console.WriteLine("*****");
            Console.WriteLine("Partial Errors caught in com.teamcenter.clientx.AppXPartialErrorListener.");

            for (int i = 0; i < stacks.Length; i++) {
                ErrorValue[] errors = stacks[i].ErrorValues;
                Console.Write("Partial Error for ");

                if (stacks[i].HasAssociatedObject()) {
                    Console.WriteLine("object " + stacks[i].AssociatedObject.Uid);
                } else
                if (stacks[i].HasClientId()) {
                    Console.WriteLine("client id " + stacks[i].ClientId);
                } else
                if (stacks[i].HasClientIndex()) {
                    Console.WriteLine("client index " + stacks[i].ClientIndex);
                }

                for (int j = 0; j < errors.Length; j++) {
                    Console.WriteLine("    Code: " + errors[j].Code + "\tSeverity: " +
                                      errors[j].Level + "\t" + errors[j].Message);
                }
            }
        }
    }
}

