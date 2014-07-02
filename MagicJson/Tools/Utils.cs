using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using MagicJson.Data;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace MagicJson.Tools
{
    public static class Utils
    {
        public enum ValidationState
        {
            NotEvaluated,
            Error,
            Success
        }

        public static Result Validate(string json, string schema)
        {
            var result = new Result();
            try
            {
                IList<string> message;
                var jsonSchema = JsonSchema.Parse(schema);
                var jObject = JObject.Parse(json);
                result.Valid = jObject.IsValid(jsonSchema, out message);
                result.Messages = message;
            }
            catch (Exception e)
            {
                result.Valid = false;
                result.Exception = e;
            }
            return result;
        }

        public static void LoadFiles(ref DataGrid dataGrid, ref List<ValidationItem> dataItems, IEnumerable<string> files)
        {
            if (dataItems == null)
                dataItems = new List<ValidationItem>();

            //on boucle sur tt les fichiers Json
            foreach (var fileName in files.Where(fileName => File.GetAttributes(fileName) != FileAttributes.Directory))
            {
                //si le fichier est déjà chargé on passe
                if (dataItems.Any(dataItem => dataItem.File == fileName)) continue;
                dataItems.Add(new ValidationItem(fileName, ValidationState.NotEvaluated));
            }
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = dataItems;
        }

        public static void OpenFile(string file)
        {
            var psi = new ProcessStartInfo("notepad++.exe", file) { LoadUserProfile = true };

            try
            {
                Process.Start(psi);
            }
            catch
            {
                psi.FileName = "notepad.exe";
                Process.Start(psi);
            }
        }

        public static string FormatResult(Result result, byte[] source)
        {
            var validation = result.Valid ? "Success" : "Error";

            var description = string.Empty;
            if (!result.Valid)
                description = result.Exception != null
                                   ? FormatedException(result.Exception)
                                   : FormatedMessage(result, source);

            return String.Format("State: {0}{1}", validation, description);
        }

        public static string FormatedException(Exception exception)
        {
            return string.Format("\r\n\r\nException\r\n\r\n{0}\r\nSource: {1}\r\nStackTrace:\r\n{2}",
                                exception.Message, exception.Source, exception.StackTrace);
        }

        private static string FormatedMessage(Result result, byte[] source)
        {
            /*
             * ici on fait plusieurs regex afin de pouvoir faire qu'un seule lecture du fichier.
             * regex améliorable pour :
             *      virer les replace moche
             *      avoir par match, le couple de valeure de "line" et "position"
             */

            var messageItems = new List<MessageItem>();

            //on créé l'objet qui contiendra le detail par message et on commence à le remplir
            var regexLine = new Regex(@"Line (.*),");
            var regexPosition = new Regex(@"Position (.*)\.");
            foreach (var message in result.Messages)
            {
                var item = new MessageItem { Message = message };

                var match = regexLine.Match(message);
                if (match.Success)
                {
                    item.Line = Convert.ToInt32(match.Value.Replace("Line ", string.Empty).Replace(",", string.Empty));
                    match = regexPosition.Match(message);
                    if (match.Success)
                        item.Position = Convert.ToInt32(match.Value.Replace("Position ", string.Empty).Replace(".", string.Empty));
                }
                messageItems.Add(item);
            }

            //on récupére la liste des ligne de code à récupérer (associé à l'index du message dans messageItems, pour le retrouver)
            var lines = new List<KeyValuePair<int, int>>();
            for (var index = 0; index < messageItems.Count; index++)
                if (messageItems[index].Line > 0)
                    lines.Add(new KeyValuePair<int, int>(index, messageItems[index].Line));//new[] {index, messageItems[index].Line});

            //on récupére la liste des lignes de code
            var codes = GetFileLinesContent(lines, source);
            foreach (var content in codes)
                messageItems[content.Key].Code = content.Value;

            //à partir de là on peut construire le messages par erreur

            return messageItems.Aggregate(string.Empty, (current, item) => current + (current.Length > 0 ? "\r\n" + item.GetPresentedData() : item.GetPresentedData()));
        }

        private static IEnumerable<KeyValuePair<int, string>> GetFileLinesContent(IList<KeyValuePair<int, int>> linesIndex, byte[] source)
        {
            var lines = new List<KeyValuePair<int, string>>();
            using (var sr = new StreamReader(new MemoryStream(source)))
            {
                var curr = 1;
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null) break;

                    for (var index = 0; index < linesIndex.Count; index++)
                    {
                        if (curr != linesIndex[index].Value) continue;
                        lines.Add(new KeyValuePair<int, string>(linesIndex[index].Key, line.Replace("\t", string.Empty)));
                        linesIndex.RemoveAt(index); //on retire l'element de la liste pour éviter de reboucler par la suite
                        break;
                    }
                    curr++;
                }
            }
            return lines;
        }
    }
}
