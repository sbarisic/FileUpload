using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using WinSCP;

namespace DeployToServer {
	class Program {
		static string Destination;
		static string Source;

		static string ReadPassword() {
			string Pass = "";

			if (Console.IsInputRedirected)
				return Console.ReadLine();

			do {
				ConsoleKeyInfo Key = Console.ReadKey(true);

				if (Key.Key != ConsoleKey.Backspace && Key.Key != ConsoleKey.Enter) {
					Pass += Key.KeyChar;
					Console.Write("*");
				} else if (Key.Key == ConsoleKey.Backspace && Pass.Length > 0) {
					Pass = Pass.Substring(0, Pass.Length - 1);
					Console.Write("\b \b");
				} else if (Key.Key == ConsoleKey.Enter) {
					Console.WriteLine();
					return Pass;
				}
			} while (true);
		}

		static string Prompt(string PromptString, string Default = null, bool IsPassword = false) {
			if (!Console.IsInputRedirected)
				Console.Write("{0}{1}: ", PromptString, (Default != null ? string.Format(" [default is `{0}´]", Default) : ""));

			string In = "";

			if (IsPassword)
				In = ReadPassword();
			else
				In = Console.ReadLine().Trim();

			if (In.Length == 0) {
				if (Default != null)
					return Default;

				return Prompt(PromptString, Default);
			}

			return In;
		}

		static void RemoveIfExists(Session S, string Remote) {
			Remote = Path.Combine(Destination, Remote);
			Console.WriteLine("Removing {0}", Remote);

			if (S.FileExists(Remote))
				S.RemoveFiles(Remote).Check();
		}

		static void Copy(Session S, string Local, string DestFolder, bool Recursive = false) {
			Local = Path.Combine(Source, Local).CleanPath();

			if (Recursive || !string.IsNullOrWhiteSpace(Path.GetExtension(Local))) {
				S.PutFiles(Local, Destination, options: new TransferOptions() { TransferMode = TransferMode.Binary }).Check();
			} else {

				string[] LocalFiles = Directory.GetFiles(Local);
				string DestFolderName = DestFolder.Length > 0 ? DestFolder + "/" : "";

				foreach (var LocalFile in LocalFiles) {
					if (Path.GetExtension(LocalFile).ToLower() == ".pdb")
						continue;

					S.PutFiles(LocalFile, Destination + DestFolderName, options: new TransferOptions() { TransferMode = TransferMode.Binary }).Check();
				}

				//S.PutFiles(Local, Destination, options: new TransferOptions() { TransferMode = TransferMode.Binary }).Check();
			}
		}

		static void Main(string[] Args) {
			bool UsingExistingCredentials = false;

			Destination = "/var/www/html/";
			Source = "C:/Projekti/FileUpload/FileUpload/";

			SessionOptions SOptions = new SessionOptions() {
				Protocol = Protocol.Ftp
			};

			if (File.Exists("Cred.txt")) {
				UsingExistingCredentials = true;
				string[] Creds = File.ReadAllLines("Cred.txt");

				SOptions.HostName = Creds[0];
				SOptions.UserName = Creds[1];
				SOptions.Password = Creds[2];
			} else {
				SOptions.HostName = Prompt("Host name", "carp.cf");
				SOptions.UserName = Prompt("User name", "carp");
				SOptions.Password = Prompt("Password", IsPassword: true);
			}

			using (Session S = new Session()) {
				S.Open(SOptions);
				S.FileTransferred += (Snd, Evt) => {
					if (Evt.Error != null)
						Console.WriteLine(Evt.Error.Message);
					else
						Console.WriteLine("{0}", Evt.FileName);
				};

				TransferOptions TOptions = new TransferOptions() { TransferMode = TransferMode.Binary };

				RemoveIfExists(S, "bin");
				Copy(S, "bin", "bin");

				RemoveIfExists(S, "content");
				Copy(S, "bin/content", "content", true);

				RemoveIfExists(S, "*.aspx");
				Copy(S, "*.aspx", "");
			}

			if (!UsingExistingCredentials) {
				Console.WriteLine("Done!");
				Console.ReadLine();
			}
		}
	}

	static class Utils {
		public static string CleanPath(this string Str) {
			return Str.Replace('/', '\\');
		}
	}
}
