﻿using Quasar.Server.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Quasar.Server.Forms
{
    public partial class FrmCertificate : Form
    {
        private X509Certificate2 _certificate;

        public FrmCertificate()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            _certificate = CertificateHelper.CreateCertificateAuthority("Quasar Server CA", 4096);
            txtDetails.Text = _certificate.ToString(false);
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Filter = "*.p12|*.p12";
                ofd.Multiselect = false;
                ofd.InitialDirectory = Application.StartupPath;
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        _certificate = new X509Certificate2(ofd.FileName, "", X509KeyStorageFlags.Exportable);
                        txtDetails.Text = _certificate.ToString(false);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, $"Error importing the certificate:\n{ex.Message}", "Save error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (_certificate == null)
                    throw new ArgumentNullException();

                if (!_certificate.HasPrivateKey)
                    throw new ArgumentException();

                string path = Path.Combine(Application.StartupPath, "quasar.p12");
                File.WriteAllBytes(path, _certificate.Export(X509ContentType.Pkcs12));

                MessageBox.Show(this,
                    "Please backup the certificate now. Loss of the certificate results in loosing all clients!",
                    "Certificate backup", MessageBoxButtons.OK, MessageBoxIcon.Information);

                string argument = "/select, \"" + path + "\"";
                Process.Start("explorer.exe", argument);

                this.DialogResult = DialogResult.OK;
            }
            catch (ArgumentNullException)
            {
                MessageBox.Show(this, "Please create or import a certificate first.", "Save error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentException)
            {
                MessageBox.Show(this,
                    "The imported certificate has no associated private key. Please import a different certificate.",
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception)
            {
                MessageBox.Show(this,
                    "There was an error saving the certificate, please make sure you have write access to the Quasar directory.",
                    "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
