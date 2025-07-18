using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

namespace ID.WS.StateService
{
    [Serializable]
    public class LatLong
    {
        public double? latitude;
        public double? longitude;
    }

    [Serializable]
    public class MapView
    {
        public LatLong center;
        public int? zoom;
    }

    // NOTE: If you change the class name "StateService" here, you must also update the reference to "StateService" in Web.config and in the associated .svc file.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Single)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class StateService : IStateService
    {
        public static WebChannelFactory<IStateService> ServiceChannelFactory;
        //static public ConcurrentDictionary<string, StateItem> Data = new ConcurrentDictionary<string, StateItem>();

        public StateService()
        {
            var connection = ConfigurationManager.ConnectionStrings["AcadNetContext"];
            //DataManager.CreateContext<AcadNetContext>("", connection.ConnectionString);
        }

        #region IStateService Members

        public List<StateItem> GetData()
        {
            //foreach (var key in HttpContext.Current.Session.Keys) { }
            //Data.TryAdd("qwe", new StateItem() {As_made_nu = "sdsf"});
            var retDic = new List<StateItem>();
            //var Context = DataToolsManager.CreateContext<AcadNetContext>("", connection.ConnectionString);

            //if (DataManager.IsConnectionEnabled())
            //{
            //    var mapinfoContext = (AcadNetContext)DataManager.Context;
            //    retDic = mapinfoContext.States.Select(v => new StateItem { As_made_nu = v.As_made, Shem_autoc = v.FileName })
            //        .ToList();
            //    //    .ToDictionary(x => x.Shem_autoc); //new Dictionary<string, StateItem>();
            //}

            //retDic.Add(1, 1);
            ////Data = HttpContext.Current.Session["Data"] as ConcurrentDictionary<string, StateItem>;
            //if (Data.Count > 0)
            //    foreach (var key in Data.Keys)
            //    {
            //        if (!retDic.ContainsKey(key))
            //            retDic[key] = Data[key];
            ////    }
            //string example = "{'center':{'latitude':'49.266214','longitude':'-122.998577'},'zoom':'12'}";
            //JavaScriptSerializer serializer = new JavaScriptSerializer();

            //// Deserialize
            //MapView view = serializer.Deserialize<MapView>(example);
            ////StringBuilder sb = new StringBuilder();
            ////sb.Append("center = (" + view.center.latitude.ToString() + ", " + view.center.longitude.ToString() + ")" + "<br/>");
            ////sb.Append("zoom = " + view.zoom.ToString());
            //// Serialize

            //string jsonString = serializer.Serialize(view);
            //return jsonString;
            //return HttpContext.Current.Session.SessionID + "; data count" + Data.Count + "; " + data.; //
            return retDic;
        }

        public string SetData(StateItem args)
        {
            ////var jsonSerializer = new JavaScriptSerializer();
            ////var arguments = (IDictionary<string, object>)jsonSerializer.DeserializeObject(args);
            ////var current = jsonSerializer.Deserialize<StateItem>(args);

            String messageResult = string.Empty;
            if (!String.IsNullOrEmpty(args.As_made_nu) && !String.IsNullOrEmpty(args.Shem_autoc))
            {
                ////Data.TryAdd(args.Shem_autoc, args);
                ////var connection = WebConfigurationManager.ConnectionStrings["AcadNetContext"];
                ////DataToolsManager.GetConnectionString<MapinfoContext>();
                ////var Context = DataToolsManager.CreateContext<AcadNetContext>("", connection.ConnectionString);
                
            //    if (DataManager.IsConnectionEnabled())
            //    {
            //        var mapinfoContext = (AcadNetContext)DataManager.Context;
            //        if (!mapinfoContext.States.Any(x => x.FileName == args.Shem_autoc))
            //        {
            //            var state = new State()
            //            {
            //                As_made = args.As_made_nu,
            //                DateCreated = DateTime.Now,
            //                FileName = args.Shem_autoc,
            //                Latitude = float.Parse(args.Latitude),
            //                Longitude = float.Parse(args.Longitude)
            //            };
            //            mapinfoContext.States.Add(state);
            //            mapinfoContext.SaveChanges();
            //            messageResult = "Object added successafuly, count: " + mapinfoContext.States.Count();
            //        }
            //        else
            //        {
            //            messageResult = "Object already is exist, count: " + mapinfoContext.States.Count();
            //        }

            //    }
            //    else
            //    {
            //        messageResult = "Warning! Connection to Mapinfo context not enabled";
            //    }
            }
            //else
            //{
            //    messageResult = "Warning! Shem_autoc is empty.";
            //}
            return messageResult;
        }

        public string ClearData()
        {
            string messageResult = "";
            //if (DataManager.IsConnectionEnabled())
            //{
            //    var mapinfoContext = (AcadNetContext)DataManager.Context;
            //    if (!mapinfoContext.States.Any())
            //    {
            //        mapinfoContext.States.XRemoveFor(x => true);
            //        mapinfoContext.SaveChanges();
            //        messageResult = "Object added successafuly, count: " + mapinfoContext.States.Count();
            //    }
            //}
            //else
            //{
            //    messageResult = "Warning! Connection to Mapinfo context not enabled";
            //}
            return messageResult;
        }

        public static void Start<TChannel, TFunc>(Expression<Func<TChannel, TFunc>> expression)
            where TChannel : class
            where TFunc : class
        {
            var body = (MemberExpression)expression.Body;
            var propertyName = body.Member.Name;

            //var uri = "http://vmmapinfo.partnergsm.co.il/AcadNetGis/AcadNetStateService/StateService.svc";
            var uri = "http://localhost/AcadNetStateService/StateService.svc";
            var webbinding = new WebHttpBinding();

            using (var proxy = new WebChannelFactory<TChannel>(webbinding, new Uri(uri)))
            {
                var webchannel = proxy.CreateChannel();
                //var m = webchannel.GetType().GetMethod(propertyName);
                var m = webchannel.GetType().GetMethod(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                m.Invoke(webchannel, new Object[] {});
            }
        }

        #endregion
    }

    [ServiceContract(Namespace = "ID.WS.StateService")]
    public interface ICalculator
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "Add?x={x}&y={y}", BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        double Add(double x, double y);
    }

    // Service class which implements the service contract.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class CalculatorService : ICalculator
    {
        public double Add(double x, double y)
        {
            return x + y;
        }
    }
}
