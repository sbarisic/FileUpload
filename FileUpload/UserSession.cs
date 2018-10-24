using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace FileUpload {
	public class UserSession {
		public Users CurrentUser;

		public UserSession(Users User) {
			CurrentUser = User;
		}

		public static UserSession GetSession(Page P) {
			return (UserSession)P.Session[nameof(UserSession)];
		}

		public static void SetSession(Page P, UserSession Session) {
			P.Session[nameof(UserSession)] = Session;

			if (Session == null) {
				P.Session.Abandon();
				return;
			}
		}

		public static bool IsSessionValid(Page P) {
			UserSession Session = (UserSession)P.Session[nameof(UserSession)];
			return Session != null;
		}
	}
}