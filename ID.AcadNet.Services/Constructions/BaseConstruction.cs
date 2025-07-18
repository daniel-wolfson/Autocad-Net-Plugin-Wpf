using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Tools.Constructions
{
	public class SupportList : System.ComponentModel.StringConverter
	{
		string[] _Status = new string[] {
			"Joint",
			"Fixed",
			"Free"
			//Boundary("Joint"), Boundary("Fixed"), Boundary("Free")
		};

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{
			return new StandardValuesCollection(_Status);
		}
		public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}
		public override bool GetStandardValuesExclusive(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}
	}

	[DefaultProperty("Title")]
	public class BaseConstruction  //DataManager
	{
        public Document Doc = Application.DocumentManager.MdiActiveDocument;
        public Database Db = Doc.Database;
        public Editor Ed = Doc.Editor;
        public bool isProjectTest = My.MySettings.Default.isProjectTest;
	        //My.MySettings.Default.RootPath 
        public string RootPath = Directory.GetCurrentDirectory() + "\\";

        public virtual string NameOfManager()
        {
	        return "Base Manager" + ".";
        }

        public virtual void DocSpaceInit(ref Document Doc, ref Database Db, ref Editor Ed)
        {
	        Doc = Application.DocumentManager.MdiActiveDocument;
	        Db = Doc.Database;
	        Ed = Doc.Editor;
        }
		public override string NameOfManager()
		{
			return "DataSet Manager";
		}

		#region "Public Const"
		public struct FieldsFormat
		{
			public const string Name = "floor-*";
			public const string Num = "0#";
			public const string elevation = "0.##";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldsName
		{
			public const string Name = "Name";
			public const string Num = "Num";
			public const string elevation = "elevation";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldsPmt
		{
			// Prompt
			public const string Name = "Floor name";
			public const string Num = "Floor number";
			public const string elevation = "Elevation";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldsCmt
		{
			//Comment
			public const string Name = "Name in format: floor[1]-[username]";
			public const string Num = "Span of the plate in Y direction - 'Num' (m)";
			public const string elevation = "Elevation";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldsVal
		{
			//Value Default
			public const string Name = "floor";
			public const string  Num = "0";
			public const string elevation = "0.00";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct Fields
		{
			//Value Default
			public const string Name = "floor";
			public const int Num = 0;
			public const double elevation = 0.0;
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldsType
		{
			public const string Name = "String";
			public const string Num = "Integer";
			public const string elevation = "Double";
			public static string[] ToList = {
				Name,
				Num,
				elevation
			};
		}

		public struct FieldId
		{
			public const int Name = 0;
			public const int Num = 1;
			public const int elevation = 2;
			public static int[] ToList = {
				Name,
				Num,
				elevation
			};
		}
		#endregion

		public class DataSelectedObject
		{
			public bool isDataCorrect;
			public List<string> BlockNames;
			public List<string> Names;
			public List<string> Values;
			public List<string> Types;
			public List<string> Defaults;
			public List<string> Prompts;
			public List<string> Comments;
			public List<string> Formats = new List<string>();

			public List<ObjectId> ObjectIds = new List<ObjectId>();
			//Public Property Names(ByVal i As Integer) As String
			//    Get
			//        Return lstNames(i)
			//    End Get
			//    Set(ByVal Value As String)
			//        lstNames(i) = Value
			//    End Set
			//End Property
			//Public Property Values(ByVal i As Integer) As String
			//    Get
			//        Return lstValues(i)
			//    End Get
			//    Set(ByVal Value As String)
			//        lstValues(i) = Value
			//    End Set
			//End Property
			//Public Property Values(ByVal i As Integer) As String
			//    Get
			//        Return lstValues(i)
			//    End Get
			//    Set(ByVal Value As String)
			//        lstValues(i) = Value
			//    End Set
			//End Property


			public DataSelectedObject(string[] tFieldNames, object[] tFieldVals, string[] tFieldTypes, string[] tFieldPromts, string[] tFieldComments, string[] tFieldFormats)
			{

				if (tFieldNames.Length == tFieldVals.Length & tFieldNames.Length == tFieldTypes.Length & tFieldNames.Length == tFieldPromts.Length & tFieldNames.Length == tFieldComments.Length) {
					Names.Clear();
					Values.Clear();
					Defaults.Clear();
					Prompts.Clear();
					Comments.Clear();
					Types.Clear();
					Formats.Clear();
					ObjectIds.Clear();

					Names.AddRange(tFieldNames);
					Values.AddRange(tFieldVals);
					Defaults.AddRange(tFieldVals);
					Prompts.AddRange(tFieldPromts);
					Comments.AddRange(tFieldComments);
					Types.AddRange(tFieldTypes);
					Formats.AddRange(tFieldFormats);

					for (var i = 0; i <= Values.Count - 1; i++) {
						ObjectIds.Add(ObjectId.Null);
						BlockNames.Add("");
					}
					isDataCorrect = true;
				} else {
					isDataCorrect = false;
				}
			}
		}

		private static DataSelectedObject dso = new DataSelectedObject(FieldsName.ToList, FieldsVal.ToList, FieldsType.ToList, FieldsPmt.ToList, FieldsCmt.ToList, FieldsFormat.ToList);
		private bool _isBlockFound;
		private new bool isBlockFound {
			get { return _isBlockFound; }
			set { _isBlockFound = value; }
		}

		private string _DataFileType = "xml";
		[Browsable(false)]
		public new string DataFileType {
			get { return _DataFileType; }
			set { _DataFileType = value; }
		}

		#region "Properties DateSetType"
		[CategoryAttribute("X,Y Span:"), DisplayName(FieldsPmt.Name), DefaultValueAttribute(FieldsVal.Name), DescriptionAttribute(FieldsCmt.Name), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Name {
			get { return FieldValues.Item(FieldId.Name); }
				//DoChangeEvent(BlockNames.Item(attrId.Name), TagNames.Item(attrId.Name), Value)
			set { SetPropertyValue(FieldId.Name, value); }
		}
		[CategoryAttribute("X,Y Span:"), DisplayName(FieldsPmt.Num), DefaultValueAttribute(FieldsVal.Num), DescriptionAttribute(FieldsCmt.Num), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Num {
			get { return FieldValues.Item(FieldId.Num); }
				//UpdateAttributes(BlockNames.Item(attrId.Num), TagNames.Item(attrId.Num), Value)
			set { SetPropertyValue(FieldId.Num, value); }
		}
		[Category("elevation:"), DisplayName(FieldsPmt.elevation), DefaultValueAttribute(FieldsVal.elevation), DescriptionAttribute(FieldsCmt.elevation), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string elevation {
			get { return FieldValues.Item(FieldId.elevation); }
			set { SetPropertyValue(FieldId.elevation, value); }
		}
		#endregion

		public DataManager(string tFileName = "") : base(tFileName)
		{
		}

		public override bool LoadDataDefault(string tBlockName = "")
		{
			//Handles cb_blk_attrObj1.LoadAttrDefault_Event
			try {
				base.LoadDataDefault(FieldsName.ToList, FieldsVal.ToList, FieldsType.ToList, FieldsPmt.ToList, FieldsCmt.ToList, FieldsFormat.ToList);
				return true;

			} catch (System.Exception ex) {
				Messager.NewMessage(NameOfManager() + "." + System.Reflection.MethodBase.GetCurrentMethod.Name(), ex.Message) = MsgMode.Err;
				return false;
			}
		}

	}
}
