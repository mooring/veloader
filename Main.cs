using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;


namespace VELoader
{
    public partial class loader : Form
    {
        private Dictionary<string, string> config;
        public loader(string[] args)
        {
            InitializeComponent();
            this.config = new Dictionary<string, string>();
            this.initConfig(args);
            this.regProgram();
            this.startProgram();
        }
        private void initConfig(string[] args)
        {
            if (args.Length == 2)
            {
                this.config.Add("Program", args[0]);
                this.config.Add("Version", args[1]);
            }
            else
            {
                this.config.Add("Program", "VCSExpress");
                this.config.Add("Version", "8.0");
            }
            string sPlat = Environment.OSVersion.Platform.ToString();
        }
        private void regProgram()
        {
            string regPath = String.Format("Software\\Microsoft\\{0}\\{1}",this.config["Program"], this.config["Version"]);
            RegistryKey uReg = Registry.CurrentUser.OpenSubKey(regPath+"\\Registration", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (uReg == null)
            {
                uReg = Registry.CurrentUser.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                uReg.CreateSubKey("Registration");
                uReg = Registry.CurrentUser.OpenSubKey(regPath+"\\Registration", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            uReg.SetValue("Params", (object)"1");
            int ver = Int32.Parse(this.config["Version"].Split(new char[]{'.'})[0]);
            if (ver > 9)
            {
                string configPath = regPath.Replace(this.config["Version"], this.config["Version"]+"_Config");
                uReg = Registry.CurrentUser.OpenSubKey(configPath + "\\Registration", RegistryKeyPermissionCheck.ReadWriteSubTree);
                if (uReg == null)
                {
                    uReg.CreateSubKey("Registration");
                    uReg = Registry.CurrentUser.OpenSubKey(configPath + "\\Registration", RegistryKeyPermissionCheck.ReadWriteSubTree);
                }
                uReg.SetValue("Params", (object)"1");
            }
            if (IntPtr.Size == 8)
            {
                regPath = regPath.Replace("Software", "Software\\Wow6432Node");
            }
            RegistryKey sReg = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadSubTree);
            if (sReg == null)
            {
                MessageBox.Show(String.Format("Invalid Param \"{0}\"\t     ", this.config["Version"]), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.config.Add("InstallDir", (string)sReg.GetValue("InstallDir"));
        }
        private void startProgram()
        {
            Process[] targets = Process.GetProcessesByName(this.config["Program"]);
            ProcessStartInfo psta;
            foreach (Process pro in targets)
            {
                string proPath = pro.MainModule.FileName.ToString();
                if (proPath.IndexOf(this.config["InstallDir"], StringComparison.OrdinalIgnoreCase)>-1)
                {
                    MessageBox.Show("Target program already Running", "Warn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    return;
                }
            }
            if (this.config.ContainsKey("InstallDir"))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = String.Format("{0}\\{1}.exe", this.config["InstallDir"], this.config["Program"]);
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                try
                {
                    Process.Start(startInfo);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.ToString());
                }
                finally
                {
                    Application.Exit();
                }
            }
        }
    }
}