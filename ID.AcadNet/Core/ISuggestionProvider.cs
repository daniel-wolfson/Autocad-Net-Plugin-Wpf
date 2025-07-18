

using System.Collections;

namespace Intellidesk.AcadNet.Core
{
    public interface ISuggestionProvider
    {

        #region Public Methods

        IEnumerable GetSuggestions(string filter);

        #endregion Public Methods

    }
}
