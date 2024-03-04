using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace ECDSA_File_Signer
{
    public class generate_key_pair : Form
    {
        private const int formWidth = 500;
        private const int formHeight = 250;
        private const int labelHeight = 30;
        private const int textBoxHeight = 30;
        private const int buttonHeight = 40;

        public generate_key_pair()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Size = new Size(formWidth, formHeight);
            this.Text = "Generate A Key Pair For...";
            this.BackColor = ColorTranslator.FromHtml("#0E71F3");

            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(20)
            };

            this.Controls.Add(mainTable);

            mainTable.Controls.Add(CreateLabel("Name"), 0, 0);
            mainTable.Controls.Add(CreateTextBox(), 1, 0);

            mainTable.Controls.Add(CreateLabel("Company"), 0, 1);
            mainTable.Controls.Add(CreateMultilineTextBox(), 1, 1);

            Button addButton = CreateButton("Create", Color.FromArgb(0, 189, 0), Color.White);
            Button cancelButton = CreateButton("Cancel", Color.FromArgb(236, 0, 0), Color.White);

            addButton.Click += (sender, e) => add_record_to_specialization_table(mainTable);
            cancelButton.Click += (sender, e) => this.Close();

            mainTable.Controls.Add(addButton, 0, 2);
            mainTable.Controls.Add(cancelButton, 1, 2);

            this.Resize += (sender, e) =>
            {
                addButton.Width = (mainTable.Width - 6) / 2;
                cancelButton.Width = (mainTable.Width - 6) / 2;
            };
        }

        private Label CreateLabel(string labelText)
        {
            Label label = new Label()
            {
                Text = labelText,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = labelHeight,
            };

            return label;
        }

        private TextBox CreateTextBox()
        {
            TextBox textBox = new TextBox()
            {
                BackColor = Color.FromArgb(220, 220, 220), // Light gray background color
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                Height = textBoxHeight,
                Font = new Font("Arial", 12, FontStyle.Regular)
            };

            return textBox;
        }

        private TextBox CreateMultilineTextBox()
        {
            TextBox textBox = new TextBox()
            {
                BackColor = Color.FromArgb(220, 220, 220), // Light gray background color
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                Multiline = true,
                Height = textBoxHeight * 3,
                Font = new Font("Arial", 12, FontStyle.Regular)
            };

            return textBox;
        }

        private Button CreateButton(string buttonText, Color backColor, Color foreColor)
        {
            Button button = new Button()
            {
                Text = buttonText,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Height = buttonHeight,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
            };

            return button;
        }

        private void add_record_to_specialization_table(TableLayoutPanel mainTable)
        {
            string name = ((TextBox)mainTable.GetControlFromPosition(1, 0)).Text;
            string company = ((TextBox)mainTable.GetControlFromPosition(1, 1)).Text;
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Save Key Pair To...";
                folderBrowserDialog.ShowNewFolderButton = true;

                // Show the folder browser dialog
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    string selectedPath = folderBrowserDialog.SelectedPath;

                    // Create a new folder
                    string folderPath = Path.Combine(selectedPath, name + "'s key pair");
                    Directory.CreateDirectory(folderPath);

                    using (ECDsa ecdsa = ECDsa.Create())
                    {
                        // Generate a new key pair
                        ecdsa.GenerateKey(ECCurve.NamedCurves.nistP256);

                        // Get the private key in PKCS#8 format
                        byte[] privateKeyBytes = ecdsa.ExportPkcs8PrivateKey();

                        // Get the public key in X.509 format
                        byte[] publicKeyBytes = ecdsa.ExportSubjectPublicKeyInfo();


                        string filePath1 = Path.Combine(folderPath, name + "'s Private Key");
                        File.WriteAllText(filePath1, name + "\n" + company + "\n" + Convert.ToBase64String(privateKeyBytes));

                        string filePath2 = Path.Combine(folderPath, name + "'s Public Key");
                        File.WriteAllText(filePath2, name + "\n" + company + "\n" + Convert.ToBase64String(publicKeyBytes));
                    }

                    Form1.ShowMessageBox("Key Pair Generated Successfully");
                }
                else
                {
                    Form1.ShowMessageBox("Operation canceled by the user");
                }
            }
            this.Close();
        }
    }
}
