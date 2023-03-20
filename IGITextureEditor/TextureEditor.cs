using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TextureEditor
{
    public partial class TextureEditor : Form
    {
        private string[] texFiles;
        private int texIndex = 0;
        private string textureSelectedPath = null;

        public TextureEditor()
        {
            InitializeComponent();
            bool status = QUtils.InitEditorAppData();
        }

        private void selectTextureButton_Click(object sender, EventArgs e)
        {

        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "New" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void SetTextureImage(string textureFile)
        {
            string sourceFileNameWithoutExt = Path.GetFileNameWithoutExtension(textureFile);
            // Convert the texture image.
            //string convPath = ConvertTextureImage(textureFile);

            // Load output TGA file into picture box
            //string pngFilePath = Path.Combine(convPath, $"{sourceFileNameWithoutExt}.png");
            if (File.Exists(textureFile))
            {
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Output PNG file: {textureFile}");
                Bitmap bitmap = new Bitmap(textureFile);

                // Set image in PictureBox
                textureBox.Image = bitmap;
            }
            else
            {
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "No output PNG file found");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Clean the temp data first.
                QUtils.CleanUpTmpFiles();
                textureSelectedPath = null;
                texFiles = null;
                texIndex = 0;

                var folderBrowser = new OpenFileDialog();
                folderBrowser.ValidateNames = false;
                folderBrowser.CheckFileExists = false;
                folderBrowser.CheckPathExists = true;
                folderBrowser.FileName = "Folder Selection.";
                folderBrowser.Title = "Select Texture path";

                var folderBrowserDlg = folderBrowser.ShowDialog();
                if (folderBrowserDlg == DialogResult.OK)
                {
                    textureSelectedPath = Path.GetDirectoryName(folderBrowser.FileName) + Path.DirectorySeparatorChar;
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"selectedPath: {textureSelectedPath}");
                    string outputPath = null;

                    if (folderBrowser.FileName.Contains(".res"))
                    {
                        QUtils.ShowWarning("Resource file needs to be unpacked first.");
                        QUtils.UnpackResourceFile(folderBrowser.FileName);
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name,$"File {folderBrowser.FileName} unpacked success");
                        var basePathName = Path.GetFileName(Path.GetDirectoryName(folderBrowser.FileName));
                        textureSelectedPath += Path.DirectorySeparatorChar + basePathName;
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"After Unpacking new path: {textureSelectedPath}");

                        outputPath = QUtils.ConvertTextureImage(textureSelectedPath, true);
                        texFiles = Directory.GetFiles(outputPath, "*.png");
                    }
                    else
                    {
                        outputPath = QUtils.ConvertTextureImage(textureSelectedPath);
                        texFiles = Directory.GetFiles(outputPath, "*.png");
                    }

                    if (texFiles.Length > 0)
                    {
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name,"All textures were loaded successfully.");
                    }
                    else
                    {
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name,"Textures failed to load from path");
                    }

                    foreach (string texFile in texFiles)
                    {
                        texListBox.Items.Add(Path.GetFileName(texFile));
                    }
                }
            }
            catch (Exception ex)
            {
                QUtils.ShowLogException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void texListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (texListBox.SelectedIndex >= 0)
            {
                int selectedIndex = texListBox.SelectedIndex;
                string selectedFile = texFiles[selectedIndex];
                SetTextureImage(selectedFile);
            }
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Save" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Save As" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Cut" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Copy" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Paste" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Delete" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Rename" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "About" functionality
            MessageBox.Show("Not yet implemented.");
        }

        private void enableLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QUtils.EnableLogs();
        }

        // TODO: Implement "
        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Cut" functionality for the left panel context menu
            MessageBox.Show("Not yet implemented.");
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Copy" functionality for the left panel context menu
            MessageBox.Show("Not yet implemented.");
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Delete" functionality for the left panel context menu
            MessageBox.Show("Not yet implemented.");
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Implement "Export" functionality for the left panel context menu
            MessageBox.Show("Not yet implemented.");
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear the PictureBox
                //textureBox.Image = null;

                // Select image file using file dialog
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.jpg, *.png)|*.jpg;*;*.jpeg;.png;|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Selecting the source image file.
                    string inputFilePath = openFileDialog.FileName;
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Selected image file: {inputFilePath}");

                    // Get filename and file extension
                    string filename = Path.GetFileName(inputFilePath);


                    // Get output directory path
                    string outputDirectoryPath = textureSelectedPath;

                    // Get the first file in the output directory
                    string[] files = Directory.GetFiles(outputDirectoryPath);
                    string firstFile = files.Length > 0 ? files[0] : string.Empty;

                    // Get the extension of the first file
                    string extension = Path.GetExtension(firstFile);

                    // Determine convert type based on file extension.
                    string convertType = ".sprite";
                    switch (extension)
                    {
                        case ".spr":
                            convertType = "sprite";
                            break;
                        case ".tex":
                            convertType = "texture";
                            break;
                        case ".pic":
                            convertType = "pic";
                            break;
                    }

                    // Convert image to texture or sprite or pic
                    string textureBoxImagePath = texFiles[texIndex];
                    string tgaResolutionSize = "128 x 128";
                    QUtils.ConvertTextureImage(inputFilePath, outputDirectoryPath, convertType,ref textureBox, textureBoxImagePath, tgaResolutionSize);

                    // Load output image into picture box
                    string outputFilePath = Path.Combine(outputDirectoryPath, $"{filename}");
                    if (File.Exists(outputFilePath))
                    {
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Output PNG file: {outputFilePath}");
                        Bitmap bitmap = new Bitmap(outputFilePath);
                        textureBox.Image = bitmap;
                        //textureFileName.Text = filename;
                        //textureFileResolution.Text = $"{bitmap.Width}x{bitmap.Height}";
                        //textureFileSize.Text = $"{new FileInfo(outputFilePath).Length / 1024.0:F2} KB";
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name,$"File {filename} loaded as {convertType} successfully.");
                    }
                    else
                    {
                        QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "No output PNG file found");
                    }
                }
            }
            catch (Exception ex)
            {
                QUtils.ShowLogException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }
    }
}