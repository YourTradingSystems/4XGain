using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using ClientsInfo;

namespace ClientsManagerUtilities
{
    public enum ClientContinueMode { OnlyContinue, Synchronize };

    [ServiceContract]
    public interface IExternalCleitnManager
    {
        [OperationContract]
        int GetProblematicListenersCount();

        [OperationContract]
        List<ClientInfo> GetAllProblematicClients();
        //String GetAllProblematicClients();

        [OperationContract]
        void ContinueClientWork(String clientId, Int32 listenedSystemId, ClientContinueMode clientContinueMode);

        [OperationContract]
        String GetVersion();
    }
}
