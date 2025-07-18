using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Common.Interfaces;

namespace Intellidesk.AcadNet.Common.Models
{
    /// <summary> Tools read arguments </summary>
    public class ActionArguments : EventArgs
    {
        /// <summary> Current text type of autocad entity </summary>
        public string DxfName { get; set; }

        private bool _filterVisible = true;
        /// <summary> Is entity visible? </summary>
        public bool FilterVisible { get { return _filterVisible; } set { _filterVisible = value; } }

        /// <summary> Results the parser depend of Element Types Filters </summary>
        public IList<Type> FilterTypesOn { get; set; }

        /// <summary> results the parser depend of Element BlockAttributes Filters </summary>
        public string[] FilterAttributesOn { get; set; }

        /// <summary> Is parent type? </summary>
        public bool IsParentFilterTypes { get; set; }

        /// <summary> Is nested entities? </summary>
        public bool IsNested { get; set; }

        /// <summary> LayerPatterns </summary>
        public string[] LayerPatternOn { get; set; }

        /// <summary> Selected objects from curent autocad model space </summary>
        public List<ObjectId> SelectedObjects { get; set; }

        /// <summary> Current position </summary>
        public Point3d Position { get; set; }

        /// <summary> Current Progress meter bar </summary>
        public ProgressMeterBar ProgressMeterBar { get; set; }

        /// <summary> Current progress index </summary>
        public int ProgressIndex { get; set; }

        /// <summary> Rules from configuration </summary>
        public ObservableCollection<IRule> Rules { get; set; }
    }
}