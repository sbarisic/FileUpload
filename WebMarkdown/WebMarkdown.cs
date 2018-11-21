using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Markdig;
using Markdig.SyntaxHighlighting;

namespace WebMarkdown {
	public static class WebMarkdown {
		const string ContentToken = "<!--CONTENT-->";
		const string BootstrapStyleToken = "<!--BOOTSTRAP_STYLE-->";
		const string BootstrapJsToken = "<!--BOOTSTRAP_JS-->";
		const string MarkdownStyleToken = "<!--MARKDOWN_STYLE-->";

		public static string BootstrapStyleFilePath = "css/bootstrap.min.css";
		public static string MarkdownStyleFilePath = "markdown_style.css";
		public static string BootstrapJsFilePath = "js/bootstrap.min.js";
		public static string MarkdownTemplateFilePath = "markdown_template.html";

		static string RootDirectory = "";
		static string ContentDirectory = "";

		const bool EmbedInDebugger = true;

		static WebClient WC;
		static MarkdownPipeline Pipeline;

		public static string ProcessFile(string RootDir, string ContentDir, string InFile) {
			RootDirectory = RootDir;
			ContentDirectory = ContentDir;

			string BootstrapStyleFile = BootstrapStyleFilePath.Contentify();
			string MarkdownStyleFile = MarkdownStyleFilePath.Contentify();
			string BootstrapJsFile = BootstrapJsFilePath.Contentify();
			string MarkdownTemplateFile = MarkdownTemplateFilePath.Contentify();

			InFile = InFile.Rootify();

			if (Pipeline == null)
				Pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseBootstrap().UseSyntaxHighlighting().Build();

			string Template = ContentToken;

			if (File.Exists(MarkdownTemplateFile))
				Template = File.ReadAllText(MarkdownTemplateFile);
			else
				Template = HtmlComment(MarkdownTemplateFile) + Template;

			string Markdown = File.ReadAllText(InFile).Trim();
			string Processed = Template.Replace(ContentToken, Process(Pipeline, Markdown));

			if (EmbedInDebugger && Debugger.IsAttached)
				Processed = EmbedImages(Processed);

			if (!EmbedInDebugger && Debugger.IsAttached)
				Processed = Processed.Replace(BootstrapStyleToken, "<link href=\"content/css/bootstrap.min.css\" rel=\"stylesheet\" />");
			else if (File.Exists(BootstrapStyleFile))
				Processed = Processed.Replace(BootstrapStyleToken, string.Format("<style>{0}</style>", File.ReadAllText(BootstrapStyleFile)));
			else
				Processed = Processed.Replace(BootstrapStyleToken, HtmlComment(BootstrapStyleFile));

			if (!EmbedInDebugger && Debugger.IsAttached)
				Processed = Processed.Replace(MarkdownStyleToken, "<link href=\"content/markdown_style.css\" rel=\"stylesheet\" />");
			else if (File.Exists(MarkdownStyleFile))
				Processed = Processed.Replace(MarkdownStyleToken, string.Format("<style>{0}</style>", File.ReadAllText(MarkdownStyleFile)));
			else
				Processed = Processed.Replace(MarkdownStyleToken, HtmlComment(MarkdownStyleFile));

			if (!EmbedInDebugger && Debugger.IsAttached)
				Processed = Processed.Replace(BootstrapJsToken, "<script src=\"content/js/bootstrap.min.js\"></script>");
			else if (File.Exists(BootstrapJsFile))
				Processed = Processed.Replace(BootstrapJsToken, string.Format("<script>{0}</script>", File.ReadAllText(BootstrapJsFile)));
			else
				Processed = Processed.Replace(BootstrapJsToken, HtmlComment(BootstrapJsFile));

			return Processed;
		}

		static string Process(MarkdownPipeline Pipeline, string MD) {
			return Markdown.ToHtml(MD, Pipeline);
		}

		static string EmbedImages(string Processed) {
			const string ImgSrcStart = "<img src=";
			const string ImgSrcAttrib = "src=\"";

			while (Processed.Contains(ImgSrcStart)) {
				int Idx = Processed.IndexOf(ImgSrcStart);
				int Ending = Processed.IndexOf("/>", Idx) + 2;
				string ImgTag = Processed.Substring(Idx, Ending - Idx);

				int ImgSrcIdx = ImgTag.IndexOf(ImgSrcAttrib) + ImgSrcAttrib.Length;
				int ImgSrcEnd = ImgTag.IndexOf('"', ImgSrcIdx);
				string ImageSource = ImgTag.Substring(ImgSrcIdx, ImgSrcEnd - ImgSrcIdx);

				string NewTag = string.Format("<img alt=\"\" src=\"data:image/png;base64,{0}\" />", GetImageAsBase64Url(ImageSource));
				Processed = Processed.Remove(Idx, Ending - Idx).Insert(Idx, NewTag);
			}

			return Processed;
		}

		static string GetImageAsBase64Url(string URL) {
			byte[] Data = new byte[] { };

			if (URL.StartsWith("http")) {
				if (WC == null)
					WC = new WebClient();

				Data = WC.DownloadData(URL);
			} else if (File.Exists(URL.Rootify()))
				Data = File.ReadAllBytes(URL.Rootify());
			else
				throw new Exception("Image not found at " + URL);

			return Convert.ToBase64String(Data);
		}

		static string HtmlComment(string Str) {
			return string.Format("<!-- {0} -->\r\n", WebUtility.HtmlEncode(Str));
		}

		static string Rootify(this string Pth) {
			return Path.Combine(RootDirectory, Pth);
		}

		static string Contentify(this string Pth) {
			return Path.Combine(ContentDirectory, Pth);
		}
	}
}
