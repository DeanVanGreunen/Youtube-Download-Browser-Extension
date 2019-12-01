using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace YTDLBE
{
    public partial class Form1 : Form
    {

        private string __DIR__ = "";
        private string __INSTALL_DIR__ = Path.GetFullPath(Environment.ExpandEnvironmentVariables("%AppData%\\..\\Local\\Programs\\YTExtension"));
        private string __BAT_FILE__ = "set \"string=%1\"\r\nset \"link=%string:~3%\"\r\npython -m youtube_dl --restrict-filenames --ignore-errors -x --audio-format mp3 -o \"\"__DIR__%%(title)s.%%(ext)s\"\" %link%\r\ntimeout 5";
        private string __REG_KEY_FILE__ = "" +
            "Windows Registry Editor Version 5.00\r\n" +
            "[HKEY_CLASSES_ROOT\\yt]\r\n" +
            "@=\"URL: Yotube MP3 Download Protocol\"\r\n" +
            "\"URL Protocol\"=\"\"\r\n" +
            "[HKEY_CLASSES_ROOT\\yt\\shell]\r\n" +
            "[HKEY_CLASSES_ROOT\\yt\\shell\\open]\r\n" +
            "[HKEY_CLASSES_ROOT\\yt\\shell\\open\\command]\r\n" +
            "@=\\\"\"__INSTALL_DIR__\\yt-dl.bat\" %1\"";
        private string python_exe = "https://www.python.org/ftp/python/3.8.0/python-3.8.0.exe";


        public Form1()
        {
            InitializeComponent();
            txtBrowse.Text = "";
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            this.btnInstall.Enabled = false;
            this.btnCancel.Enabled = false;
            this.lmsg.Enabled = false;
            this.lmsg2.Enabled = true;
            string path = txtBrowse.Text;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Directory Doesn't Exist", string.Format("\"{0}\" is missing", path));
                return;
            }

            var x = StartInstaller();
            if (x == 0)
            {
                MessageBox.Show("Installation Completed Succefully\r\nClick OK to exit", "Installation Succefully");
                Application.Exit();
            }
            else
            {
                MessageBox.Show(string.Format("Error Code 0x{0}", x), "An Error Occured");
            }
        }

        private int StartInstaller()
        {
            pgInstall.Value = 0;
            //Install Python 0x1
            var python = InstallPython();
            if (python != 0) { return 1; }
            pgInstall.Value = 16;
            Application.DoEvents();
            Thread.Sleep(1500);
            //Install PIP 0x2
            var python_pip = InstallPythonPIP();
            if (python_pip != 0) { return 2; }
            pgInstall.Value = 32; Application.DoEvents();
            Thread.Sleep(1500);
            //PIP Install Youtube_dl 0x3
            var python_pip_youtube_dl = InstallPythonPIPYoutubeDL();
            if (python_pip_youtube_dl != 0) { return 3; }
            pgInstall.Value = 48;
            Application.DoEvents();
            Thread.Sleep(1500);
            //Create Bat File From Template 0x4
            var bat_from_template = InstallCreateBat();
            if (bat_from_template != 0) { return 4; }
            pgInstall.Value = 64; Application.DoEvents();
            Thread.Sleep(1500);
            //Create Register Key File From Template 0x5
            var reg_key_from_template = InstallCreateRegKey();
            if (reg_key_from_template != 0) { return 5; }
            pgInstall.Value = 80; Application.DoEvents();
            Thread.Sleep(1500);
            //Merge Key into Register 0x6
            var merge_reg_key = InstallMergeRegKey();
            if (merge_reg_key != 0) { return 6; }
            pgInstall.Value = 100; Application.DoEvents();
            Thread.Sleep(1500);
            return 0; //Succesfull 0x0
        }

        private int InstallPython()
        {
            try {
                using (WebClient myWebClient = new WebClient())
                {
                    string myStringWebResource = python_exe;
                    myWebClient.DownloadFile(myStringWebResource, "python-setup.exe");
                }
                Process regeditProcess = Process.Start("python-setup.exe", "/quiet");
                regeditProcess.WaitForExit();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
        private int InstallPythonPIP()
        {
            try {
                Process regeditProcess = Process.Start("python.exe", "-m pip install pip -U");
                regeditProcess.WaitForExit();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
        private int InstallPythonPIPYoutubeDL()
        {
            try
            {
                Process regeditProcess = Process.Start("python.exe", "-m pip install youtube_dl -U");
                regeditProcess.WaitForExit();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
        private int InstallCreateBat()
        {
            try { 
            CreateFolders(this.__INSTALL_DIR__);
            File.WriteAllText(this.__INSTALL_DIR__ + "\\yt-dl.bat", this.__BAT_FILE__.Replace("__DIR__", this.__DIR__));
            return 0;
            }
            catch
            {
                return 1;
            }
        }

        private int InstallCreateRegKey()
        {
            try
            {
                CreateFolders(this.__INSTALL_DIR__);
                this.__REG_KEY_FILE__ = this.__REG_KEY_FILE__.Replace("__INSTALL_DIR__", this.__INSTALL_DIR__);
                File.WriteAllText(this.__INSTALL_DIR__ + "\\key.reg", this.__REG_KEY_FILE__);
                return 0;
            }
            catch
            {
                return 1;
            }
        }
        
        private int InstallMergeRegKey()
        {
            try
            {
                Process regeditProcess = Process.Start("regedit.exe", "/s " + this.__INSTALL_DIR__ + "\\key.reg");
                regeditProcess.WaitForExit();
                return 0;
            }
            catch
            {
                return 1;
            }
        }

        private void CreateFolders(string path)
        {
            Directory.CreateDirectory(path);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select Your Music Folder";
            fbd.RootFolder = Environment.SpecialFolder.MyComputer;
            
            if(fbd.ShowDialog() == DialogResult.OK)
            {
                txtBrowse.Text = fbd.SelectedPath;
                this.__DIR__ = fbd.SelectedPath;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
