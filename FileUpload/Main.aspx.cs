using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace FileUpload {
	public enum ViewStates {
		MainViewState,
		LoginViewState,
		AddTagState,
		UploadFileViewState,
	}

	public partial class Main : System.Web.UI.Page {
		public static readonly string[] SupportedImageExtensions = new string[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".tiff", ".apng", ".webp" };
		public const string AddTagParam = "AddTagFor";
		public string FilesDir = "/home/anon/public_html";

		Database CarpDB;
		List<FileEntry> FileEntries = new List<FileEntry>();
		string CurrentAddTagFile;

		ViewStates CurrentViewState {
			get {
				return (Session["CurrentViewState"] as ViewStates?) ?? ViewStates.MainViewState;
			}

			set {
				Session["CurrentViewState"] = value;
			}
		}

		protected void Page_Load(object Sender, EventArgs E) {
			if (Debugger.IsAttached) {
				FilesDir = "C:/Projekti/Discord_FTL_RE";
			}

			CarpDB = new Database("Carp");
			CarpDB.Connect();

			ListFilesDropdown.Visible = true;
			UploadFileDropdown.Visible = true;
			ReloadDropdown.Visible = true;
			DropdownDivider.Visible = true;

			if (Request.QueryString[AddTagParam] != null) {
				CurrentAddTagFile = Uri.UnescapeDataString(Request.QueryString[AddTagParam]);

				if (CurrentViewState != ViewStates.AddTagState)
					AddTagState_TagsBox.Value = string.Join(";", GetFileTags(new FileEntry() { Name = CurrentAddTagFile }));

				ChangeViewState(ViewStates.AddTagState);
			} else
				ChangeViewState(CurrentViewState);

			if (!UserSession.IsSessionValid(this)) {
				ChangeViewState(ViewStates.LoginViewState);

				ListFilesDropdown.Visible = false;
				UploadFileDropdown.Visible = false;
				ReloadDropdown.Visible = false;
				DropdownDivider.Visible = false;

				SearchInput.Disabled = true;
				SearchButton.Enabled = false;

				UserMenuButton.Attributes.Remove("data-toggle");
				Utils.ClassSet(UserMenuButton, "dropdown-toggle", false);
				UserMenuButton.InnerText = "Log In";
				UserMenuButton.ServerClick += (Snd, Evt) => ChangeViewState(ViewStates.LoginViewState);
				return;
			}

			UserMenuButton.InnerText = UserSession.GetSession(this).CurrentUser.Username;

			if (CurrentViewState == ViewStates.MainViewState) {
				ReloadFiles();
				ReloadFileTable();
			}
		}

		protected void ReloadFiles() {
			FileEntries.Clear();

			//if (Debugger.IsAttached)
			//FileEntries.Add(new FileEntry("8XpsBM.png"));

			if (Directory.Exists(FilesDir))
				foreach (string FileEntry in Directory.EnumerateFiles(FilesDir)) {
					FileEntry FEntry = new FileEntry(FileEntry);
					FEntry.Tags = GetFileTags(FEntry);
					FileEntries.Add(FEntry);
				}
		}

		protected void ReloadFileTable() {
			int Idx = 0;
			MainViewState_TableBody.InnerHtml = "";

			foreach (var FileEntry in FileEntries.AsEnumerable().OrderByDescending(Entry => Entry.ModTime)) {
				MainViewState_TableBody.InnerHtml += FileEntry.ToTableRow(++Idx);
			}
		}

		protected void ChangeViewState(ViewStates State) {
			foreach (ViewStates Val in Enum.GetValues(typeof(ViewStates)))
				Utils.SetVisible(GetType().GetField(Val.ToString(), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this), false);

			Utils.SetVisible(GetType().GetField(State.ToString(), BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this), true);
			CurrentViewState = State;
		}

		protected IEnumerable<string> ParseTags(string TagsRaw) {
			return (TagsRaw?.Trim() ?? "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(T => T.ToLower().Trim());
		}

		protected void SetFileTags(FileEntry Entry, string TagsRaw, bool AddUser, bool AddWebUpload) {
			List<string> Tags = new List<string>();

			if (AddUser)
				Tags.Add(string.Format("user:{0}", UserSession.GetSession(this).CurrentUser.Username));

			if (AddWebUpload)
				Tags.Add("webupload");

			foreach (var Tg in ParseTags(TagsRaw))
				if (!Tags.Contains(Tg))
					Tags.Add(Tg);

			SetFileTags(Entry, Tags.ToArray());
		}

		protected void SetFileTags(FileEntry Entry, string[] Tags) {
			Debug.WriteLine("{0} <= {1}", Entry.Name, string.Join(", ", Tags));

			CarpDB.SetTagsForFile(Entry.Name, string.Join(",", Tags));
		}

		protected string[] GetFileTags(FileEntry Entry) {
			Tags T = CarpDB.GetTagsForFile(Entry.Name);
			string[] Tags = new string[] { };

			if (T != null)
				Tags = T.FileTags.Split(',').ToArray();

			Debug.WriteLine("{0} => {1}", Entry.Name, string.Join(", ", Tags));
			return Tags;
		}

		protected void SetTags_Click(object Sender, EventArgs E) {
			string Tags = AddTagState_TagsBox.Value;
			AddTagState_TagsBox.Value = "";

			SetFileTags(new FileEntry() { Name = CurrentAddTagFile }, Tags, true, false);
			ChangeViewState(ViewStates.MainViewState);
			Response.Redirect("~/Main.aspx");
		}

		protected void TryLoginButton_Click(object Sender, EventArgs E) {
			string Username = LoginViewState_InputUsername.Value;
			string Password = LoginViewState_InputPassword.Value;
			Users User = null;

			if ((User = CarpDB.GetUserByUsername(Username)) != null && PasswordManager.IsValidPassword(Password, User.Salt, User.Pwd)) {
				UserSession.SetSession(this, new UserSession(User));
				ChangeViewState(ViewStates.MainViewState);
				Response.Redirect(Request.RawUrl);
				return;
			} else
				Print("Invalid username or password");
		}

		protected void UploadButton_Click(object Sender, EventArgs E) {
			if (!UserSession.IsSessionValid(this)) {
				Response.Redirect("~/Main.aspx");
				return;
			}

			ChangeViewState(ViewStates.UploadFileViewState);
		}

		protected void StartUploadFileButton_Click(object Sender, EventArgs E) {
			if (!UserSession.IsSessionValid(this)) {
				Response.Redirect("~/Main.aspx");
				return;
			}

			if (!Directory.Exists(FilesDir)) {
				Print("ERROR: Files directory does not exist");
				return;
			}

			HttpPostedFile PostedFile = UploadFile.PostedFile;
			if ((PostedFile?.ContentLength ?? 0) == 0) {
				Print("ERROR: No file selected");
				return;
			}

			string Ext = Path.GetExtension(PostedFile.FileName);
			string UploadName = Utils.GenRndLabel();
			string UploadNameWithExt = UploadName + Ext;
			string FullPath = Path.Combine(FilesDir, UploadNameWithExt);

			PostedFile.SaveAs(FullPath);
			FileEntry FileEntry = new FileEntry(FullPath);
			string FileLink = FileEntry.GenerateLinkText();

			SetFileTags(FileEntry, UploadFileViewState_TagsBox?.Value, true, true);
			FileEntries.Add(FileEntry);

			Print("Done!");
			UploadFileViewState_FileLink.Visible = true;
			UploadFileViewState_FileLink.HRef = FileLink;
			UploadFileViewState_FileLink.InnerText = FileLink;
		}

		protected void RegisterButton_Click(object Sender, EventArgs E) {
			if (!UserSession.IsSessionValid(this)) {
				Response.Redirect("~/Main.aspx");
				return;
			}

			Print("Register");
		}

		protected void SettingsButton_Click(object Sender, EventArgs E) {
			if (!UserSession.IsSessionValid(this)) {
				Response.Redirect("~/Main.aspx");
				return;
			}

			Print("Settings");
		}

		protected void LogoutButton_Click(object Sender, EventArgs E) {
			if (UserSession.IsSessionValid(this)) {
				UserSession.SetSession(this, null);
				Response.Redirect("~/Main.aspx");
			}
		}

		protected void ListFilesButton_Click(object Sender, EventArgs E) {
			if (CurrentViewState != ViewStates.MainViewState)
				ReloadFiles();

			ChangeViewState(ViewStates.MainViewState);
			ReloadFileTable();
		}

		protected void ReloadButton_Click(object Sender, EventArgs E) {
			Response.Redirect("~/Main.aspx");
		}

		protected void SearchButton_Click(object Sender, EventArgs E) {
			if (!UserSession.IsSessionValid(this)) {
				Response.Redirect("~/Main.aspx");
				return;
			}

			string SearchQuery = SearchInput.Value.Trim();
			string[] Tags = ParseTags(SearchQuery).ToArray();

			if (string.IsNullOrEmpty(SearchQuery))
				return;

			FileEntries.RemoveAll(Entry => {
				foreach (var Tg in Tags)
					if (!(Entry.Tags?.Contains(Tg) ?? false))
						return true;

				return false;
			});

			ReloadFileTable();
		}

		public void Print(string Text) {
			if (CurrentViewState == ViewStates.LoginViewState)
				LoginViewState_Info.InnerText = Text;
			else if (CurrentViewState == ViewStates.MainViewState)
				InfoOutput.InnerText = Text;
			else if (CurrentViewState == ViewStates.UploadFileViewState)
				UploadFileViewState_Info.InnerText = Text;
			else
				throw new NotImplementedException();
		}

		public void Print(string Fmt, params object[] Args) {
			Print(string.Format(Fmt, Args));
		}

		public override void Dispose() {
			CarpDB?.Dispose();
			base.Dispose();
		}
	}
}