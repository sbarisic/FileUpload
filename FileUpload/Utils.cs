using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace FileUpload {
	public static class Utils {
		static Random Rnd = new Random();

		public static void SetVisible(object Ctrl, bool Visible) {
			((Control)Ctrl).Visible = Visible;
		}

		/*public static void CreateRootUser(Database DB) {
			if (DB.GetUserByUsername("root") == null) {
				Users RootUser = DB.CreateUser("root", "root");

				UserPermissions RootUserPermissions = RootUser.GetUserPermissions();
				RootUserPermissions.Set(true, true, true, true, true, true);
				RootUserPermissions.Save();

				UserInfo Info = RootUser.GetUserInfo();
				Info.FirstName = "Rooty";
				Info.LastName = "Rootington";
				Info.Address = "Root Street 420";
				Info.Save();
			}
		}*/

		public static void ClassSet(HtmlControl Ctrl, string Item, bool Val) {
			string Class = Ctrl.Attributes["class"];

			if (Val) {
				if (!Class.Contains(Item))
					Ctrl.Attributes["class"] += " " + Item;
			} else {
				if (Class.Contains(Item))
					Ctrl.Attributes["class"] = Class.Replace(Item, "");
			}
		}

		public static int MultiRandom(params Tuple<int, int>[] IncExcRanges) {
			int Num = Rnd.Next(0, IncExcRanges.Length);
			return Rnd.Next(IncExcRanges[Num].Item1, IncExcRanges[Num].Item2);
		}

		public static string GenRndLabel(bool MustBeginWithLetter = false, int Len = 6) {
			StringBuilder SB = new StringBuilder();
			Tuple<int, int> Upper = new Tuple<int, int>(65, 91);
			Tuple<int, int> Lower = new Tuple<int, int>(97, 123);
			Tuple<int, int> Numbers = new Tuple<int, int>(48, 58);

			if (MustBeginWithLetter)
				SB.Append((char)MultiRandom(Upper, Lower));

			if (MustBeginWithLetter)
				Len--;

			for (int i = 0; i < Len; i++)
				SB.Append((char)MultiRandom(Upper, Lower, Numbers));

			return SB.ToString();
		}
	}
}