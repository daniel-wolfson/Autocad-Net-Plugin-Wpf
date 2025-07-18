using System.ServiceModel;

namespace Intellidesk.AcadNet.Services.Interfaces
{
    [ServiceContract()]
    public interface IMapInfoAcmv
    {
        [OperationContract()]
        double SetViewrXY(double X, double Y);
    }
}
