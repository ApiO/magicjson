using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MagicJson.Tools
{
    public static class Documentation
    {
        public enum Type
        {
            HTML,
            Wiki,
            Redmine
        }
        
        public static string Generate(JObject jObject, Type convertType)
        {
            var result = string.Empty;
            switch (convertType)
            {
                case Type.HTML:
                    result = FormatJObjectToHTML(jObject);
                    break;
                case Type.Wiki:
                    result = FormatJObjectToWiki(jObject, 0);
                    //http://fr.wikipedia.org/wiki/Aide:Tableau
                    break;
                case Type.Redmine:
                    result = FormatJObjectToRedmine(jObject);
                    //http://www.redmine.org/projects/redmine/wiki/RedmineTextFormatting#Tables
                    break;
            }

            return result;
        }

        private static string FormatJObjectToRedmine(JObject jObject)
        {
            return null;
        }

        private static string FormatJObjectToHTML(IDictionary<string, JToken> jObject)
        {
            var strBuilder = new StringBuilder();

            const string htmlHeader = "<!DOCTYPE html><html><head><style>"+
                            "body{font-family:'Segoe UI',Verdana,Arial;font-size: .813em;}"+
                            "table{border-style: solid;border-width: 1px;border-color: #BBB;border-collapse: collapse;margin-left: 20px}"+
                            "tr:hover{background-color:#E2F3FD;}" +
                            "th{background-color:#E5E5E5;padding:6px;font-style: normal;"+
                            "font-weight: normal;}"+
                            "td{padding:6px;}"+
                            "h2{color: #3F529C;}"+
                            "ul{margin:0;}"+
                            ".bold{font-weight:bold;}"+
                            ".padded{padding-left:20px;}</style></head><body>";
            const string desc = "<p><span class=\"bold\">{0}</span>: {1}</p>";

            const string htmlFooter = "</body></html>";
            
            if (jObject["description"] != null) strBuilder.AppendFormat(desc, "Description", ((JValue)jObject["description"]).Value);
            if (jObject["$schema"] != null) strBuilder.AppendFormat(desc, "Schema", ((JValue)jObject["$schema"]).Value);

            strBuilder.AppendFormat("<h2>Properties</h2>");

            strBuilder.AppendLine(GetHTMLTable(jObject));
            
            var result = string.Format("{0}{1}{2}", htmlHeader, strBuilder, htmlFooter);
            return result;
        }

        private static string GetHTMLTable(IDictionary<string, JToken> jObject)
        {
            var strBuilder = new StringBuilder();

            const string tableHeader = "<table border=\"1\" {0}><thead>" +
                               "<tr><th>Name</th><th>Type</th><th>Default</th><th>Required</th>" +
                               "<th>Values</th><th>minItems</th><th>maxItems</th>" +
                               "<th>Description</th></tr></thead><tbody>";
            const string tableLine = "<tr><td{8}>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>";
            const string tableFooter = "</tbody></table>";
            
            var properties = (JObject)jObject["properties"];

            strBuilder.AppendLine(tableHeader);
            foreach (var property in properties.Properties())
            {
                var name = property.Name;

                var type = property.Value["type"] != null ? FormatType(property.Value["type"].ToString()) : string.Empty;
                var getSubTable = false;
                if (type == "array")
                {
                    if (property.Value["items"]["extends"] == null)//si le type de la propriete n'est pas un objet
                        type += string.Format(" of {0}", property.Value["items"]["type"]);
                    else
                    {
                        var objectType = property.Value["items"]["extends"]["$ref"].ToString();
                        type += " of " + (objectType == "#" ? "himself" : objectType);
                        getSubTable = true;
                    }
                }

                var def = property.Value["default"] != null ? property.Value["default"].ToString() : string.Empty;
                var required = property.Value["required"] != null ? "X" : string.Empty;
                var values = property.Value["enum"] != null ? FormatEnum(property.Value["enum"].ToString()) : string.Empty;
                var minItems = property.Value["minItems"] != null ? property.Value["minItems"].ToString() : string.Empty;
                var maxItems = property.Value["maxItems"] != null ? property.Value["maxItems"].ToString() : string.Empty;
                var description = property.Value["description"] != null ? property.Value["description"].ToString() : string.Empty;

                strBuilder.AppendFormat(tableLine, name, type, def, required, values, minItems, maxItems, description, getSubTable ? " class=\"bold\"" : string.Empty);
                
                JToken obj = property.Value["items"];

                //strBuilder.AppendFormat("", FormatJObjectToHTML() + "");
            }

            strBuilder.AppendLine(tableFooter);

            return strBuilder.ToString();
        }


        private static string FormatEnum(string value)
        {
            return "<ul><li>" + value.Replace(" ", string.Empty).Replace("\"", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace(",", "</li><li>") + "</li></ul>";
        }

        private static string FormatType(string value)
        {
            return value.Replace(" ", string.Empty).Replace("\"", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace(",", " or ");
        }

        private static string FormatJObjectToWiki(JObject jObject, int paddingCount)
        {
            var strPadding = string.Empty;
            for (var i = 0; i < paddingCount; i++)
                strPadding += "> ";

            var strBuilder = new StringBuilder();

            foreach (var property in jObject.Properties())
            {
                var type = property.Value.GetType();
                if (type == typeof(JValue))
                {
                    var name = property.Name;
                    var value = ((JValue)property.Value).Value;

                    strBuilder.AppendLine(string.Format("{0}*{1}:*  {2}",strPadding, name, value));
                }
                else if (type == typeof(JObject))
                {
                    var name = property.Name;
                    strBuilder.AppendFormat("{0}*{1}:*\r\n> {2}", strPadding, name, FormatJObjectToWiki((JObject)property.Value, paddingCount++));
                }
            }
            return strBuilder.ToString();
        }
    }
}
