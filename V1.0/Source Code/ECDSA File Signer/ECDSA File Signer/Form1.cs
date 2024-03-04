using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Xml.Linq;

namespace ECDSA_File_Signer
{
    public partial class Form1 : Form
    {
        private Panel container;
        private Panel leftHalf;
        private ComboBox comboBox;
        private TextBox textBox;
        private System.Windows.Forms.Label lhlabel;
        private System.Windows.Forms.Label lhlabel1;
        private static string[] loaded_private_key;
        private static string[] loaded_public_key;

        public Form1()
        {
            InitializeComponent();
            InitializeGUI();
            lhlabel.Click += (sender, e) =>
            {
                open_file_private_key_selection_dialog();
            };
            lhlabel1.Click += (sender, e) =>
            {
                open_file_public_key_selection_dialog();
            };
            CenterContainer();
            this.Resize += (sender, e) => CenterContainer();
        }

        private void InitializeGUI()
        {
            this.BackColor = ColorTranslator.FromHtml("#0E71F3");

            container = new Panel();
            container.Size = new Size(940, 250);
            container.BackColor = ColorTranslator.FromHtml("#2C2C2C");
            container.BorderStyle = BorderStyle.None;
            container.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, container.Width, container.Height, 14, 14));
            this.Controls.Add(container);

            leftHalf = new Panel();
            leftHalf.Size = new Size(container.Width / 2, container.Height);
            leftHalf.BackColor = ColorTranslator.FromHtml("#2C2C2C");
            leftHalf.AllowDrop = true;
            leftHalf.DragEnter += new DragEventHandler(LeftHalf_DragEnter);
            leftHalf.DragDrop += new DragEventHandler(LeftHalf_DragDrop);
            container.Controls.Add(leftHalf);

            lhlabel = new System.Windows.Forms.Label();
            lhlabel.Text = "Drag && Drop Private Key Here\n\nor\n\nClick to Select One";
            lhlabel.Font = new Font("Segoe UI Semibold", 10, FontStyle.Regular);
            lhlabel.ForeColor = ColorTranslator.FromHtml("#EEEEEE");
            lhlabel.Dock = DockStyle.Fill; // This will make the label fill the entire space of the panel
            lhlabel.TextAlign = ContentAlignment.MiddleCenter; // This will center the text horizontally and vertically
            leftHalf.Controls.Add(lhlabel);

            Panel rightHalf = new Panel();
            rightHalf.Size = new Size(container.Width / 2, container.Height);
            rightHalf.Location = new Point(container.Width / 2, 0);
            rightHalf.BackColor = ColorTranslator.FromHtml("#EEEEEE");
            rightHalf.AllowDrop = true;
            rightHalf.DragEnter += new DragEventHandler(RightHalf_DragEnter);
            rightHalf.DragDrop += new DragEventHandler(RightHalf_DragDrop);
            container.Controls.Add(rightHalf);

            lhlabel1 = new System.Windows.Forms.Label();
            lhlabel1.Text = "Drag && Drop Public Key Here\n\nor\n\nClick to Select One";
            lhlabel1.Font = new Font("Segoe UI Semibold", 10, FontStyle.Regular);
            lhlabel1.ForeColor = ColorTranslator.FromHtml("#2C2C2C");
            lhlabel1.Dock = DockStyle.Fill; // This will make the label fill the entire space of the panel
            lhlabel1.TextAlign = ContentAlignment.MiddleCenter; // This will center the text horizontally and vertically
            rightHalf.Controls.Add(lhlabel1);

        }
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void CenterContainer()
        {
            container.Location = new Point((this.ClientSize.Width - container.Width) / 2, 99);
        }

        private void open_file_private_key_selection_dialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Private Key";
            openFileDialog.Filter = "All Files (*.*)|*.*"; // Allow all file types to be selected

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                set_private_key(openFileDialog.FileName);
            }
        }

        private void open_file_public_key_selection_dialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Public Key";
            openFileDialog.Filter = "All Files (*.*)|*.*"; // Allow all file types to be selected

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                set_public_key(openFileDialog.FileName);
            }
        }

        private void LeftHalf_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void LeftHalf_DragDrop(object sender, DragEventArgs e)
        {
            string[] selectedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (selectedFiles.Length == 1)
            {
                set_private_key(selectedFiles[0]);
            }
            else
            {
                ShowErrorMessageBox("Multiple files were dropped", "You can only drop one file at a time");
            }
        }

        private void set_private_key(string path)
        {
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length == 3)
                {
                    loaded_private_key = new string[3];
                    for (int i = 0; i < 3; i++)
                    {
                        loaded_private_key[i] = lines[i];
                    }
                    lhlabel.Text = "   Loaded Private Key\n\n   Name: " + loaded_private_key[0] + "\n   Company: " + loaded_private_key[1] + "\n   SHA-512 Fingerprint: " + HashStringWithSHA512(loaded_private_key[2], 84507);
                    lhlabel.TextAlign = ContentAlignment.MiddleLeft;
                }
                else
                {
                    ShowErrorMessageBox("Invalid Key", "Try Selecting Another One");
                }
            }
            else
            {
                ShowErrorMessageBox("File Not Found", "Please, Try Again");
            }
        }

        private void RightHalf_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void RightHalf_DragDrop(object sender, DragEventArgs e)
        {
            string[] selectedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (selectedFiles.Length == 1)
            {
                set_public_key(selectedFiles[0]);
            }
            else
            {
                ShowErrorMessageBox("Multiple files were dropped", "You can only drop one file at a time");
            }
        }

        private void set_public_key(string path)
        {
            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);
                if (lines.Length == 3)
                {
                    loaded_public_key = new string[3];
                    for (int i = 0; i < 3; i++)
                    {
                        loaded_public_key[i] = lines[i];
                    }
                    lhlabel1.Text = "   Loaded Public Key\n\n   Name: " + loaded_public_key[0] + "\n   Company: " + loaded_public_key[1] + "\n   SHA-512 Fingerprint: " + HashStringWithSHA512(loaded_public_key[2], 1);
                    lhlabel1.TextAlign = ContentAlignment.MiddleLeft;
                }
                else
                {
                    ShowErrorMessageBox("Invalid Key", "Try Selecting Another One");
                }
            }
            else
            {
                ShowErrorMessageBox("File Not Found", "Please, Try Again");
            }
        }

        public static string HashStringWithSHA512(string input, int iterations)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(input);

                for (int i = 0; i < iterations; i++)
                {
                    data = sha512.ComputeHash(data);
                }

                // Convert the final hash to a hexadecimal string
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < 16; i++)
                {
                    builder.Append(data[i].ToString("x2"));
                }

                return builder.ToString().ToUpper();
            }
        }

        public void ShowSuccessMessageBox(string line1, string line2)
        {
            Form customMessageBox = new Form
            {
                Text = "ECDSA File Signer Success",
                Size = new Size(640, 162),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(0, 128, 0)
            };

            // Create label for the first line
            Label label1 = new Label
            {
                Text = line1,
                ForeColor = Color.FromArgb(238, 238, 238),
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customMessageBox.Controls.Add(label1);

            // Create label for the second line
            Label label2 = new Label
            {
                Text = line2,
                ForeColor = Color.FromArgb(238, 238, 238),
                Font = new Font("Arial", 14),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customMessageBox.Controls.Add(label2);

            // Create OK button
            Button okButton = new Button
            {
                Text = "OK",
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(32, 32, 32), // "#202020"
                ForeColor = Color.FromArgb(238, 238, 238), // "#EEEEEE"
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            customMessageBox.Controls.Add(okButton);

            label1.Location = new Point((customMessageBox.ClientSize.Width) / 2, +10);
            label2.Location = new Point((customMessageBox.ClientSize.Width) / 2, label1.Bottom + 10);
            okButton.Location = new Point((customMessageBox.ClientSize.Width - okButton.Width) / 2, label2.Bottom + 12);

            CenterLabelText(label1, customMessageBox);
            CenterLabelText(label2, customMessageBox);

            // Handle Resize event to adjust positions dynamically
            customMessageBox.Resize += (sender, e) =>
            {
                CenterLabelText(label1, customMessageBox);
                CenterLabelText(label2, customMessageBox);
                okButton.Location = new Point((customMessageBox.ClientSize.Width - okButton.Width) / 2, label2.Bottom + 20);
            };

            // Show the message box
            customMessageBox.ShowDialog();
        }


        public void ShowErrorMessageBox(string line1, string line2)
        {
            Form customMessageBox = new Form
            {
                Text = "ECDSA File Signer Error",
                Size = new Size(640, 162),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(171, 49, 18)
            };

            // Create label for the first line
            Label label1 = new Label
            {
                Text = line1,
                ForeColor = Color.FromArgb(238, 238, 238),
                Font = new Font("Arial", 16, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customMessageBox.Controls.Add(label1);

            // Create label for the second line
            Label label2 = new Label
            {
                Text = line2,
                ForeColor = Color.FromArgb(238, 238, 238),
                Font = new Font("Arial", 14),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customMessageBox.Controls.Add(label2);

            // Create OK button
            Button okButton = new Button
            {
                Text = "OK",
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(32, 32, 32), // "#202020"
                ForeColor = Color.FromArgb(238, 238, 238), // "#EEEEEE"
                DialogResult = DialogResult.OK,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 }
            };
            customMessageBox.Controls.Add(okButton);

            label1.Location = new Point((customMessageBox.ClientSize.Width) / 2, +10);
            label2.Location = new Point((customMessageBox.ClientSize.Width) / 2, label1.Bottom + 10);
            okButton.Location = new Point((customMessageBox.ClientSize.Width - okButton.Width) / 2, label2.Bottom + 12);

            CenterLabelText(label1, customMessageBox);
            CenterLabelText(label2, customMessageBox);

            // Handle Resize event to adjust positions dynamically
            customMessageBox.Resize += (sender, e) =>
            {
                CenterLabelText(label1, customMessageBox);
                CenterLabelText(label2, customMessageBox);
                okButton.Location = new Point((customMessageBox.ClientSize.Width - okButton.Width) / 2, label2.Bottom + 20);
            };

            // Show the message box
            customMessageBox.ShowDialog();
        }

        public static void ShowMessageBox(string line1)
        {
            Form customMessageBox = new Form
            {
                Text = "ECDSA File Signer",
                Size = new Size(540, 132),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = ColorTranslator.FromHtml("#D31457")
            };

            Label label1 = new Label
            {
                Text = line1,
                ForeColor = Color.FromArgb(238, 238, 238),
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Regular),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customMessageBox.Controls.Add(label1);

            Button okButton = new Button
            {
                Text = "OK",
                Size = new Size(70, 30), // Fixed button width
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Regular),
                BackColor = Color.FromArgb(32, 32, 32),
                ForeColor = Color.FromArgb(238, 238, 238),
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat
            };
            customMessageBox.Controls.Add(okButton);

            label1.Location = new Point((customMessageBox.ClientSize.Width - label1.Width) / 2, 10);

            int buttonY = label1.Bottom + 14;
            int buttonX = (customMessageBox.ClientSize.Width - okButton.Width) / 2;
            okButton.Location = new Point(buttonX, buttonY);

            customMessageBox.Resize += (sender, e) =>
            {
                label1.Location = new Point((customMessageBox.ClientSize.Width - label1.Width) / 2, 10);
                buttonX = (customMessageBox.ClientSize.Width - okButton.Width) / 2;
                okButton.Location = new Point(buttonX, buttonY);
            };

            DialogResult result = customMessageBox.ShowDialog();
        }

        private void CenterLabelText(System.Windows.Forms.Label label,
        Form form)
        {
            label.Location =
            new Point((form.ClientSize.Width - label.Width) / 2,
            label.Location.Y);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void generateKeyPairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            generate_key_pair nkpair = new generate_key_pair();
            DialogResult result = nkpair.ShowDialog();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void signToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded_private_key == null)
            {
                ShowErrorMessageBox("Private key isn't loaded", "Load the private key and try again");
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        sign_file(filePath);
                    }
                }
            }
        }

        private void sign_file(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        byte[] privateKeyBytes = Convert.FromBase64String(loaded_private_key[2]);
                        using (ECDsa ecdsa = ECDsa.Create())
                        {
                            ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                            byte[] signature = ecdsa.SignData(ComputeFileHash(filePath, HashAlgorithmName.SHA512), HashAlgorithmName.SHA512);
                            File.WriteAllBytes(filePath + ".authentication_certificate", signature);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessageBox($"Can't Sign The \"{filePath}\" File", $"Error: {ex.Message}");
                    }
                }
                else
                {
                    ShowErrorMessageBox("Whoah!", $"File \"{filePath}\" Doesn't Exist");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox("Something Went Wrong", $"Error: {ex.Message}");
            }
        }
        static byte[] ComputeFileHash(string filePath, HashAlgorithmName hashAlgorithmName)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                using (var hashAlgorithm = HashAlgorithm.Create(hashAlgorithmName.Name))
                {
                    return hashAlgorithm.ComputeHash(stream);
                }
            }
        }

        private void verifyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (loaded_public_key == null)
            {
                ShowErrorMessageBox("Public key isn't loaded", "Load the public key and try again");
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Authentication Certificates (*.authentication_certificate)|*.authentication_certificate|All Files (*.*)|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        // Verify each file, removing the extension if necessary
                        string filePathWithoutExtension = filePath;
                        if (filePath.EndsWith(".authentication_certificate", StringComparison.OrdinalIgnoreCase))
                        {
                            filePathWithoutExtension = Path.ChangeExtension(filePath, null);
                        }
                        verify_file(filePathWithoutExtension);
                    }
                }
            }
        }

        private void verify_file(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Load public key from byte array
                        byte[] publicKeyBytes = Convert.FromBase64String(loaded_public_key[2]);

                        // Create an instance of ECDsa and import the public key
                        using (ECDsa ecdsa = ECDsa.Create())
                        {
                            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                            // Read the signature from the certificate file
                            string certificateFilePath = filePath + ".authentication_certificate";
                            byte[] signatureFromFile = File.ReadAllBytes(certificateFilePath);

                            // Compute the hash of the file
                            byte[] fileHash = ComputeFileHash(filePath, HashAlgorithmName.SHA512);

                            // Verify the signature against the computed hash
                            bool isSignatureValid = ecdsa.VerifyData(fileHash, signatureFromFile, HashAlgorithmName.SHA512);

                            if (isSignatureValid)
                            {
                                ShowSuccessMessageBox($"Authenticity Of \"{Path.GetFileName(filePath)}\" Verified Successfully", "The file was signed by " + loaded_public_key[0] + " from " + loaded_public_key[1]);
                            }
                            else
                            {
                                ShowErrorMessageBox($"Failed to Verify Integrity/Authenticity", $" of a file \"{filePath}\".");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessageBox($"Verification Failed for File \"{filePath}\"", $"Error: {ex.Message}");
                    }
                }
                else
                {
                    ShowErrorMessageBox("File Not Found", $"File \"{filePath}\" doesn't exist.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessageBox("Something Went Wrong", $"Error: {ex.Message}");
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            about_software();
        }

        private void about_software()
        {
            Form customForm = new Form
            {

                Text = "About Serpent + Twofish + AES File Encrypter",
                Size = new Size(860, 440),
                MinimumSize = new Size(640, 430),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = ColorTranslator.FromHtml("#0E71F3")
            };

            Label label = new Label
            {
                Text = "ECDSA File Signer is an open-source software distributed under the MIT License.\n" +
                       "You are free to modify and distribute copies of the ECDSA File Signer.\n" +
                       "You can use the ECDSA File Signer in commercial applications.\n\n" +
                       "The ECDSA File Signer app and its source code can be found on:\n\n" +
                       "SourceForge",
                ForeColor = ColorTranslator.FromHtml("#FFFFFF"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customForm.Controls.Add(label);

            TextBox textField = new TextBox
            {
                Size = new Size(440, 30),
                Text = "sourceforge.net/projects/ecdsa-file-signer/",
                Location = new Point((customForm.ClientSize.Width - 200) / 2, label.Bottom + 12),
                Font = new Font("Segoe UI", 14),
                ReadOnly = true,
                BackColor = ColorTranslator.FromHtml("#2C2C2C"),
                ForeColor = ColorTranslator.FromHtml("#E4E3DF")
            };
            customForm.Controls.Add(textField);

            Label label1 = new Label
            {
                Location = new Point((customForm.ClientSize.Width - 200) / 2, textField.Bottom + 15),
                Text = "Github",
                ForeColor = ColorTranslator.FromHtml("#FFFFFF"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customForm.Controls.Add(label1);

            TextBox textField1 = new TextBox
            {
                Size = new Size(440, 30),
                Text = "github.com/Northstrix/ECDSA-File-Signer",
                Location = new Point((customForm.ClientSize.Width - 200) / 2, label1.Bottom + 6),
                Font = new Font("Segoe UI", 14),
                ReadOnly = true,
                BackColor = ColorTranslator.FromHtml("#2C2C2C"),
                ForeColor = ColorTranslator.FromHtml("#E4E3DF")
            };
            customForm.Controls.Add(textField1);

            Label label2 = new Label
            {
                Location = new Point((customForm.ClientSize.Width - 200) / 2, textField1.Bottom + 20),
                Text = "Copyright " + "\u00a9" + " 2024 Maxim Bortnikov",
                ForeColor = ColorTranslator.FromHtml("#FFFFFF"),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter
            };
            customForm.Controls.Add(label2);

            Button continueButton = new Button
            {
                Text = "Got It",
                Size = new Size(120, 38),
                Location = new Point((customForm.ClientSize.Width - 200) / 2, label2.Bottom + 30),
                BackColor = ColorTranslator.FromHtml("#2C2C2C"),
                ForeColor = ColorTranslator.FromHtml("#FFFFFF"),
                DialogResult = DialogResult.Yes,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            customForm.Controls.Add(continueButton);

            CenterLabelText(label, customForm);
            CenterLabelText(label1, customForm);
            CenterLabelText(label2, customForm);
            label.Location = new Point((customForm.ClientSize.Width - label.Width) / 2, +12);
            label1.Location = new Point((customForm.ClientSize.Width - label1.Width) / 2, textField.Bottom + 15);
            textField.Location = new Point((customForm.ClientSize.Width - textField.Width) / 2, label.Bottom + 10);
            textField1.Location = new Point((customForm.ClientSize.Width - textField1.Width) / 2, label1.Bottom + 6);
            label2.Location = new Point((customForm.ClientSize.Width - label2.Width) / 2, textField1.Bottom + 20);
            continueButton.Location = new Point((customForm.ClientSize.Width - continueButton.Width) / 2, label2.Bottom + 20);

            // Handle Resize event to adjust positions dynamically
            customForm.Resize += (sender, e) =>
            {
                CenterLabelText(label, customForm);
                CenterLabelText(label1, customForm);
                CenterLabelText(label2, customForm);
                label.Location = new Point((customForm.ClientSize.Width - label.Width) / 2, +12);
                label1.Location = new Point((customForm.ClientSize.Width - label1.Width) / 2, textField.Bottom + 15);
                textField.Location = new Point((customForm.ClientSize.Width - textField.Width) / 2, label.Bottom + 10);
                textField1.Location = new Point((customForm.ClientSize.Width - textField.Width) / 2, label1.Bottom + 6);
                label2.Location = new Point((customForm.ClientSize.Width - label2.Width) / 2, textField1.Bottom + 20);
                continueButton.Location = new Point((customForm.ClientSize.Width - continueButton.Width) / 2, label2.Bottom + 20);
            };
            customForm.ShowDialog();
        }
    }
}