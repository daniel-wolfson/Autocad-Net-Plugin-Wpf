using System.ComponentModel.DataAnnotations.Schema;

namespace Intellidesk.Data.Repositories.Infrastructure
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState ObjectState { get; set; }
    }
}