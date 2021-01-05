using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Net.Http;

namespace findTool
{
    public class SearchForJson
    {
        public static bool isRunning = true;
        public static bool isFound = false;
        public static bool isNext = false;
        public static bool isSave;
        public static CountdownEvent cde;
        public void Prepare(string mapId, string mapCode, string next, string curLang, string saveFile)
        {
            // fix for win7
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            string url = "https://prod.cloud.rockstargames.com/ugc/gta5mission/" + mapId + "/" + mapCode + "/0_";
            isNext = next.Equals("1");
            isSave = saveFile.Equals("1");
            IList<Action> actions = new List<Action>();
            if (curLang.Equals("none"))
            {
                actions.Add((new FindJson(url, "en")).Search);
                actions.Add((new FindJson(url, "de")).Search);
                actions.Add((new FindJson(url, "ru")).Search);
                actions.Add((new FindJson(url, "zh")).Search);
                actions.Add((new FindJson(url, "zh-cn")).Search);
                actions.Add((new FindJson(url, "ja")).Search);
                actions.Add((new FindJson(url, "fr")).Search);
                actions.Add((new FindJson(url, "it")).Search);
                actions.Add((new FindJson(url, "ko")).Search);
                actions.Add((new FindJson(url, "es")).Search);
                actions.Add((new FindJson(url, "es-mx")).Search);
                actions.Add((new FindJson(url, "pl")).Search);
                actions.Add((new FindJson(url, "pt")).Search);
            }
            else
            {
                actions.Add((new FindJson(url, curLang)).Search);
            }
            cde = new CountdownEvent(actions.Count);
            foreach (var action in actions)
            {
                try
                {
                    Task.Run(action);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            cde.Wait();
            if (!isFound)
                File.WriteAllText(@"map.json", "emptyE[Q", new UTF8Encoding(false));
            cde.Dispose();
        }
    }

    public class FindJson
    {
        private string url;
        private string lang;
        private string lang2;

        public FindJson(string url, string lang)
        {
            this.url = url;
            this.lang = "_" + lang + ".json";
            this.lang2 = lang;
        }

        public async void Search()
        {
            var httpClient = new HttpClient();
            for (int i = 0; i < 300 && SearchForJson.isRunning; i++)
            {
                try
                {
                    var response = await httpClient.GetAsync(url + i.ToString() + lang);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        if (content.Length > 100)
                        {
                            if (SearchForJson.isNext)
                            {
                                SearchForJson.isNext = false;
                                continue;
                            }

                            string jsonText = lang2 + content + "E[Q";
                            File.WriteAllText(@"map.json", jsonText, new UTF8Encoding(false));
                            if (SearchForJson.isSave)
                            {
                                try
                                {
                                    string path = File.ReadAllText("origPath.txt", new UTF8Encoding(false));
                                    File.WriteAllText(@path + "map.json", content, new UTF8Encoding(false));
                                }
                                catch (Exception)
                                { }
                            }
                            SearchForJson.isRunning = false;
                            SearchForJson.isFound = true;
                            SearchForJson.cde.Reset();
                        }
                    }
                }
                catch (Exception e)
                { }
            }
            SearchForJson.cde.Signal();
        }
    }
}
