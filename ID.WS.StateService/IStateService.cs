using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ID.WS.StateService
{
    [DataContract]
    public class AcadNetArgs
    {
        [DataMember]
        string AsMade { get; set; }
        [DataMember]
        string FileName { get; set; }
        [DataMember]
        string Lat { get; set; }
        [DataMember]
        string Lon { get; set; }
    }

    [DataContract]
    public class StateItem
    {
        [DataMember(Name = "Id")]
        public string Id { get; set; }
        [DataMember(Name = "Coordx")]
        public string Coordx { get; set; }
        [DataMember(Name = "Coordy")]
        public string Coordy { get; set; }
        [DataMember(Name = "Name")]
        public string Name { get; set; }
        [DataMember(Name = "Status")]
        public string Status { get; set; }
        [DataMember(Name = "As_made_nu")]
        public string As_made_nu { get; set; }
        [DataMember(Name = "Shem_autoc")]
        public string Shem_autoc { get; set; }
        [DataMember(Name = "Kablan")]
        public string Kablan { get; set; }
        [DataMember(Name = "Data_Search")]
        public string Data_Search { get; set; }
        [DataMember(Name = "Longitude")]
        public string Longitude { get; set; }
        [DataMember(Name = "Latitude")]
        public string Latitude { get; set; }
    }

    //[CollectionDataContract(ItemName = "As_mades", KeyName = "As_made_nu", ValueName = "Shem_autoc")]
    //public class AsmadeDictionary : IDictionary<string, StateItem> { }

    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.(UriTemplate = "SetData?f={args}")
    [ServiceContract(Namespace = "ID.WS.StateServices")]
    [ServiceKnownType(typeof(int))]
    [ServiceKnownType(typeof(string))]
    [ServiceKnownType(typeof(StateItem))]
    public interface IStateService
    {
        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml)]
        string ClearData();

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml)]
        List<StateItem> GetData();

        [OperationContract]
        //[WebGet][WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        //[WebInvoke(Method="GET", RequestFormat=WebMessageFormat.Json, ResponseFormat=WebMessageFormat.Json)]
        //[WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        string SetData(StateItem args);
    }
}
