<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="FileUpload.Main" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Main</title>

	<link href="content/css/bootstrap.min.css" rel="stylesheet" />
	<link href="content/css/Site.css" rel="stylesheet" />
	<link href="content/css/jquery-tagsinput.min.css" rel="stylesheet" />
</head>
<body>
	<form id="MainForm" runat="server">
		<header>
			<nav class="navbar navbar-expand-md navbar-dark fixed-top bg-dark">
				<a class="navbar-brand" href="#">FileHost</a>

				<button class="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbarCollapse" aria-controls="navbarCollapse" aria-expanded="false" aria-label="Toggle navigation">
					<span class="navbar-toggler-icon"></span>
				</button>

				<div class="collapse navbar-collapse" id="navbarCollapse">
					<ul class="navbar-nav mr-auto">
						<li class="dropdown">
							<button id="UserMenuButton" class="btn btn-info dropdown-toggle" type="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false" runat="server">User</button>

							<div id="DropdownMenu" class="dropdown-menu" aria-labelledby="dropdownMenuButton" runat="server">
								<a id="ListFilesDropdown" class="dropdown-item" runat="server" onserverclick="ListFilesButton_Click" aria-disabled="true">List Files</a>
								<a id="UploadFileDropdown" class="dropdown-item" runat="server" onserverclick="UploadButton_Click" aria-disabled="true">Upload File</a>
								<a id="ReloadDropdown" class="dropdown-item" runat="server" onserverclick="ReloadButton_Click" aria-disabled="true">Reload</a>
								<div id="DropdownDivider" runat="server" class="dropdown-divider"></div>
								<a id="LogOutDropdown" class="dropdown-item" runat="server" onserverclick="LogoutButton_Click">Log Out</a>
							</div>
						</li>
					</ul>

					<div class="form-inline mt-2 mt-md-0">
						<input id="SearchInput" class="form-control" type="text" placeholder="Search" aria-label="Search" runat="server" data-role="tags-input" />
						<label>&nbsp;&nbsp;</label>
						<asp:Button ID="SearchButton" class="btn btn-success my-2 my-s" type="button" runat="server" OnClick="SearchButton_Click" Text="Search" formnovalidate="formnovalidate" />
					</div>
				</div>
			</nav>
		</header>

		<%-- Elements come here --%>

		<div id="MainViewState" class="mt-5" runat="server" visible="false">
			<p id="InfoOutput" runat="server" class="h6 font-weight-normal">
			</p>

			<img id="preview-img" src="//:0" />

			<table class="table table-striped mainview">
				<thead id="MainViewState_TableColumns" runat="server">
					<tr>
						<th scope="col">Index</th>
						<th scope="col">Name</th>
						<th scope="col">Creation Date</th>
						<th scope="col">Tags</th>
						<th scope="col">Link</th>
					</tr>
				</thead>

				<tbody id="MainViewState_TableBody" runat="server" class="file-links">
				</tbody>
			</table>
		</div>

		<asp:Panel ID="LoginViewState" class="form-signin mt-5" runat="server" Visible="false" DefaultButton="TryLoginButton">
			<label for="LoginViewState_InputUsername" class="sr-only">Username</label>
			<input id="LoginViewState_InputUsername" type="text" class="form-control" runat="server" placeholder="Username" required="required" autofocus="autofocus" />

			<label for="LoginViewState_InputPassword" class="sr-only">Password</label>
			<input id="LoginViewState_InputPassword" type="password" class="form-control" runat="server" placeholder="Password" required="required" />

			<asp:Button ID="TryLoginButton" class="btn btn-lg btn-primary btn-block mb-3" runat="server" Text="Log In" OnClick="TryLoginButton_Click" />
			<h1 id="LoginViewState_Info" runat="server" class="h6 font-weight-normal"></h1>
		</asp:Panel>

		<asp:Panel ID="AddTagState" CssClass="form-group mt-5" runat="server" Visible="false">
			<input id="AddTagState_TagsBox" type="text" class="form-control" runat="server" placeholder="WebUpload" data-role="tags-input" value=""/>
			<label for="AddTagState_TagsBox" class="mt-3 mb-3">Use ; to separate tags</label>
			<asp:Button ID="AddTagState_SetTags" class="btn btn-lg btn-primary btn-block mt-3 mb-3" runat="server" Text="Set Tags" OnClick="SetTags_Click" />
			<h1 id="AddTagState_Info" runat="server" class="h6 font-weight-normal"></h1>
		</asp:Panel>

		<asp:Panel ID="UploadFileViewState" CssClass="form-group mt-5" runat="server" Visible="false" DefaultButton="UploadFileViewState_StartUploadFileButton">
			<input type="file" class="form-control-file mb-3" id="UploadFile" runat="server" required="required" />
			<input id="UploadFileViewState_TagsBox" type="text" class="form-control" runat="server" placeholder="WebUpload" data-role="tags-input" />
			<label for="UploadFileViewState_TagsBox" class="mt-3 mb-3">Use ; to separate tags</label>
			<asp:Button ID="UploadFileViewState_StartUploadFileButton" class="btn btn-lg btn-primary btn-block mt-3 mb-3" runat="server" Text="Upload File" OnClick="StartUploadFileButton_Click" />
			<h1 id="UploadFileViewState_Info" runat="server" class="h6 font-weight-normal"></h1>
			<a id="UploadFileViewState_FileLink" runat="server" href="//:0" visible="false"></a>
		</asp:Panel>
	</form>

	<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.3.1/jquery.min.js"></script>
	<script src="content/js/jquery_color/jquery.color.js"></script>
	<script src="content/js/jquery_color/jquery.color.svg-names.js"></script>
	<script src="content/js/bootstrap.min.js"></script>
	<script src="content/js/jquery_tagsinput/jquery-tagsinput.min.js"></script>
	<script src="content/js/Site.js"></script>
</body>
</html>
