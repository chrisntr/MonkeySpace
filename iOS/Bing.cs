using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Linq;
using System.Json;
using System.IO;
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.AVFoundation;

namespace Monospace11
{
	public delegate void SynchronizerDelegate (List<string>[] results);
	
	public class Bing
	{
		const string AZURE_KEY = "XdIZ8YFGJs3o0DWHaGSUEx3VJmpTsqFSadX5pec9dl4=";
		static SynchronizerDelegate sync;
		
		public Bing (SynchronizerDelegate sync)
		{
			Bing.sync = sync;
		}
		
		public void ImageSearch ()
		{
			var t = new Thread (DoSearch);
			t.Start ();
		}
		
		void DoSearch ()
		{
			string uri = "https://api.datamarket.azure.com/Data.ashx/Bing/Search/v1/Image?Query=%27monkeyspace%27&$top=50&$format=Json";
			
			var httpReq = (HttpWebRequest)HttpWebRequest.Create (new Uri (uri));
			
			httpReq.Credentials = new NetworkCredential (AZURE_KEY, AZURE_KEY);
			
			try {
				using (HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse ()) {
					
					var response = httpRes.GetResponseStream ();
					var json = (JsonObject)JsonObject.Load (response);
					Console.WriteLine (json.ToString());
					var result1 = (from result in (JsonArray)json ["d"] ["results"]
					               let jResult = result as JsonObject 
					               select jResult ["Thumbnail"] ["MediaUrl"].ToString ()).ToList ();
					
					var result2 = (from result in (JsonArray)json ["d"] ["results"]
					               let jResult = result as JsonObject 
					               select jResult ["MediaUrl"].ToString ()).ToList ();
					var results = new List<String>[] { result1, result2 };
					
					if (sync != null)
						sync (results);
				}
			} catch (Exception) {
				if (sync != null)
					sync (null);
			}
		}
		
	}
}