using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace findTool
{
    class MainClass
    {
		public static async Task Main(string[] args)
		{
			if (args[0].Length > 30)
			{
				string str = args[0];
				string mapId = str.Substring(53, 4);
				string mapCode = str.Substring(58, str.LastIndexOf("/") - 58);
				SearchForJson sfj = new SearchForJson();
				sfj.Prepare(mapId, mapCode, args[2], args[3], args[4]);
			}
			else
			{
				RestoreDeleted rd = new RestoreDeleted();
				rd.Restore(args[0], args[1], args[4]);
			}
		}
	}
}
