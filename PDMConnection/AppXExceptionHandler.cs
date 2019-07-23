using System;
using System.IO;

using Teamcenter.Schemas.Soa._2006_03.Exceptions;
using Teamcenter.Soa.Client;
using Teamcenter.Soa.Exceptions;

namespace PDMConnection.ClientX { 

    public class AppXExceptionHandler : ExceptionHandler {

        public void HandleException(InternalServerException ise) {
            Console.WriteLine("");
            Console.WriteLine("*****");
            Console.WriteLine("Exception caught in com.teamcenter.clientx.AppXExceptionHandler.handleException(InternalServerException).");

            if (ise is ConnectionException) {
                Console.Write("\nThe Server returned a connection error.\n" + ise.Message +
                              "\nDo you wish to retry the last service request?[y/n]");
            } else
            if (ise is ProtocolException) {
                Console.Write("\nThe server ereturned a protocol error.\n" + ise.Message +
                              "\nThis is most likely the result of a programming error." +
                              "\nDo you wish to retry the last service request?[y/n]");
            } else {
                Console.WriteLine("\nThe server returned an internal server error.\n" + ise.Message +
                                  "\nThis is most likely the result of a programming error." +
                                  "\nA runtimeException will be thrown.");
                throw new SystemException(ise.Message);
            }

            try {
                String retry = Console.ReadLine();

                if (retry.ToLower().Equals("y") || retry.ToLower().Equals("yes")) {
                    return;
                }

                throw new SystemException("The user has opted not to retry the last request"); 
            } catch (IOException e ) {
                Console.Error.WriteLine("Failed to read user response.\nA RuntimeException will be thrown.");
                throw new SystemException(e.Message);
            }
        }

        public void HandleException(CanceledOperationException coe) {
            Console.WriteLine("");
            Console.WriteLine("*****");
            Console.WriteLine("Exception cought in com.teamcenter.clientx.AppXExceptionHandler.handleException(CanceledOperationException).");

            throw new SystemException(coe.Message);
        }
    }

}