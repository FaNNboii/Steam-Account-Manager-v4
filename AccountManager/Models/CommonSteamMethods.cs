using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using System.Text.RegularExpressions;

namespace AccountManager.Models
{
    public class CommonSteamMethods
    {
        public static bool IsBase64(string base64String)
        {
            if (base64String == null || base64String.Length == 0 || base64String.Length % 4 != 0
               || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetSteamPath()
        {
            RegistryKey registryKey = Registry.CurrentUser;
            registryKey = registryKey.OpenSubKey("Software\\Valve\\Steam");
            if (registryKey != null)
            {
                return registryKey.GetValue("SteamExe").ToString();
            }
            return "";
        }

        public static bool Login(Account a)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = GetSteamPath();
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.Arguments = "-login " + a.Username + " " + a.Password;
            try
            {
                Process[] processesByName = Process.GetProcessesByName("Steam");
                for (int i = 0; i < processesByName.Length; i++)
                {
                    Process process = processesByName[i];
                    process.Kill();
                }
                Process.Start(processStartInfo);

                // Steam will never start faster than 1 second

                if (string.IsNullOrWhiteSpace(a.AuthKey))
                    return true;

                Thread.Sleep(1000);

                string[] names = { "Steam Guard - Computerautorisierung erforderlich", "Steam Guard - Computer Authorization Required" };

                IntPtr window = new IntPtr(0);

                int maxSearches = 150;
                int currentSearch = 0;

                while (window.ToInt32() == 0 && ++currentSearch <= maxSearches)
                {
                    Thread.Sleep(200);
                    for (int i = 0; i < names.Length && window.ToInt32() == 0; i++)
                    {
                        window = FindWindow(null, names[i]);
                    }
                }

                if (window.ToInt32() == 0)
                {
                    return false;
                }

                long time = Models.SteamTwoFactor.GetSteamTime();
                string code = Models.SteamTwoFactor.GenerateSteamGuardCodeForTime(time, a.AuthKey);

                InputSimulator iss = new InputSimulator();
                KeyboardSimulator s = new KeyboardSimulator(iss);
                var chars = new VirtualKeyCode[code.Length + 1];
                for (int i = 0; i < code.Length; i++)
                {
                    chars[i] = (VirtualKeyCode)code[i];
                }

                chars[chars.Length - 1] = VirtualKeyCode.RETURN;

                SetForegroundWindow(window);

                s.KeyPress(chars);
            }
            catch
            {
                return false;
            }
            

            return true;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, String lpWindowName);
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static string CalculateProfId(string steamID)
        {
            try
            {
                string[] teile = steamID.Split(':');
                long t2 = long.Parse(teile[2]);
                long t1 = long.Parse(teile[1]);

                long ergebnis = t2 * 2 + 76561197960265728 + t1;

                return ergebnis.ToString();
            }
            catch
            {
                return "";
            }
        }

        public static bool IsValidSteamID(string id)
        {
            return Regex.IsMatch(id, @"STEAM_0:(0|1):\d{1,}");
        }
        public static bool IsValidProfileID(string id)
        {
            id.TrimStart('0');
            if(!Regex.IsMatch(id, @"\d{17,19}"))
            {
                return false;
            }

            long lId = long.Parse(id);
            return lId > 76561197960265728;
        }

        public static string CalculateSteamID(string profID)
        {
            if (!string.IsNullOrWhiteSpace(profID))
            {
                long profid = long.Parse(profID);
                string steamid = "STEAM_0:";

                if (profid % 2 == 0)
                {
                    steamid += "0:";
                }
                else
                {
                    steamid += "1:";
                }

                profid -= 76561197960265728;
                steamid += profid / 2;

                return steamid;
            }

            return "";
        }
    }
}
