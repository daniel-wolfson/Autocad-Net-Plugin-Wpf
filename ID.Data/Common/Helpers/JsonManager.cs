using ID.Infrastructure.General;
using Intellidesk.Data.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Intellidesk.Data.Common.Helpers
{
    public class JsonActionManager
    {
        public static SimpleActionResult LoadJsonFileData<T>(string fullPath) where T : MapElement
        {
            //JObject o1 = JObject.Parse(File.ReadAllText(@"c:\videogames.json"));
            //JsonTextReader reader = new JsonTextReader(file)

            const Int32 BufferSize = 128;

            try
            {
                using (var fileStream = File.OpenRead(fullPath))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    string[] lines = streamReader.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                    if (lines.Length < 2) throw new InvalidDataException("Must have header line.");

                    string source = Normal(lines[1]);

                    if (typeof(T) == typeof(MapLineElement))
                    {
                        source = source.Replace("Len", "Length")
                            .Replace("Koter", "Diameter")
                            .Replace("Shem_file", "FileName")
                            .Replace("Date", "EffecDate")
                            .Replace("Kablan", "Contractor")
                            .Replace("Kamut", "FiberCount");
                    }
                    else if (typeof(T) == typeof(MapMarkerElement))
                    {
                        source = source.Replace("Len", "Length")
                            .Replace("Data_Search", "ElementName")
                            .Replace("As_made_nu", "FolderName")
                            .Replace("Shem_autoc", "FileName")
                            .Replace("Date", "EffecDate")
                            .Replace("Kablan", "Contractor");
                    }

                    string[] headers = source.Split(',');

                    List<string> jsonResult = new List<string>();

                    for (int i = 2; i < lines.Length; i++)
                    {
                        string[] fields = Normal(lines[i]).Split(',');

                        if (fields.Length == headers.Length)
                        {
                            var jsonElements = headers.Zip(fields, (header, field) => $"{header}: {field}").ToArray();
                            string jsonObject = "{" + $"{string.Join(",", jsonElements)}" + "}";

                            if (i < lines.Length - 1)
                            {
                                jsonResult.Add(jsonObject.Replace(",:", "")); //JObject.Parse
                            }
                        }
                    }

                    var arr = $"[{string.Join(",", jsonResult)}]";

                    List<T> variables = JsonConvert.DeserializeObject<List<T>>(arr);

                    return new SimpleActionResult
                    {
                        Message = fullPath,
                        StatusCode = HttpStatusCode.Found,
                        ActionResult = variables
                    }; //(JObject)JToken.ReadFrom(reader)
                }
            }
            catch (Exception ex)
            {
                return new SimpleActionResult { Message = ex, StatusCode = HttpStatusCode.NotFound, ActionResult = null };
            }
        }

        public static SimpleActionResult LoadGeoJsonFileData<T>(string fullPath)
        {
            //JObject o1 = JObject.Parse(File.ReadAllText(fullPath));
            JObject o2;
            using (StreamReader streamReader = File.OpenText(fullPath))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                o2 = (JObject)JToken.ReadFrom(reader);
            }
            //var arr = JArray.Parse($"[{sb}]".Replace(",: ", "").Replace("{{", "{").Replace("}}", "}".Replace(",]", "]")));

            JArray blogPostArray = JArray.Parse(o2.ToString());

            //IList<Feature> features = blogPostArray.Select(p => 
            //    new Feature(new LineString(new IPosition[1]))
            //    ).ToList();

            //foreach (var variable in variables)
            //{
            //    Coordinate latLng = WebMercator.itm2gps(
            //        new Point(variable.Latitude.GetValueOrDefault(), variable.Longitude.GetValueOrDefault()));
            //    variable.CoordX = Math.Round(latLng.lat, 8);
            //    variable.CoordY = Math.Round(latLng.lng, 8);
            //}
            //var result = objects.Select(JsonConvert.SerializeObject).ToArray();

            //List<T> variables = JsonConvert.DeserializeObject<List<T>>(arr);

            return new SimpleActionResult
            {
                Message = fullPath,
                StatusCode = HttpStatusCode.Found,
                ActionResult = new { }//features
            };
        }

        public static string Normal(string str)
        {
            return new string(str.Replace("[", "").Replace("]", "").Replace(" ", "").Replace("?", "a").ToCharArray());
        }
    }
}
