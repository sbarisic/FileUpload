using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace FileUpload {
	public static class Config {
		static Dictionary<string, string> ConnectionStrings;

		public static string GetConnectionString(string Name) {
			if (ConnectionStrings == null) {
				ConnectionStrings = new Dictionary<string, string>();
				Confidential.PopulateConnectionStrings(ConnectionStrings);
			}

			return ConnectionStrings[Name];
		}
	}
}