using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WM = WebMarkdown.WebMarkdown;

namespace FileUpload {
	public partial class md : System.Web.UI.Page {
		protected void Page_Load(object sender, EventArgs e) {
			Response.ClearContent();

			try {
				HandleLoad();
			} catch (Exception E) {
				PlainPrint(string.Format("{0}\r\n{1}", E.Message, E.StackTrace));
			}

			Response.Flush();
			Response.End();
			Context.ApplicationInstance.CompleteRequest();
		}

		void HandleLoad() {
			string AbsRequest = Request.RawUrl.Substring(1);
			string FileName = null;

			string RootDir = "/var/www/html/bin/";
			string ContentDir = "/var/www/html/content/";
			string PublicHtmlDir = "/home/anon/public_html/";

			if (Debugger.IsAttached) {
				RootDir = "C:/Projekti/FileUpload/FileUpload/bin/";
				ContentDir = "C:/Projekti/FileUpload/FileUpload/bin/content/";
				PublicHtmlDir = "C:/Projekti/FileUpload/FileUpload/bin/tests/";
			}

			if (AbsRequest.Contains("?"))
				FileName = AbsRequest.Substring(AbsRequest.IndexOf('?') + 1);

			if (string.IsNullOrWhiteSpace(FileName)) {
				//PlainPrint("ERROR: File name received was null or empty");
				Response.Redirect("md.aspx?main.md");
				return;
			} else if (FileName.StartsWith("/")) {
				PlainPrint("ERROR: File name must not start with /");
				return;
			} else if (FileName.Contains("..")) {
				PlainPrint("ERROR: .. not allowed in file path");
				return;
			} else if (FileName.StartsWith("$/")) {
				FileName = FileName.Substring(2);
				FileName = Path.Combine(PublicHtmlDir, FileName);
			}

			Response.ContentType = "text/html";
			Response.Write(WM.ProcessFile(RootDir, ContentDir, FileName));
		}

		void PlainPrint(string Msg) {
			Response.ContentType = "text/plain";
			Response.Write(Msg);
		}
	}
}