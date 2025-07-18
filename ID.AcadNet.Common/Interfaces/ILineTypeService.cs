using Autodesk.AutoCAD.Colors;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.Data.Models.Cad;
using System.Collections.Generic;

namespace Intellidesk.AcadNet.Common.Interfaces
{
    public interface ILineTypeService
    {
        string this[int index] { get; set; }

        void Active(string value);
        object Add(string value);
        object Add(string value, short tColor);
        object Add(string value, Color tColor);
        void AddRange(string[] tListNames, short[] tListColorsIndex);
        void AddRange(string[] tListNames, Color[] tListColors);
        void ChangeLayerNamesDynamically();
        void Clean(string value);
        void CleanAll(string[] layerNames = null);
        bool Contains(string layerName);
        void CreateLayerFilters();
        LineTypeItem Current();
        void DeleteLayerFilter();
        void Freeze(string value, bool tFreeze);
        List<LineTypeItem> GetAll();
        void Init(List<Rule> rules);
        void Init(List<LineTypeItem> listItems = null);
        bool IsOnOff(string value);
        void ListLayerFilters();
        void OnOff(string value, bool tOnOff);
        void Remove(string value);
    }
}