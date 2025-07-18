using System.Net;
using System.Net.Http;
using System.Web.Http;
using Intellidesk.Data.Models.Dto;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.WebApi
{
    /// <summary> SysVariableController </summary>
    public class ManageController : AcadActionController
    {
        /// <summary> Get </summary>
        public string Get(string varName)
        {
            try
            {
                object varValue = App.GetSystemVariable(varName);
                return varValue.ToString();
            }
            catch
            {
                return "Invalid SYSTEM VARIABLE name: \"" + varName + "\"";
            }
        }

        public string Get()
        {
            return "Please supply SYSTEM VARIABLE name!";
        }

        public HttpResponseMessage Put([FromBody]AcadSysVar sysVar)
        {
            HttpStatusCode code = HttpStatusCode.Accepted;
            ActionMessage.Length = 0;
            if (sysVar == null)
            {
                code = HttpStatusCode.ExpectationFailed;
                ActionMessage.Append("SysVar argument is not supplied.");
            }
            else
            {
                if (!UpdateSystemVariable(sysVar))
                {
                    code = HttpStatusCode.ExpectationFailed;
                }
            }
            return Request.CreateResponse(code, ActionMessage.ToString());
        }

        public string GetVar(string varName)
        {
            try
            {
                object varValue = App.GetSystemVariable(varName);
                return varValue.ToString();
            }
            catch
            {
                return "Invalid SYSTEM VARIABLE name: \"" + varName + "\"";
            }
        }

        public HttpResponseMessage SetVar([FromBody]AcadSysVar sysVar)
        {
            ActionMessage.Length = 0;
            HttpStatusCode code = HttpStatusCode.Accepted;

            if (sysVar == null)
            {
                code = HttpStatusCode.ExpectationFailed;
                ActionMessage.Append("SysVar argument is not supplied.");
            }
            else
            {
                if (!UpdateSystemVariable(sysVar))
                {
                    code = HttpStatusCode.ExpectationFailed;
                }
            }

            if (code != HttpStatusCode.Accepted)
                return Request.CreateErrorResponse(code, ActionMessage.ToString());
            return Request.CreateResponse(code, ActionMessage.ToString());
        }

        private bool UpdateSystemVariable(AcadSysVar sysVar)
        {
            try
            {
                using (Doc.LockDocument())
                {
                    App.SetSystemVariable(sysVar.Name, sysVar.Value);
                }

                ActionMessage.Append("System variable \"" + sysVar.Name + "\" is updated successfully.");
                return true;
            }
            catch (System.Exception ex)
            {
                ActionMessage.Append("Setting system variable \"" + sysVar.Name + "\" failed:\n" + ex.Message);
                return false;
            }
        }
    }
}
