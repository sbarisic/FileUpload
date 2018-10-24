using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FileUpload {
	public partial class Default : System.Web.UI.Page {
		protected void Page_Load(object Sender, EventArgs E) {
			Response.Redirect("~/Main.aspx");
		}
	}
}