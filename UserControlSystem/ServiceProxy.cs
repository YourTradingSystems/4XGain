﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConsoleTestProject
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClientInfo", Namespace="http://schemas.datacontract.org/2004/07/ClientsInfo")]
    public partial class ClientInfo : object, System.Runtime.Serialization.IExtensibleDataObject
    {
        
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private string ClientBrockerField;
        
        private string ClientBrockerLoginField;
        
        private string ClientEmailField;
        
        private string ClientIdField;
        
        private int ClientsListenedSystemIdField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData
        {
            get
            {
                return this.extensionDataField;
            }
            set
            {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ClientBrocker
        {
            get
            {
                return this.ClientBrockerField;
            }
            set
            {
                this.ClientBrockerField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ClientBrockerLogin
        {
            get
            {
                return this.ClientBrockerLoginField;
            }
            set
            {
                this.ClientBrockerLoginField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ClientEmail
        {
            get
            {
                return this.ClientEmailField;
            }
            set
            {
                this.ClientEmailField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ClientId
        {
            get
            {
                return this.ClientIdField;
            }
            set
            {
                this.ClientIdField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int ClientsListenedSystemId
        {
            get
            {
                return this.ClientsListenedSystemIdField;
            }
            set
            {
                this.ClientsListenedSystemIdField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ClientContinueMode", Namespace="http://schemas.datacontract.org/2004/07/LMAX_Console.ClientsManagerUtilities")]
    public enum ClientContinueMode : int
    {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        OnlyContinue = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Synchronize = 1,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ConsoleTestProject.IExternalCleitnManager")]
    public interface IExternalCleitnManager
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExternalCleitnManager/GetProblematicListenersCount", ReplyAction="http://tempuri.org/IExternalCleitnManager/GetProblematicListenersCountResponse")]
        int GetProblematicListenersCount();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExternalCleitnManager/GetAllProblematicClients", ReplyAction="http://tempuri.org/IExternalCleitnManager/GetAllProblematicClientsResponse")]
        ConsoleTestProject.ClientInfo[] GetAllProblematicClients();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExternalCleitnManager/ContinueClientWork", ReplyAction="http://tempuri.org/IExternalCleitnManager/ContinueClientWorkResponse")]
        void ContinueClientWork(string clientId, int listenedSystemId, ConsoleTestProject.ClientContinueMode clientContinueMode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IExternalCleitnManager/GetVersion", ReplyAction="http://tempuri.org/IExternalCleitnManager/GetVersionResponse")]
        string GetVersion();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IExternalCleitnManagerChannel : ConsoleTestProject.IExternalCleitnManager, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ExternalCleitnManagerClient : System.ServiceModel.ClientBase<ConsoleTestProject.IExternalCleitnManager>, ConsoleTestProject.IExternalCleitnManager
    {
        
        public ExternalCleitnManagerClient()
        {
        }
        
        public ExternalCleitnManagerClient(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
        }
        
        public ExternalCleitnManagerClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public ExternalCleitnManagerClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
        }
        
        public ExternalCleitnManagerClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public int GetProblematicListenersCount()
        {
            return base.Channel.GetProblematicListenersCount();
        }
        
        public ConsoleTestProject.ClientInfo[] GetAllProblematicClients()
        {
            return base.Channel.GetAllProblematicClients();
        }
        
        public void ContinueClientWork(string clientId, int listenedSystemId, ConsoleTestProject.ClientContinueMode clientContinueMode)
        {
            base.Channel.ContinueClientWork(clientId, listenedSystemId, clientContinueMode);
        }
        
        public string GetVersion()
        {
            return base.Channel.GetVersion();
        }
    }
}
