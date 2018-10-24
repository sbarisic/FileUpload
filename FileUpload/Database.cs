using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;

namespace FileUpload {
	public struct DBConfig {
		public string ConnectionString;
	}

	public class Database : IDisposable {
		DBConfig Cfg;
		SqlConnection Con;
		Random Rnd = new Random();
		string DBName;

		public Database(string DatabaseName) {
			Cfg = new DBConfig() {
				ConnectionString = Config.GetConnectionString(DatabaseName)
			};

			DBName = DatabaseName;
			Con = new SqlConnection(Cfg.ConnectionString);
		}

		public void Connect() {
			Con.Open();
			Con.ChangeDatabase(DBName);
		}

		public void Disconnect() {
			Con.Close();
		}

		public IEnumerable<T> InvokeStoredProcedure<T>(string Name, SqlParameter[] Params) where T : SQLTable, new() {
			using (SqlCommand Cmd = new SqlCommand(string.Format("usp_LGN_{0}", Name), Con) { CommandType = CommandType.StoredProcedure }) {
				Cmd.Parameters.AddRange(Params);

				using (SqlDataReader Reader = Cmd.ExecuteReader()) {
					while (Reader.Read()) {
						T Result = new T();
						Result.Load(this, Reader);
						yield return Result;
					}
				}
			}
		}

		public void InvokeStoredProcedure(string Name, SqlParameter[] Params) {
			InvokeStoredProcedure<Users>(Name, Params).FirstOrDefault();
		}

		public Users GetUserByUsername(string Username) {
			return InvokeStoredProcedure<Users>(nameof(GetUserByUsername), new[] { new SqlParameter(nameof(Username), Username) }).FirstOrDefault();
		}

		public Users GetUserByID(int ID) {
			return InvokeStoredProcedure<Users>(nameof(GetUserByID), new[] { new SqlParameter(nameof(ID), ID) }).FirstOrDefault();
		}

		Users CreateUser(string Username, string Pwd, string Salt, out int UserID) {
			Users User = null;
			UserID = 0;

			try {
				SqlParameter UserIDParam = new SqlParameter(nameof(UserID), 0) { Direction = ParameterDirection.Output };

				User = InvokeStoredProcedure<Users>(nameof(CreateUser),
				   new[] { new SqlParameter(nameof(Username), Username), new SqlParameter(nameof(Pwd), Pwd), new SqlParameter(nameof(Salt), Salt), UserIDParam }).FirstOrDefault();

				UserID = (int)(long)UserIDParam.Value;
			} catch (SqlException) {
			}

			return User;
		}

		public Users CreateUser(string Username, string Password) {
			string Salt = PasswordManager.GenerateSalt();
			return CreateUser(Username, PasswordManager.HashPassword(Password, Salt), Salt, out int UserID);
		}

		public Tags GetTagsForFile(string FName) {
			return InvokeStoredProcedure<Tags>(nameof(GetTagsForFile), new[] { new SqlParameter(nameof(FName), FName) }).FirstOrDefault();
		}

		public void DeleteTagsForFile(string FName) {
			InvokeStoredProcedure<Tags>(nameof(DeleteTagsForFile), new[] { new SqlParameter(nameof(FName), FName) }).FirstOrDefault();
		}

		public void SetTagsForFile(string FName, string FileTags) {
			InvokeStoredProcedure<Tags>(nameof(SetTagsForFile), new[] { new SqlParameter(nameof(FName), FName), new SqlParameter(nameof(FileTags), FileTags) }).FirstOrDefault();
		}

		#region IDisposable Support
		private bool DisposedValue = false;

		protected virtual void Dispose(bool Disposing) {
			if (!DisposedValue) {
				if (Disposing) {
					Disconnect();
					Con.Dispose();
				}

				DisposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}

	public abstract class SQLTable {
		protected Database Database;

		public void Load(Database DB, SqlDataReader Reader) {
			Database = DB;
			FieldInfo[] Fields = GetFields();

			for (int i = 0; i < Fields.Length; i++) {
				object Val = Reader[Fields[i].Name];

				if (Fields[i].FieldType == typeof(bool) && Val is int I)
					Val = I > 0;

				if (Val is DBNull)
					Val = null;

				Fields[i].SetValue(this, Val);
			}
		}

		public virtual void Save() {
			FieldInfo[] Fields = GetFields();
			List<SqlParameter> Params = new List<SqlParameter>();

			for (int i = 0; i < Fields.Length; i++) {
				object Val = Fields[i].GetValue(this);

				if (Val is bool B)
					Val = B ? 1 : 0;

				if (Val == null)
					Val = DBNull.Value;

				SqlParameter Param = new SqlParameter(Fields[i].Name, Val);
				Params.Add(Param);
			}

			Database.InvokeStoredProcedure(CreateUpdateProcedureName(), Params.ToArray());
		}

		protected string CreateUpdateProcedureName() {
			return string.Format("Update{0}ByID", GetType().Name);
		}

		FieldInfo[] GetFields() {
			return GetType().GetFields();
		}
	}

	public class Users : SQLTable {
		public readonly int ID;

		public string Username;
		public string Pwd;
		public string Salt;

		public int LGN_UserInfoID;
		public int LGN_UserPermissionsID;

		UserPermissions UserPermissions;
		UserInfo UserInfo;

		public UserPermissions GetUserPermissions(bool ForceReload = false) {
			if (!ForceReload && UserPermissions != null)
				return UserPermissions;

			return UserPermissions = Database.InvokeStoredProcedure<UserPermissions>("GetUserPermissionsByID", new[] { new SqlParameter("ID", LGN_UserPermissionsID) }).FirstOrDefault();
		}

		public UserInfo GetUserInfo(bool ForceReload = false) {
			if (!ForceReload && UserInfo != null)
				return UserInfo;

			return UserInfo = Database.InvokeStoredProcedure<UserInfo>("GetUserInfoByID", new[] { new SqlParameter("ID", LGN_UserInfoID) }).FirstOrDefault();
		}

		public void Delete() {
			Database.InvokeStoredProcedure<UserPermissions>("DeleteUserByID", new[] { new SqlParameter("ID", ID) }).FirstOrDefault();

			Username = null;
			Pwd = null;
			Salt = null;
			LGN_UserInfoID = 0;
			LGN_UserPermissionsID = 0;
		}
	}

	public class UserInfo : SQLTable {
		public readonly int ID;

		public string Address;
		public string FirstName;
		public string LastName;
	}

	public class Tags : SQLTable {
		public string FName;
		public string FileTags;
	}

	public class UserPermissions : SQLTable {
		public readonly int ID;

		public bool ReadPerm;
		public bool WritePerm;
		public bool CreatePerm;
		public bool DeletePerm;
		public bool CreateUser;
		public bool DeleteUser;

		public void Set(bool ReadPerm, bool WritePerm, bool CreatePerm, bool DeletePerm, bool CreateUser, bool DeleteUser) {
			this.ReadPerm = ReadPerm;
			this.WritePerm = WritePerm;
			this.CreatePerm = CreatePerm;
			this.DeletePerm = DeletePerm;
			this.CreateUser = CreateUser;
			this.DeleteUser = DeleteUser;
		}

		public void Set(IEnumerable<KeyValuePair<string, bool>> Values) {
			foreach (FieldInfo PermField in GetAllPermissionFields())
				PermField.SetValue(this, Values.Where(KV => KV.Key == PermField.Name).Select(KV => KV.Value).First());
		}

		public static IEnumerable<FieldInfo> GetAllPermissionFields() {
			return typeof(UserPermissions).GetFields().Where(F => F.FieldType == typeof(bool));
		}

		public static IEnumerable<string> GetAllPermissionNames() {
			return GetAllPermissionFields().Select(F => F.Name);
		}
	}
}