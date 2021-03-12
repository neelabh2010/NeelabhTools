using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;

namespace NeelabhCoreTools
{
    public static class JSONTools
    {
        /// <summary>
        /// Convert JSON string to dynamic type.
        /// <para>How to read result? Below is the code: </para>
        /// <para>var result = jsonString.DynamicJSON()</para>
        /// <para>string status = Convert.ToString(result.status);</para>
        /// <para>string data = Convert.ToString(result.data);</para>
        /// <para>Here "result.status" and "result.data" are assumed as json string parameters</para>
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static dynamic DynamicJSON(this string jsonString)
        {
            dynamic result = JsonConvert.DeserializeObject<dynamic>(jsonString);
            return result;
        }

        public static T JsonToClass<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static string ClassToJson<T>(this T classObject)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, classObject);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }

        public static List<T> JsonToList<T>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<List<T>>(jsonString);
        }

        public static XmlDocument JsonToXml(this string jsonString)
        {
            return JsonConvert.DeserializeXmlNode(jsonString);
        }

        public static string XmlToJson(this string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            return JsonConvert.SerializeXmlNode(doc);
        }

    }
}
