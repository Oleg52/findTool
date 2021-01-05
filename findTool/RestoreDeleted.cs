using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace findTool
{
    public class RestoreDeleted
    {
        public static bool isRunning = true;
        public static string mapCode;
        public static string method;
        public static string saveFile;
        public static CountdownEvent cde;
        public static bool isFound = false;
        public void Restore(string mapCd, string meth, string saveF)
        {
            // fix for win7
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            mapCode = mapCd;
            method = meth;
            saveFile = saveF;
            IList<Task> tasks = new List<Task>();
            for(int i = 0; i <= 9500; i += 500)
                tasks.Add(new Task((new FindMapId(i, i + 500)).Search));
            cde = new CountdownEvent(tasks.Count);
            foreach (var task in tasks)
                task.Start();
            cde.Wait();
            if (!isFound)
                File.WriteAllText(@"map.json", "emptyE[Q", new UTF8Encoding(false));
            cde.Dispose();
        }
    }

    public class FindMapId
    {
        private int bound1;
        private int bound2;

        public FindMapId(int bound1, int bound2)
        {
            this.bound1 = bound1;
            this.bound2 = bound2;
        }

        public async void Search()
        {
            var httpClient = new HttpClient();
            string lastPart;
            if (RestoreDeleted.method.Equals("0")) lastPart = "/2_0.jpg";
            else if (RestoreDeleted.method.Equals("1")) lastPart = "/2_1.jpg";
            else if (RestoreDeleted.method.Equals("2")) lastPart = "/2_2.jpg";
            else lastPart = "/1_0.jpg";
            for (int i = bound1; i < bound2 && RestoreDeleted.isRunning; i++)
            {
                string mapId = i.ToString();
                while (mapId.Length != 4)
                    mapId = "0" + mapId;
                string url = "https://prod.cloud.rockstargames.com/ugc/gta5mission/" + mapId + "/" + RestoreDeleted.mapCode;
                url += lastPart;
                try
                {
                    var response = await httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        if (content.Length > 10)
                        {
                            RestoreDeleted.isRunning = false;
                            string path = File.ReadAllText("origPath.txt", new UTF8Encoding(false));
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(url, @path + "image_" + mapId + ".jpg");
                            webClient.Dispose();
                            RestoreDeleted.isFound = true;
                            //RestoreDeleted.cde.Reset();
                            SearchForJson sfj = new SearchForJson();
                            sfj.Prepare(mapId, RestoreDeleted.mapCode, "0", "none", RestoreDeleted.saveFile);
                        }
                    }
                }
                catch (Exception e)
                { }
            }
            RestoreDeleted.cde.Signal();
        }
    }
}
