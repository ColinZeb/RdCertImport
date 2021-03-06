﻿using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RdCertImport
{
    public partial class Form1 : MaterialForm
    {
        public Form1()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            CertManager = new CertManager();
        }

        CertManager CertManager { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }


        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {

                var row = dataGridView1.SelectedRows[0];
                var hash = (string)row.Cells["Hash"].Value;
                certHash.Text = hash;
            }
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void mfbChoice_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Multiselect = false,
                CheckFileExists = true,
                DefaultExt = "pfx",
                Filter = "PFX(*.pfx)|*.pfx|CRT(*.crt)|*.crt|All files (*.*)|*.*"
            };
            var res = ofd.ShowDialog();
            if (res == DialogResult.OK)
            {
                CertFile = ofd.FileName;
                certFile.Text = ofd.SafeFileName;
                clearerror();
            }
        }
        private void clearerror()
        {
            errorProvider1.Clear();
        }
        public string CertFile { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        string Password
        {
            get
            {
                return materialSingleLineTextField2.Text;
            }
        }
        private void materialFlatButton1_Click(object sender, EventArgs e)
        {
            if (CheckFileExist())
            {
                try
                {
                    var cert = CertManager.OpenCert(CertFile, Password);
                    var hash = cert.GetCertHashString();
                    MessageBox.Show($"密码正确,哈希为:{hash}");
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {

                    errorProvider1.SetError(materialSingleLineTextField2, ex.Message);

                }
            }
        }

        public bool CheckFileExist()
        {
            clearerror();
            if (!File.Exists(CertFile))
            {
                errorProvider1.SetError(certFile, "文件不存在");
                return false;
            }
            return true;
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            ImportCert();
        }

        private void ImportCert()
        {
            if (CheckFileExist())
            {
                string hash = null;
                try
                {

                    var cert = CertManager.OpenCert(CertFile, Password);

                    hash = cert.GetCertHashString();
                    if (CertManager.Exist(cert))
                    {
                        //errorProvider1.SetError(materialRaisedButton1, "已经导入");

                        var choice = MessageBox.Show("已经存在该证书，是否重新导入\n选择是重新导入，选择否直接设置，选择取消终止操作", "证书已被导入", MessageBoxButtons.YesNoCancel);
                        if (choice == DialogResult.Cancel)
                        {
                            return;
                        }
                        if (choice == DialogResult.Yes)
                        {
                            CertManager.Remove(cert);
                            CertManager.ImportWithKey(cert);
                        }
                    }
                    else
                    {
                        CertManager.ImportWithKey(cert);
                    }


                    if (hash != null)
                    {
                        SetRdCertByHash(hash);
                    }
                }
                catch (Exception ex)
                {
                    errorProvider1.SetError(materialRaisedButton1, ex.Message);
                }

            }
        }

        private bool SetRdCertByHash(string hash)
        {
            var args = $@"/namespace:\\root\cimv2\TerminalServices PATH Win32_TSGeneralSetting Set SSLCertificateSHA1Hash=""{hash}""";

            Process ps = new Process();
            ps.StartInfo = new ProcessStartInfo("wmic", args)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };
            ps.Start();
            ps.WaitForExit(10000);
            if (!ps.HasExited)
            {
                errorProvider1.SetError(materialRaisedButton1, "超时");
                return false;
            }
            else
            {
                if (ps.ExitCode == 0)
                {
                    MessageBox.Show("操作成功，请重新连接远程桌面查看结果");
                    return true;
                }
                else
                {
                    MessageBox.Show($"可能出现了一些意外……\n详细信息：\n{ps.StandardOutput.ReadToEnd()}");
                    return false;
                }
            }
        }

        private void materialRaisedButton2_Click(object sender, EventArgs e)
        {
            if (!SetRdCertByHash(certHash.Text))
            {
                MessageBox.Show("或许是Hash不正确");
            }
        }

        private void materialFlatButton2_Click(object sender, EventArgs e)
        {
            if (CheckFileExist())
            {
                try
                {
                    var cert = CertManager.OpenCert(CertFile, Password);
                    var hash = cert.GetCertHashString();
                    Clipboard.SetText(hash);
                    MessageBox.Show("已复制到剪切板");
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {

                    errorProvider1.SetError(materialSingleLineTextField2, ex.Message);

                }
            }

        }

        private void materialSingleLineTextField1_Click(object sender, EventArgs e)
        {

        }

        private void materialSingleLineTextField1_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.Clear();
        }

        private void materialRaisedButton3_Click(object sender, EventArgs e)
        {

            dataGridView1.DataSource = GetData();
            //dataGridView1.DataBindings

        }

        public List<CertInfo> GetData()
        {
            return CertManager.GetMyCert();
        }
    }
}
