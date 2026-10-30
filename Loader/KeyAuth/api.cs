using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cryptographic;

namespace KeyAuth
{
	// Token: 0x02000002 RID: 2
	public class api
	{
		// Token: 0x06000001 RID: 1
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		// Token: 0x06000002 RID: 2
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetCurrentProcess();

		// Token: 0x06000003 RID: 3
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern ushort GlobalAddAtom(string lpString);

		// Token: 0x06000004 RID: 4
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern ushort GlobalFindAtom(string lpString);

		// Token: 0x06000005 RID: 5 RVA: 0x00002050 File Offset: 0x00000250
		public api(string name, string ownerid, string version, string path = null)
		{
			bool flag = ownerid.Length != 10;
			if (flag)
			{
				Process.Start("https://youtube.com/watch?v=RfDTdiBq4_o");
				Process.Start("https://keyauth.cc/app/");
				Thread.Sleep(2000);
				api.error("Application not setup correctly. Please watch the YouTube video for setup.");
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			this.name = name;
			this.ownerid = ownerid;
			this.version = version;
			this.path = path;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002100 File Offset: 0x00000300
		public void init()
		{
			Random random = new Random();
			int num = random.Next(5, 51);
			StringBuilder stringBuilder = new StringBuilder(num);
			for (int i = 0; i < num; i++)
			{
				char c = (char)random.Next(32, 127);
				stringBuilder.Append(c);
			}
			this.seed = stringBuilder.ToString();
			this.checkAtom();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "init";
			nameValueCollection["ver"] = this.version;
			nameValueCollection["hash"] = api.checksum(Process.GetCurrentProcess().MainModule.FileName);
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			bool flag = !string.IsNullOrEmpty(this.path);
			if (flag)
			{
				nameValueCollection2.Add("token", File.ReadAllText(this.path));
				nameValueCollection2.Add("thash", api.TokenHash(this.path));
			}
			string text = api.req(nameValueCollection2);
			bool flag2 = text == "KeyAuth_Invalid";
			if (flag2)
			{
				api.error("Application not found");
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag3 = response_structure.ownerid == this.ownerid;
			if (flag3)
			{
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					api.sessionid = response_structure.sessionid;
					this.initialized = true;
				}
				else
				{
					bool flag4 = response_structure.message == "invalidver";
					if (flag4)
					{
						this.app_data.downloadLink = response_structure.download;
					}
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000022DC File Offset: 0x000004DC
		private void checkAtom()
		{
			new Thread(delegate
			{
				for (;;)
				{
					Thread.Sleep(60000);
					ushort num = api.GlobalFindAtom(this.seed);
					bool flag = num == 0;
					if (flag)
					{
						api.TerminateProcess(api.GetCurrentProcess(), 1U);
					}
				}
			})
			{
				IsBackground = true
			}.Start();
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000230C File Offset: 0x0000050C
		public static string TokenHash(string tokenPath)
		{
			string text;
			using (SHA256 sha = SHA256.Create())
			{
				using (FileStream fileStream = File.OpenRead(tokenPath))
				{
					byte[] array = sha.ComputeHash(fileStream);
					text = BitConverter.ToString(array).Replace("-", string.Empty);
				}
			}
			return text;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000237C File Offset: 0x0000057C
		public void CheckInit()
		{
			bool flag = !this.initialized;
			if (flag)
			{
				api.error("You must run the function KeyAuthApp.init(); first");
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000023B0 File Offset: 0x000005B0
		public string expirydaysleft(string Type, int subscription)
		{
			this.CheckInit();
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			dateTime = dateTime.AddSeconds((double)long.Parse(this.user_data.subscriptions[subscription].expiry)).ToLocalTime();
			TimeSpan timeSpan = dateTime - DateTime.Now;
			string text = Type.ToLower();
			string text2 = text;
			string text3;
			if (!(text2 == "months"))
			{
				if (!(text2 == "days"))
				{
					if (!(text2 == "hours"))
					{
						text3 = null;
					}
					else
					{
						text3 = Convert.ToString(timeSpan.Hours);
					}
				}
				else
				{
					text3 = Convert.ToString(timeSpan.Days);
				}
			}
			else
			{
				text3 = Convert.ToString(timeSpan.Days / 30);
			}
			return text3;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002480 File Offset: 0x00000680
		public void register(string username, string pass, string key, string email = "")
		{
			this.CheckInit();
			string value = WindowsIdentity.GetCurrent().User.Value;
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "register";
			nameValueCollection["username"] = username;
			nameValueCollection["pass"] = pass;
			nameValueCollection["key"] = key;
			nameValueCollection["email"] = email;
			nameValueCollection["hwid"] = value;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				api.GlobalAddAtom(this.seed);
				api.GlobalAddAtom(this.ownerid);
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					this.load_user_data(response_structure.info);
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000025AC File Offset: 0x000007AC
		public void forgot(string username, string email)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "forgot";
			nameValueCollection["username"] = username;
			nameValueCollection["email"] = email;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002644 File Offset: 0x00000844
		public void login(string username, string pass)
		{
			this.CheckInit();
			string value = WindowsIdentity.GetCurrent().User.Value;
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "login";
			nameValueCollection["username"] = username;
			nameValueCollection["pass"] = pass;
			nameValueCollection["hwid"] = value;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				api.GlobalAddAtom(this.seed);
				api.GlobalAddAtom(this.ownerid);
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					this.load_user_data(response_structure.info);
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002754 File Offset: 0x00000954
		public void logout()
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "logout";
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000027F8 File Offset: 0x000009F8
		public void web_login()
		{
			this.CheckInit();
			string value = WindowsIdentity.GetCurrent().User.Value;
			HttpListener httpListener;
			HttpListenerRequest request;
			HttpListenerResponse httpListenerResponse;
			for (;;)
			{
				httpListener = new HttpListener();
				string text = "handshake";
				text = "http://localhost:1337/" + text + "/";
				httpListener.Prefixes.Add(text);
				httpListener.Start();
				HttpListenerContext context = httpListener.GetContext();
				request = context.Request;
				httpListenerResponse = context.Response;
				httpListenerResponse.AddHeader("Access-Control-Allow-Methods", "GET, POST");
				httpListenerResponse.AddHeader("Access-Control-Allow-Origin", "*");
				httpListenerResponse.AddHeader("Via", "hugzho's big brain");
				httpListenerResponse.AddHeader("Location", "your kernel ;)");
				httpListenerResponse.AddHeader("Retry-After", "never lmao");
				httpListenerResponse.Headers.Add("Server", "\r\n\r\n");
				bool flag = request.HttpMethod == "OPTIONS";
				if (!flag)
				{
					break;
				}
				httpListenerResponse.StatusCode = 200;
				Thread.Sleep(1);
				httpListener.Stop();
			}
			httpListener.AuthenticationSchemes = AuthenticationSchemes.Negotiate;
			httpListener.UnsafeConnectionNtlmAuthentication = true;
			httpListener.IgnoreWriteExceptions = true;
			string rawUrl = request.RawUrl;
			string text2 = rawUrl.Replace("/handshake?user=", "");
			text2 = text2.Replace("&token=", " ");
			string text3 = text2;
			string text4 = text3.Split(Array.Empty<char>())[0];
			string text5 = text3.Split(new char[] { ' ' })[1];
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "login";
			nameValueCollection["username"] = text4;
			nameValueCollection["token"] = text5;
			nameValueCollection["hwid"] = value;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text6 = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text6);
			bool flag2 = true;
			bool flag3 = response_structure.ownerid == this.ownerid;
			if (flag3)
			{
				api.GlobalAddAtom(this.seed);
				api.GlobalAddAtom(this.ownerid);
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					this.load_user_data(response_structure.info);
					httpListenerResponse.StatusCode = 420;
					httpListenerResponse.StatusDescription = "SHEESH";
				}
				else
				{
					Console.WriteLine(response_structure.message);
					httpListenerResponse.StatusCode = 200;
					httpListenerResponse.StatusDescription = response_structure.message;
					flag2 = false;
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			byte[] bytes = Encoding.UTF8.GetBytes("Complete");
			httpListenerResponse.ContentLength64 = (long)bytes.Length;
			Stream outputStream = httpListenerResponse.OutputStream;
			outputStream.Write(bytes, 0, bytes.Length);
			Thread.Sleep(1);
			httpListener.Stop();
			bool flag4 = !flag2;
			if (flag4)
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002B28 File Offset: 0x00000D28
		public void button(string button)
		{
			this.CheckInit();
			HttpListener httpListener = new HttpListener();
			string text = "http://localhost:1337/" + button + "/";
			httpListener.Prefixes.Add(text);
			httpListener.Start();
			HttpListenerContext context = httpListener.GetContext();
			HttpListenerRequest request = context.Request;
			HttpListenerResponse httpListenerResponse = context.Response;
			httpListenerResponse.AddHeader("Access-Control-Allow-Methods", "GET, POST");
			httpListenerResponse.AddHeader("Access-Control-Allow-Origin", "*");
			httpListenerResponse.AddHeader("Via", "hugzho's big brain");
			httpListenerResponse.AddHeader("Location", "your kernel ;)");
			httpListenerResponse.AddHeader("Retry-After", "never lmao");
			httpListenerResponse.Headers.Add("Server", "\r\n\r\n");
			httpListenerResponse.StatusCode = 420;
			httpListenerResponse.StatusDescription = "SHEESH";
			httpListener.AuthenticationSchemes = AuthenticationSchemes.Negotiate;
			httpListener.UnsafeConnectionNtlmAuthentication = true;
			httpListener.IgnoreWriteExceptions = true;
			httpListener.Stop();
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002C2C File Offset: 0x00000E2C
		public void upgrade(string username, string key)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "upgrade";
			nameValueCollection["username"] = username;
			nameValueCollection["key"] = key;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				response_structure.success = false;
				this.load_response_struct(response_structure);
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002CF4 File Offset: 0x00000EF4
		public void license(string key)
		{
			this.CheckInit();
			string value = WindowsIdentity.GetCurrent().User.Value;
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "license";
			nameValueCollection["key"] = key;
			nameValueCollection["hwid"] = value;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				api.GlobalAddAtom(this.seed);
				api.GlobalAddAtom(this.ownerid);
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					this.load_user_data(response_structure.info);
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002DF8 File Offset: 0x00000FF8
		public void check()
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "check";
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002E9C File Offset: 0x0000109C
		public void setvar(string var, string data)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "setvar";
			nameValueCollection["var"] = var;
			nameValueCollection["data"] = data;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002F5C File Offset: 0x0000115C
		public string getvar(string var)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "getvar";
			nameValueCollection["var"] = var;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					return response_structure.response;
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			return null;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000302C File Offset: 0x0000122C
		public void ban(string reason = null)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "ban";
			nameValueCollection["reason"] = reason;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000030E0 File Offset: 0x000012E0
		public string var(string varid)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "var";
			nameValueCollection["varid"] = varid;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					return response_structure.message;
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			return null;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000031B0 File Offset: 0x000013B0
		public List<api.users> fetchOnline()
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "fetchOnline";
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
			bool success = response_structure.success;
			List<api.users> list;
			if (success)
			{
				list = response_structure.users;
			}
			else
			{
				list = null;
			}
			return list;
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00003248 File Offset: 0x00001448
		public void fetchStats()
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "fetchStats";
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
			bool success = response_structure.success;
			if (success)
			{
				this.load_app_data(response_structure.appinfo);
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000032DC File Offset: 0x000014DC
		public List<api.msg> chatget(string channelname)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "chatget";
			nameValueCollection["channel"] = channelname;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
			bool success = response_structure.success;
			List<api.msg> list;
			if (success)
			{
				list = response_structure.messages;
			}
			else
			{
				list = null;
			}
			return list;
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00003384 File Offset: 0x00001584
		public bool chatsend(string msg, string channelname)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "chatsend";
			nameValueCollection["message"] = msg;
			nameValueCollection["channel"] = channelname;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
			return response_structure.success;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00003434 File Offset: 0x00001634
		public bool checkblack()
		{
			this.CheckInit();
			string value = WindowsIdentity.GetCurrent().User.Value;
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "checkblacklist";
			nameValueCollection["hwid"] = value;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			bool flag2;
			if (flag)
			{
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				flag2 = success;
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
				flag2 = true;
			}
			return flag2;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003514 File Offset: 0x00001714
		public string webhook(string webid, string param, string body = "", string conttype = "")
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "webhook";
			nameValueCollection["webid"] = webid;
			nameValueCollection["params"] = param;
			nameValueCollection["body"] = body;
			nameValueCollection["conttype"] = conttype;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			bool flag = response_structure.ownerid == this.ownerid;
			if (flag)
			{
				this.load_response_struct(response_structure);
				bool success = response_structure.success;
				if (success)
				{
					return response_structure.response;
				}
			}
			else
			{
				api.TerminateProcess(api.GetCurrentProcess(), 1U);
			}
			return null;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000360C File Offset: 0x0000180C
		public byte[] download(string fileid)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "file";
			nameValueCollection["fileid"] = fileid;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
			bool success = response_structure.success;
			byte[] array;
			if (success)
			{
				array = encryption.str_to_byte_arr(response_structure.contents);
			}
			else
			{
				array = null;
			}
			return array;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000036B8 File Offset: 0x000018B8
		public void log(string message)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "log";
			nameValueCollection["pcuser"] = Environment.UserName;
			nameValueCollection["message"] = message;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			api.req(nameValueCollection2);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003740 File Offset: 0x00001940
		public void changeUsername(string username)
		{
			this.CheckInit();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection["type"] = "changeUsername";
			nameValueCollection["newUsername"] = username;
			nameValueCollection["sessionid"] = api.sessionid;
			nameValueCollection["name"] = this.name;
			nameValueCollection["ownerid"] = this.ownerid;
			NameValueCollection nameValueCollection2 = nameValueCollection;
			string text = api.req(nameValueCollection2);
			api.response_structure response_structure = this.response_decoder.string_to_generic<api.response_structure>(text);
			this.load_response_struct(response_structure);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000037CC File Offset: 0x000019CC
		public static string checksum(string filename)
		{
			string text;
			using (MD5 md = MD5.Create())
			{
				using (FileStream fileStream = File.OpenRead(filename))
				{
					byte[] array = md.ComputeHash(fileStream);
					text = BitConverter.ToString(array).Replace("-", "").ToLowerInvariant();
				}
			}
			return text;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x0000384C File Offset: 0x00001A4C
		public static void LogEvent(string content)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
			string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "KeyAuth", "debug", fileNameWithoutExtension);
			bool flag = !Directory.Exists(text);
			if (flag)
			{
				Directory.CreateDirectory(text);
			}
			string text2 = string.Format("{0:MMM_dd_yyyy}_logs.txt", DateTime.Now);
			string text3 = Path.Combine(text, text2);
			try
			{
				content = api.RedactField(content, "sessionid");
				content = api.RedactField(content, "ownerid");
				content = api.RedactField(content, "app");
				content = api.RedactField(content, "version");
				content = api.RedactField(content, "fileid");
				content = api.RedactField(content, "webhooks");
				content = api.RedactField(content, "nonce");
				using (StreamWriter streamWriter = File.AppendText(text3))
				{
					streamWriter.WriteLine(string.Format("[{0}] [{1}] {2}", DateTime.Now, AppDomain.CurrentDomain.FriendlyName, content));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error logging data: " + ex.Message);
			}
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00003990 File Offset: 0x00001B90
		private static string RedactField(string content, string fieldName)
		{
			string text = "\"" + fieldName + "\":\"[^\"]*\"";
			string text2 = "\"" + fieldName + "\":\"REDACTED\"";
			return Regex.Replace(content, text, text2);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000039CC File Offset: 0x00001BCC
		public static void error(string message)
		{
			string text = "Logs";
			string text2 = Path.Combine(text, "ErrorLogs.txt");
			bool flag = !Directory.Exists(text);
			if (flag)
			{
				Directory.CreateDirectory(text);
			}
			bool flag2 = !File.Exists(text2);
			if (flag2)
			{
				using (File.Create(text2))
				{
					File.AppendAllText(text2, DateTime.Now.ToString() + " > This is the start of your error logs file");
				}
			}
			File.AppendAllText(text2, DateTime.Now.ToString() + " > " + message + Environment.NewLine);
			Process.Start(new ProcessStartInfo("cmd.exe", "/c start cmd /C \"color b && title Error && echo " + message + " && timeout /t 5\"")
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			});
			api.TerminateProcess(api.GetCurrentProcess(), 1U);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00003ACC File Offset: 0x00001CCC
		private static string req(NameValueCollection post_data)
		{
			string text;
			try
			{
				using (WebClient webClient = new WebClient())
				{
					webClient.Proxy = null;
					ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(api.assertSSL));
					byte[] array = webClient.UploadValues("https://keyauth.win/api/1.3/", post_data);
					
					api.sigCheck(Encoding.UTF8.GetString(array), webClient.ResponseHeaders, post_data.Get(0));
					api.LogEvent(Encoding.Default.GetString(array) + "\n");
					text = Encoding.Default.GetString(array);
				}
			}
			catch (WebException ex)
			{
				HttpWebResponse httpWebResponse = (HttpWebResponse)ex.Response;
				HttpStatusCode statusCode = httpWebResponse.StatusCode;
				HttpStatusCode httpStatusCode = statusCode;
				if (httpStatusCode != (HttpStatusCode)429)
				{
					api.error("Connection failure. Please try again, or contact us for help.");
					api.LogEvent("Connection failure. Please try again, or contact us for help.");
					api.TerminateProcess(api.GetCurrentProcess(), 1U);
					text = "";
				}
				else
				{
					api.error("You're connecting too fast to loader, slow down.");
					api.LogEvent("You're connecting too fast to loader, slow down.");
					api.TerminateProcess(api.GetCurrentProcess(), 1U);
					text = "";
				}
			}
			return text;
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00003C2C File Offset: 0x00001E2C
		private static bool assertSSL(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool flag = (!certificate.Issuer.Contains("Google Trust Services") && !certificate.Issuer.Contains("Let's Encrypt")) || sslPolicyErrors > SslPolicyErrors.None;
			bool flag2;
			if (flag)
			{
				api.error("SSL assertion fail, make sure you're not debugging Network. Disable internet firewall on router if possible. & echo: & echo If not, ask the developer of the program to use custom domains to fix this.");
				api.LogEvent("SSL assertion fail, make sure you're not debugging Network. Disable internet firewall on router if possible. If not, ask the developer of the program to use custom domains to fix this.");
				flag2 = false;
			}
			else
			{
				flag2 = true;
			}
			return flag2;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003C8C File Offset: 0x00001E8C
		private static void sigCheck(string resp, WebHeaderCollection headers, string type)
		{
			bool flag = type == "log" || type == "file";
			if (!flag)
			{
				try
				{
					string text = headers["x-signature-ed25519"];
					string text2 = headers["x-signature-timestamp"];
					long num;
					bool flag2 = !long.TryParse(text2, out num);
					if (flag2)
					{
						api.TerminateProcess(api.GetCurrentProcess(), 1U);
					}
					DateTime utcDateTime = DateTimeOffset.FromUnixTimeSeconds(num).UtcDateTime;
					DateTime utcNow = DateTime.UtcNow;
					bool flag3 = (utcNow - utcDateTime).TotalSeconds > 20.0;
					if (flag3)
					{
						api.TerminateProcess(api.GetCurrentProcess(), 1U);
					}
					byte[] array = encryption.str_to_byte_arr(text);
					byte[] array2 = encryption.str_to_byte_arr("5586b4bc69c7a4b487e4563a4cd96afd39140f919bd31cea7d1c6a1e8439422b");
					string text3 = text2 + resp;
					byte[] bytes = Encoding.Default.GetBytes(text3);
					Console.Write(" Authenticating");
					bool flag4 = Ed25519.CheckValid(array, bytes, array2);
					bool flag5 = !flag4;
					if (flag5)
					{
						api.error("Signature checksum failed. Request was tampered with or session ended most likely. & echo: & echo Response: " + resp);
						api.LogEvent(resp + "\n");
						api.TerminateProcess(api.GetCurrentProcess(), 1U);
					}
				}
				catch
				{
					api.error("Signature checksum failed. Request was tampered with or session ended most likely. & echo: & echo Response: " + resp);
					api.LogEvent(resp + "\n");
					api.TerminateProcess(api.GetCurrentProcess(), 1U);
				}
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003E14 File Offset: 0x00002014
		private void load_app_data(api.app_data_structure data)
		{
			this.app_data.numUsers = data.numUsers;
			this.app_data.numOnlineUsers = data.numOnlineUsers;
			this.app_data.numKeys = data.numKeys;
			this.app_data.version = data.version;
			this.app_data.customerPanelLink = data.customerPanelLink;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003E7C File Offset: 0x0000207C
		private void load_user_data(api.user_data_structure data)
		{
			this.user_data.username = data.username;
			this.user_data.ip = data.ip;
			this.user_data.hwid = data.hwid;
			this.user_data.createdate = data.createdate;
			this.user_data.lastlogin = data.lastlogin;
			this.user_data.subscriptions = data.subscriptions;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003EF6 File Offset: 0x000020F6
		private void load_response_struct(api.response_structure data)
		{
			this.response.success = data.success;
			this.response.message = data.message;
		}

		// Token: 0x04000001 RID: 1
		public string name;

		// Token: 0x04000002 RID: 2
		public string ownerid;

		// Token: 0x04000003 RID: 3
		public string version;

		// Token: 0x04000004 RID: 4
		public string path;

		// Token: 0x04000005 RID: 5
		public string seed;

		// Token: 0x04000006 RID: 6
		private static string sessionid;

		// Token: 0x04000007 RID: 7
		private static string enckey;

		// Token: 0x04000008 RID: 8
		private bool initialized;

		// Token: 0x04000009 RID: 9
		public api.app_data_class app_data = new api.app_data_class();

		// Token: 0x0400000A RID: 10
		public api.user_data_class user_data = new api.user_data_class();

		// Token: 0x0400000B RID: 11
		public api.response_class response = new api.response_class();

		// Token: 0x0400000C RID: 12
		private json_wrapper response_decoder = new json_wrapper(new api.response_structure());

		// Token: 0x0200000A RID: 10
		[DataContract]
		private class response_structure
		{
			// Token: 0x17000001 RID: 1
			// (get) Token: 0x06000062 RID: 98 RVA: 0x000055E6 File Offset: 0x000037E6
			// (set) Token: 0x06000063 RID: 99 RVA: 0x000055EE File Offset: 0x000037EE
			[DataMember]
			public bool success { get; set; }

			// Token: 0x17000002 RID: 2
			// (get) Token: 0x06000064 RID: 100 RVA: 0x000055F7 File Offset: 0x000037F7
			// (set) Token: 0x06000065 RID: 101 RVA: 0x000055FF File Offset: 0x000037FF
			[DataMember]
			public bool newSession { get; set; }

			// Token: 0x17000003 RID: 3
			// (get) Token: 0x06000066 RID: 102 RVA: 0x00005608 File Offset: 0x00003808
			// (set) Token: 0x06000067 RID: 103 RVA: 0x00005610 File Offset: 0x00003810
			[DataMember]
			public string sessionid { get; set; }

			// Token: 0x17000004 RID: 4
			// (get) Token: 0x06000068 RID: 104 RVA: 0x00005619 File Offset: 0x00003819
			// (set) Token: 0x06000069 RID: 105 RVA: 0x00005621 File Offset: 0x00003821
			[DataMember]
			public string contents { get; set; }

			// Token: 0x17000005 RID: 5
			// (get) Token: 0x0600006A RID: 106 RVA: 0x0000562A File Offset: 0x0000382A
			// (set) Token: 0x0600006B RID: 107 RVA: 0x00005632 File Offset: 0x00003832
			[DataMember]
			public string response { get; set; }

			// Token: 0x17000006 RID: 6
			// (get) Token: 0x0600006C RID: 108 RVA: 0x0000563B File Offset: 0x0000383B
			// (set) Token: 0x0600006D RID: 109 RVA: 0x00005643 File Offset: 0x00003843
			[DataMember]
			public string message { get; set; }

			// Token: 0x17000007 RID: 7
			// (get) Token: 0x0600006E RID: 110 RVA: 0x0000564C File Offset: 0x0000384C
			// (set) Token: 0x0600006F RID: 111 RVA: 0x00005654 File Offset: 0x00003854
			[DataMember]
			public string ownerid { get; set; }

			// Token: 0x17000008 RID: 8
			// (get) Token: 0x06000070 RID: 112 RVA: 0x0000565D File Offset: 0x0000385D
			// (set) Token: 0x06000071 RID: 113 RVA: 0x00005665 File Offset: 0x00003865
			[DataMember]
			public string download { get; set; }

			// Token: 0x17000009 RID: 9
			// (get) Token: 0x06000072 RID: 114 RVA: 0x0000566E File Offset: 0x0000386E
			// (set) Token: 0x06000073 RID: 115 RVA: 0x00005676 File Offset: 0x00003876
			[DataMember(IsRequired = false, EmitDefaultValue = false)]
			public api.user_data_structure info { get; set; }

			// Token: 0x1700000A RID: 10
			// (get) Token: 0x06000074 RID: 116 RVA: 0x0000567F File Offset: 0x0000387F
			// (set) Token: 0x06000075 RID: 117 RVA: 0x00005687 File Offset: 0x00003887
			[DataMember(IsRequired = false, EmitDefaultValue = false)]
			public api.app_data_structure appinfo { get; set; }

			// Token: 0x1700000B RID: 11
			// (get) Token: 0x06000076 RID: 118 RVA: 0x00005690 File Offset: 0x00003890
			// (set) Token: 0x06000077 RID: 119 RVA: 0x00005698 File Offset: 0x00003898
			[DataMember]
			public List<api.msg> messages { get; set; }

			// Token: 0x1700000C RID: 12
			// (get) Token: 0x06000078 RID: 120 RVA: 0x000056A1 File Offset: 0x000038A1
			// (set) Token: 0x06000079 RID: 121 RVA: 0x000056A9 File Offset: 0x000038A9
			[DataMember]
			public List<api.users> users { get; set; }
		}

		// Token: 0x0200000B RID: 11
		public class msg
		{
			// Token: 0x1700000D RID: 13
			// (get) Token: 0x0600007B RID: 123 RVA: 0x000056BB File Offset: 0x000038BB
			// (set) Token: 0x0600007C RID: 124 RVA: 0x000056C3 File Offset: 0x000038C3
			public string message { get; set; }

			// Token: 0x1700000E RID: 14
			// (get) Token: 0x0600007D RID: 125 RVA: 0x000056CC File Offset: 0x000038CC
			// (set) Token: 0x0600007E RID: 126 RVA: 0x000056D4 File Offset: 0x000038D4
			public string author { get; set; }

			// Token: 0x1700000F RID: 15
			// (get) Token: 0x0600007F RID: 127 RVA: 0x000056DD File Offset: 0x000038DD
			// (set) Token: 0x06000080 RID: 128 RVA: 0x000056E5 File Offset: 0x000038E5
			public string timestamp { get; set; }
		}

		// Token: 0x0200000C RID: 12
		public class users
		{
			// Token: 0x17000010 RID: 16
			// (get) Token: 0x06000082 RID: 130 RVA: 0x000056F7 File Offset: 0x000038F7
			// (set) Token: 0x06000083 RID: 131 RVA: 0x000056FF File Offset: 0x000038FF
			public string credential { get; set; }
		}

		// Token: 0x0200000D RID: 13
		[DataContract]
		private class user_data_structure
		{
			// Token: 0x17000011 RID: 17
			// (get) Token: 0x06000085 RID: 133 RVA: 0x00005711 File Offset: 0x00003911
			// (set) Token: 0x06000086 RID: 134 RVA: 0x00005719 File Offset: 0x00003919
			[DataMember]
			public string username { get; set; }

			// Token: 0x17000012 RID: 18
			// (get) Token: 0x06000087 RID: 135 RVA: 0x00005722 File Offset: 0x00003922
			// (set) Token: 0x06000088 RID: 136 RVA: 0x0000572A File Offset: 0x0000392A
			[DataMember]
			public string ip { get; set; }

			// Token: 0x17000013 RID: 19
			// (get) Token: 0x06000089 RID: 137 RVA: 0x00005733 File Offset: 0x00003933
			// (set) Token: 0x0600008A RID: 138 RVA: 0x0000573B File Offset: 0x0000393B
			[DataMember]
			public string hwid { get; set; }

			// Token: 0x17000014 RID: 20
			// (get) Token: 0x0600008B RID: 139 RVA: 0x00005744 File Offset: 0x00003944
			// (set) Token: 0x0600008C RID: 140 RVA: 0x0000574C File Offset: 0x0000394C
			[DataMember]
			public string createdate { get; set; }

			// Token: 0x17000015 RID: 21
			// (get) Token: 0x0600008D RID: 141 RVA: 0x00005755 File Offset: 0x00003955
			// (set) Token: 0x0600008E RID: 142 RVA: 0x0000575D File Offset: 0x0000395D
			[DataMember]
			public string lastlogin { get; set; }

			// Token: 0x17000016 RID: 22
			// (get) Token: 0x0600008F RID: 143 RVA: 0x00005766 File Offset: 0x00003966
			// (set) Token: 0x06000090 RID: 144 RVA: 0x0000576E File Offset: 0x0000396E
			[DataMember]
			public List<api.Data> subscriptions { get; set; }
		}

		// Token: 0x0200000E RID: 14
		[DataContract]
		private class app_data_structure
		{
			// Token: 0x17000017 RID: 23
			// (get) Token: 0x06000092 RID: 146 RVA: 0x00005780 File Offset: 0x00003980
			// (set) Token: 0x06000093 RID: 147 RVA: 0x00005788 File Offset: 0x00003988
			[DataMember]
			public string numUsers { get; set; }

			// Token: 0x17000018 RID: 24
			// (get) Token: 0x06000094 RID: 148 RVA: 0x00005791 File Offset: 0x00003991
			// (set) Token: 0x06000095 RID: 149 RVA: 0x00005799 File Offset: 0x00003999
			[DataMember]
			public string numOnlineUsers { get; set; }

			// Token: 0x17000019 RID: 25
			// (get) Token: 0x06000096 RID: 150 RVA: 0x000057A2 File Offset: 0x000039A2
			// (set) Token: 0x06000097 RID: 151 RVA: 0x000057AA File Offset: 0x000039AA
			[DataMember]
			public string numKeys { get; set; }

			// Token: 0x1700001A RID: 26
			// (get) Token: 0x06000098 RID: 152 RVA: 0x000057B3 File Offset: 0x000039B3
			// (set) Token: 0x06000099 RID: 153 RVA: 0x000057BB File Offset: 0x000039BB
			[DataMember]
			public string version { get; set; }

			// Token: 0x1700001B RID: 27
			// (get) Token: 0x0600009A RID: 154 RVA: 0x000057C4 File Offset: 0x000039C4
			// (set) Token: 0x0600009B RID: 155 RVA: 0x000057CC File Offset: 0x000039CC
			[DataMember]
			public string customerPanelLink { get; set; }

			// Token: 0x1700001C RID: 28
			// (get) Token: 0x0600009C RID: 156 RVA: 0x000057D5 File Offset: 0x000039D5
			// (set) Token: 0x0600009D RID: 157 RVA: 0x000057DD File Offset: 0x000039DD
			[DataMember]
			public string downloadLink { get; set; }
		}

		// Token: 0x0200000F RID: 15
		public class app_data_class
		{
			// Token: 0x1700001D RID: 29
			// (get) Token: 0x0600009F RID: 159 RVA: 0x000057EF File Offset: 0x000039EF
			// (set) Token: 0x060000A0 RID: 160 RVA: 0x000057F7 File Offset: 0x000039F7
			public string numUsers { get; set; }

			// Token: 0x1700001E RID: 30
			// (get) Token: 0x060000A1 RID: 161 RVA: 0x00005800 File Offset: 0x00003A00
			// (set) Token: 0x060000A2 RID: 162 RVA: 0x00005808 File Offset: 0x00003A08
			public string numOnlineUsers { get; set; }

			// Token: 0x1700001F RID: 31
			// (get) Token: 0x060000A3 RID: 163 RVA: 0x00005811 File Offset: 0x00003A11
			// (set) Token: 0x060000A4 RID: 164 RVA: 0x00005819 File Offset: 0x00003A19
			public string numKeys { get; set; }

			// Token: 0x17000020 RID: 32
			// (get) Token: 0x060000A5 RID: 165 RVA: 0x00005822 File Offset: 0x00003A22
			// (set) Token: 0x060000A6 RID: 166 RVA: 0x0000582A File Offset: 0x00003A2A
			public string version { get; set; }

			// Token: 0x17000021 RID: 33
			// (get) Token: 0x060000A7 RID: 167 RVA: 0x00005833 File Offset: 0x00003A33
			// (set) Token: 0x060000A8 RID: 168 RVA: 0x0000583B File Offset: 0x00003A3B
			public string customerPanelLink { get; set; }

			// Token: 0x17000022 RID: 34
			// (get) Token: 0x060000A9 RID: 169 RVA: 0x00005844 File Offset: 0x00003A44
			// (set) Token: 0x060000AA RID: 170 RVA: 0x0000584C File Offset: 0x00003A4C
			public string downloadLink { get; set; }
		}

		// Token: 0x02000010 RID: 16
		public class user_data_class
		{
			// Token: 0x17000023 RID: 35
			// (get) Token: 0x060000AC RID: 172 RVA: 0x0000585E File Offset: 0x00003A5E
			// (set) Token: 0x060000AD RID: 173 RVA: 0x00005866 File Offset: 0x00003A66
			public string username { get; set; }

			// Token: 0x17000024 RID: 36
			// (get) Token: 0x060000AE RID: 174 RVA: 0x0000586F File Offset: 0x00003A6F
			// (set) Token: 0x060000AF RID: 175 RVA: 0x00005877 File Offset: 0x00003A77
			public string ip { get; set; }

			// Token: 0x17000025 RID: 37
			// (get) Token: 0x060000B0 RID: 176 RVA: 0x00005880 File Offset: 0x00003A80
			// (set) Token: 0x060000B1 RID: 177 RVA: 0x00005888 File Offset: 0x00003A88
			public string hwid { get; set; }

			// Token: 0x17000026 RID: 38
			// (get) Token: 0x060000B2 RID: 178 RVA: 0x00005891 File Offset: 0x00003A91
			// (set) Token: 0x060000B3 RID: 179 RVA: 0x00005899 File Offset: 0x00003A99
			public string createdate { get; set; }

			// Token: 0x17000027 RID: 39
			// (get) Token: 0x060000B4 RID: 180 RVA: 0x000058A2 File Offset: 0x00003AA2
			// (set) Token: 0x060000B5 RID: 181 RVA: 0x000058AA File Offset: 0x00003AAA
			public string lastlogin { get; set; }

			// Token: 0x17000028 RID: 40
			// (get) Token: 0x060000B6 RID: 182 RVA: 0x000058B3 File Offset: 0x00003AB3
			// (set) Token: 0x060000B7 RID: 183 RVA: 0x000058BB File Offset: 0x00003ABB
			public List<api.Data> subscriptions { get; set; }
		}

		// Token: 0x02000011 RID: 17
		public class Data
		{
			// Token: 0x17000029 RID: 41
			// (get) Token: 0x060000B9 RID: 185 RVA: 0x000058CD File Offset: 0x00003ACD
			// (set) Token: 0x060000BA RID: 186 RVA: 0x000058D5 File Offset: 0x00003AD5
			public string subscription { get; set; }

			// Token: 0x1700002A RID: 42
			// (get) Token: 0x060000BB RID: 187 RVA: 0x000058DE File Offset: 0x00003ADE
			// (set) Token: 0x060000BC RID: 188 RVA: 0x000058E6 File Offset: 0x00003AE6
			public string expiry { get; set; }

			// Token: 0x1700002B RID: 43
			// (get) Token: 0x060000BD RID: 189 RVA: 0x000058EF File Offset: 0x00003AEF
			// (set) Token: 0x060000BE RID: 190 RVA: 0x000058F7 File Offset: 0x00003AF7
			public string timeleft { get; set; }
		}

		// Token: 0x02000012 RID: 18
		public class response_class
		{
			// Token: 0x1700002C RID: 44
			// (get) Token: 0x060000C0 RID: 192 RVA: 0x00005909 File Offset: 0x00003B09
			// (set) Token: 0x060000C1 RID: 193 RVA: 0x00005911 File Offset: 0x00003B11
			public bool success { get; set; }

			// Token: 0x1700002D RID: 45
			// (get) Token: 0x060000C2 RID: 194 RVA: 0x0000591A File Offset: 0x00003B1A
			// (set) Token: 0x060000C3 RID: 195 RVA: 0x00005922 File Offset: 0x00003B22
			public string message { get; set; }
		}
	}
}
