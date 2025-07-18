using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

namespace ID.Infrastructure.Extensions
{
    public static class HtmlHelperExtensions
    {
        private static readonly Lazy<GeneralLocalizer<IAppConfig>> _localizer = new Lazy<GeneralLocalizer<IAppConfig>>(() =>
        {
            var localizer = GeneralContext.GetService<GeneralLocalizer<IAppConfig>>();
            return localizer;
        });
        public static GeneralLocalizer<IAppConfig> CurrentLocalizer => _localizer.Value;

        /// <summary> Add tag script wrapper </summary>
        /// /// <param name="tagMode">TagRenderMode: start, end</param>
        public static HtmlString AddScriptTag(this IHtmlHelper helper, TagRenderMode tagMode)
        {
            TagBuilder builder = new TagBuilder("script");
            builder.MergeAttribute("type", "text/javascript");
            builder.TagRenderMode = tagMode;

            var writer = new System.IO.StringWriter();
            builder.WriteTo(writer, HtmlEncoder.Default);
            return new HtmlString(writer.ToString());
        }

        /// <summary> Create javascript value </summary>
        /// <param name="data">value that will pass to javascript</param>
        /// <returns>javascript string</returns>
        public static HtmlString Json(this IHtmlHelper helper, object data)
        {
            var htmlStringResult = helper.Json(PageRender.None, data, false);
            return htmlStringResult;
        }

        /// <summary> Create javascript value as varName </summary>
        /// <param name="data">value that will pass to javascript</param>
        /// <returns>javascript string</returns>
        public static HtmlString Json(this IHtmlHelper helper, string varName, object data)
        {
            var htmlStringResult = helper.Json("page", varName, data, false);
            return htmlStringResult;
        }

        /// <summary> Create javascript value as varName into named pageModel</summary>
        /// <param name="data">value that will pass to javascript</param>
        /// <returns>javascript string</returns>
        public static HtmlString Json(this IHtmlHelper helper, PageRender varName, string pageModelName, object data)
        {
            var htmlStringResult = helper.Json(pageModelName, varName.ToString(), data, false);
            return htmlStringResult;
        }

        /// <summary> Create javascript value as varName into named pageModel with tagScript</summary>
        /// <param name="data">value that will pass to javascript</param>
        /// <returns>javascript string</returns>
        public static HtmlString Json(this IHtmlHelper helper, PageRender varName, object data, bool withTagScript = false)
        {
            var ownerName = varName == PageRender.Model ? data.ToString() : "page";
            return helper.Json(ownerName, varName.ToString(), data, withTagScript);
        }

        #region privates

        private static HtmlString Json(this IHtmlHelper helper, string varOwnerName, string varName, object data, bool withTagScript = false)
        {
            HtmlString htmlStringResult;
            StringBuilder builder = new StringBuilder();
            string comment = "";

            string _varOwnerName;
            if (!string.IsNullOrEmpty(varOwnerName))
                _varOwnerName = $"\r\t${varOwnerName.Replace("$", "")}";
            else
                _varOwnerName = "\r\t$page";


            // set first character of string lowercase
            //if (!string.IsNullOrEmpty(varName) && char.IsUpper(varName[0]))
            //    varName = char.ToLower(varName[0]) + varName.Substring(1);

            try
            {
                PageRender eVarName = PageRender.None;
                string json;
                if (Enum.TryParse(varName, true, out eVarName))
                {
                    //varName = $"{_varOwnerName}";
                    //varName = $"{varName} = ";
                    switch (eVarName)
                    {
                        case PageRender.Model:
                            varName = $"\r\tvar {_varOwnerName} = ";
                            comment = "// auto-generated from c# page model name";
                            json = data != null ? json = JsonConvert.SerializeObject(GetDictionary(data)) : "{}";
                            break;
                        case PageRender.Resources:
                            varName = $"{_varOwnerName}.{eVarName.GetDisplayName()} = ";
                            comment = "// auto-generated from c# resource type " + CurrentLocalizer.ResourceType.Name;
                            var dataList = new List<string>(data as string[]);
                            json = JsonConvert.SerializeObject(helper.GetResources(dataList.Distinct().ToList()));
                            break;

                        case PageRender.None:
                        case PageRender.Data:
                        case PageRender.Config:
                            if (eVarName != PageRender.None)
                            {
                                varName = $"{_varOwnerName}.{eVarName.GetDisplayName()} = ";
                                comment = "// auto-generated from c# data";
                            }
                            else
                                varName = "";

                            json = JsonConvert.SerializeObject(GetDictionary(data));

                            if (data.GetType() == typeof(bool))
                                json = json.ToLower();
                            break;

                        case PageRender.Properties:
                        default:
                            varName = $"{_varOwnerName}";
                            comment = "// auto-generated from c# page properties";
                            json = helper.AddProperties(varName, data);
                            varName = "";
                            break;
                    }
                }
                else
                {
                    comment = $"// auto-generated from c# with custom name";
                    json = helper.AddProperties($"{varName}", data);
                    varName = $"{varName.Replace("\r\t", "")} = {{}};";
                }

                json =
                    $"{(!string.IsNullOrEmpty(comment) ? "\r\t" + comment : "")}" +
                    $"{(!string.IsNullOrEmpty(varName) ? "\r\t" + varName : "")}" +
                    $"{json}";

                if (withTagScript)
                    builder.AppendFormat("<script type='text/javascript'>\t\t{0}\r\t</script>", json);
                else
                    builder.Append($"{json}");

                htmlStringResult = new HtmlString(builder.ToString());
            }
            catch (Exception ex)
            {
                Log.Logger.ErrorEx(ex);
                var value = !data.GetType().IsValueType ? "{}" : "''";
                htmlStringResult = new HtmlString($"{varName} = {value}");
            }

            return htmlStringResult;
        }

        private static string AddProperties(this IHtmlHelper helper, string varName, object data)
        {
            StringBuilder sb = new StringBuilder();

            var props = GetDictionary(data);
            //var props0 = GetDic(data);
            //var props1 = helper.deserializeToDictionary(JsonConvert.SerializeObject(data));

            foreach (var prop in props)
            {
                string value = null;
                //string value = prop.Value.ToString().Replace("\r\n", "").Replace("  ", " ").Replace("True", "true");
                if (!prop.Value.GetType().IsValueType)
                {
                    if (prop.Value.GetType() == typeof(string))
                    {
                        if (prop.Value.ToString().Contains("function"))
                            sb.Append($"\t{varName}.{prop.Key} = {prop.Value.ToString()};");
                        else
                            sb.Append($"\t{varName}.{prop.Key} = \"{prop.Value.ToString()}\";");
                    }
                    else
                    {
                        sb.Append($"\t{varName}.{prop.Key} = {JsonConvert.SerializeObject(prop.Value)};");
                        //sb.Append($"\t{withVarName.ToLower()}.{prop.Key} = {{}};");
                        //foreach (var propValue in prop.Value.ToDictionary<object>())
                        //{
                        //    sb.Append($"\t{withVarName.ToLower()}.{prop.Key}.{propValue.Key} = {propValue.Value};");
                        //}
                    }
                }
                else
                {
                    value = helper.Encode(prop.Value.ToString());
                    if (prop.Value.GetType() == typeof(bool))
                        value = value.ToLower();
                    sb.Append($"\t{varName}.{prop.Key} = {value};");
                }
            }

            string jsonData = sb.ToString();
            return jsonData;
        }

        private static Dictionary<string, object> GetResources(this IHtmlHelper helper, IList<string> resources)
        {
            Dictionary<string, object> dataItems = new Dictionary<string, object>();
            if (resources != null)
            {
                foreach (var key in resources)
                {
                    var localizedString = CurrentLocalizer.GetString(key);
                    dataItems.Add(key, localizedString.Value);
                }
            }
            return dataItems;
        }

        private static Dictionary<string, object> deserializeToDictionary(this IHtmlHelper helper, string data)
        {
            var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            var values2 = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> d in values)
            {
                if (d.Value != null && d.Value.GetType().FullName.Contains("Newtonsoft.Json.Linq.JObject"))
                {
                    values2.Add(d.Key, helper.deserializeToDictionary(d.Value.ToString()));
                }
                else
                {
                    values2.Add(d.Key, d.Value);
                }
            }
            return values2;
        }

        private static IDictionary<string, object> DeserializeData(this IHtmlHelper helper, JObject data)
        {
            var dict = data.ToObject<Dictionary<string, object>>();

            return helper.DeserializeData(dict);
        }

        private static IDictionary<string, object> DeserializeData(this IHtmlHelper helper, IDictionary<string, object> data)
        {
            foreach (var key in data.Keys.ToArray())
            {
                var value = data[key];

                if (value is JObject)
                    data[key] = helper.DeserializeData(value as JObject);

                if (value is JArray)
                    data[key] = helper.DeserializeData(value as JArray);
            }

            return data;
        }

        private static IList<object> DeserializeData(this IHtmlHelper helper, JArray data)
        {
            var list = data.ToObject<List<object>>();

            for (int i = 0; i < list.Count; i++)
            {
                var value = list[i];

                if (value is JObject)
                    list[i] = helper.DeserializeData(value as JObject);

                if (value is JArray)
                    list[i] = helper.DeserializeData(value as JArray);
            }

            return list;
        }

        private static string GetValue(this IHtmlHelper helper, object data)
        {
            return helper.Encode(data.ToString());
        }

        #region privates temp

        private static Dictionary<string, object> GetDictionary(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return dictionary;
        }

        #endregion

        #endregion
    }
}
