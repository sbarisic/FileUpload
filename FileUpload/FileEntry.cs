using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FileUpload {
	public struct FileEntry {
		public string FullPath;
		public string Name;
		public DateTime ModTime;
		public string[] Tags;

		bool FileExists;

		public FileEntry(string FullFilePath) {
			FullPath = FullFilePath;
			Name = Path.GetFileName(FullFilePath);
			ModTime = DateTime.Now;
			Tags = new string[] { };

			FileExists = File.Exists(FullFilePath);

			if (FileExists)
				ModTime = File.GetLastWriteTime(FullFilePath);
		}

		public string GetTagsHtml() {
			string BtnSrc = string.Format("<a href=\"?" + Main.AddTagParam + "={0}\" class=\"btn btn-primary btn-sm btn-outline-success\">Add</a>", Uri.EscapeDataString(Name));

			if (Tags.Length == 0)
				return "No Tags &nbsp;&nbsp;" + BtnSrc;

			return string.Join(", ", Tags) + " &nbsp;&nbsp;" + BtnSrc;
		}

		public string GenerateLinkText() {
			return string.Format("https://carp.cf/$/{0}", Name);
		}

		public string GenerateLink() {
			string URL = GenerateLinkText();

			if (Main.SupportedImageExtensions.Contains(Path.GetExtension(Name).ToLower()))
				return string.Format("<a href=\"{0}\" class=\"img-link\">{0}</a>", URL);

			return string.Format("<a href=\"{0}\">{0}</a>", URL);
		}

		public string ToTableRow(int Idx) {
			string Link = GenerateLink() + "\r\n";
			string RowAttribs = "";

			if (!FileExists)
				RowAttribs = "class = \"table-danger\"";

			return string.Format("<tr {0}><th scope=\"row\">{1}</th><th>{2}</th><th>{3}</th><th>{4}</th><th>{5}</th></tr>", RowAttribs, Idx, Name, ModTime.ToString("dd.MM.yyyy. HH:mm"), GetTagsHtml(), Link);
		}
	}
}