using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace TextureEditor
{
    class QUtils
    {
        private static bool logEnabled;
        private static string editorLogFile;
        internal static string editorAppName;
        internal static string editorCurrPath;
        internal static string qTools;
        internal static string editorQTools;
        internal static string initErrReason;
        internal static string qCompile;
        internal static string qDecompile;

        #region Extensions Constants
        public static class FileExtensions
        {
            public const string Qvm = ".qvm";
            public const string Qsc = ".qsc";
            public const string Dat = ".dat";
            public const string Csv = ".csv";
            public const string Json = ".json";
            public const string Txt = ".txt";
            public const string Xml = ".xml";
            public const string Dll = ".dll";
            public const string Mission = ".igimsf";
            public const string Jpg = ".jpg";
            public const string Png = ".png";
            public const string Rar = ".rar";
            public const string Zip = ".zip";
            public const string Exe = ".exe";
            public const string Bat = ".bat";
            public const string Ini = ".ini";
            public const string Spr = ".spr";
            public const string Text = ".txt";
            public const string Pic = ".pic";
        }
        #endregion


        #region Error Constants
        public static class ApplicationError
        {
            public static readonly string CAPTION_CONFIG_ERR = "Config - Error";
            public static readonly string CAPTION_FATAL_SYS_ERR = "Sytem-Fatal - Error";
            public static readonly string CAPTION_APP_ERR = "Application - Error";
            public static readonly string CAPTION_COMPILER_ERR = "Compiler - Error";
            public static readonly string EDITOR_LEVEL_ERR = "EDITOR ERROR";
            public static readonly string EXTERNAL_COMPILER_ERR = "External Compiler not found in QEditor directory." + "\n" + "Try switching to External compiler from settings.";
        }
        #endregion

        internal static bool InitEditorAppData()
        {
            bool initStatus = true;
            editorAppName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", String.Empty);
            editorLogFile = editorAppName + ".log";
            editorCurrPath = Directory.GetCurrentDirectory();
            editorQTools = editorCurrPath + @"\QTools";
            qTools = editorCurrPath + @"\QTools\Tools";
            qCompile = editorCurrPath + @"\QTools\Tools";
            qDecompile = editorCurrPath + @"\QTools\Tools";

            if (!Directory.Exists(editorQTools)) { initErrReason = "QTools"; initStatus = false; }
            else if (!Directory.Exists(qTools)) { initErrReason = @"QTools\Tools"; initStatus = false; }
            else if (!Directory.Exists(qCompile)) { initErrReason = @"QTools\Compile"; initStatus = false; }
            else if (!Directory.Exists(qDecompile)) { initErrReason = @"QTools\Decompile"; initStatus = false; }


            initErrReason = "'" + initErrReason + "' Directory is missing";
            //Show error if 'QEditor' path has invalid structure..
            if (!initStatus) ShowSystemFatalError("Editor Appdata directory is invalid Error: (0x0000000F)\nReason: " + initErrReason + "\nPlease re-install new copy from Setup file.");

            return initStatus;
        }


        internal static string ConvertTextureImage(string textureFilePath, bool resourceFile = false)
        {
            try
            {
                // Clear the PictureBox
                //textureBox.Image = null;
                string sourceDir = null;
                string destDir = null;

                // Move all files from source path to DConv input directory
                if (!resourceFile)
                {
                    sourceDir = Path.GetDirectoryName(textureFilePath);
                }
                else
                {
                    sourceDir = textureFilePath;
                }
                destDir = Path.Combine(QUtils.qTools, @"DConv\input");

                // Copy all files to DConv directory.
                Directory.CreateDirectory(destDir);
                Directory.GetFiles(sourceDir).ToList().ForEach(f => File.Copy(f, Path.Combine(destDir, Path.GetFileName(f)), true));

                // Run DConv to convert files to TGA
                string dconvPath = Path.Combine(QUtils.qTools, @"DConv\dconv.exe");
                string dconvArgs = "tex convert input output";
                string dconvDir = Path.Combine(QUtils.qTools, @"DConv");
                QUtils.ShellExec($"cd {dconvDir} && {dconvPath} {dconvArgs}");
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "DConv conversion completed");

                // Move TGA files from DConv output directory to TGAConv directory
                string tgaConvPath = Path.Combine(QUtils.qTools, @"TGAConv");
                destDir = Path.Combine(tgaConvPath, "");
                foreach (string file in Directory.GetFiles(Path.Combine(QUtils.qTools, @"DConv\output"), "*.tga"))
                {
                    string fileName = Path.GetFileName(file);
                    string destination = Path.Combine(destDir, fileName);
                    QUtils.FileMove(file, destination);
                }

                // Run TGAConv to convert TGA files to PNG
                string tgaConvExePath = Path.Combine(tgaConvPath, "tgaconv.exe");
                string tgaConvArgs = "*.tga -ToPng";
                string tgaConvDir = Path.Combine(QUtils.qTools, @"TGAConv");
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Running TGAConv: {tgaConvExePath} {tgaConvArgs} in directory {tgaConvDir}");
                QUtils.ShellExec($"cd {tgaConvDir} && {tgaConvExePath} {tgaConvArgs}");
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "TGAConv conversion completed");

                // Return the path to the TGAConv directory
                return tgaConvPath;
            }
            catch (Exception ex)
            {
                QUtils.ShowLogException(MethodBase.GetCurrentMethod().Name, ex);
                return null;
            }
        }

        static bool ConvertToTga(string inputFilePath, string resolution)
        {
            bool status = false;
            int.TryParse(resolution.Split('x')[0].Trim(), out int width);
            int.TryParse(resolution.Split('x')[1].Trim(), out int height);

            // Run TGAConv to convert TGA files to PNG
            string inputFileWithoutExt = Path.Combine(Path.GetDirectoryName(inputFilePath), Path.GetFileNameWithoutExtension(inputFilePath));
            string tgaConvPath = Path.Combine(QUtils.qTools, @"TGAConv");
            string tgaConvExePath = Path.Combine(tgaConvPath, "tgaconv.exe");

            string tgaConvArgs = $"{inputFilePath} -ToTga --resize {width} {height}";
            string tgaConvDir = Path.Combine(QUtils.qTools, @"TGAConv");
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Running TGAConv: {tgaConvExePath} {tgaConvArgs} in directory {tgaConvDir}");
            QUtils.ShellExec($"cd {tgaConvDir} && {tgaConvExePath} {tgaConvArgs}");

            string tgaFilePath = Path.Combine(tgaConvPath, inputFileWithoutExt + ".tga");
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "Input TGA Path: " + tgaFilePath);
            string destTgaFilePath = Path.Combine(tgaConvPath, Path.GetFileName(tgaFilePath));

            if (File.Exists(tgaFilePath) && !File.Exists(destTgaFilePath))
            {
                QUtils.FileMove(tgaFilePath, destTgaFilePath);
            }

            if (!File.Exists(destTgaFilePath))
            {
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Error: TGAConv failed to convert {inputFilePath} to TGA format");
                return status;
            }
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"TGAConv conversion of {inputFilePath} to TGA format completed successfully");
            status = true;
            return status;
        }

        internal static void ConvertTextureImage(string inputFilePath, string outputDirectoryPath, string convertType,ref PictureBox textureBox,string textureBoxImagePath,string tgaResolutionSize)
        {
            try
            {
                // Get the full path of the image file
                string imagePath = textureBox.ImageLocation;

                // Delete the file
                if (!string.IsNullOrEmpty(imagePath))
                {
                    File.Delete(imagePath);
                    textureBox.Image = null;
                }

                string sourceFileNameWithoutExt = Path.GetFileNameWithoutExtension(inputFilePath);
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Selected output directory: {outputDirectoryPath}");

                // Run TGAConv to convert JPG/PNG to TGA.
                string tgaConvPath = Path.Combine(QUtils.qTools, @"TGAConv");
                bool convertStatus = ConvertToTga(inputFilePath, tgaResolutionSize);

                if (!convertStatus)
                {
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name,"Error while converting Textures.");
                    return;
                }

                string makeTexCmd = null;
                string makeScriptPath = null;
                string inputConvertPath = null;
                string outputConvertPath = null;

                if (convertType == "texture")
                {
                    // Generate MakeTex script
                    makeScriptPath = Path.Combine(QUtils.qTools, "maketex.qsc");
                    string tgaFilePath = Path.Combine(tgaConvPath, $"{sourceFileNameWithoutExt}.tga");
                    string texFilePath = Path.Combine(outputDirectoryPath, $"{sourceFileNameWithoutExt}.tex");
                    makeTexCmd = $"MakeTexture(\"{$"{sourceFileNameWithoutExt}.tga"}\", \"{$"{sourceFileNameWithoutExt}.tex"}\");";
                    File.WriteAllText(makeScriptPath, makeTexCmd + "\r\n");
                    inputConvertPath = Path.GetDirectoryName(tgaFilePath);
                    outputConvertPath = Path.GetDirectoryName(texFilePath);
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Added MakeTex command: {makeTexCmd}");
                }

                else if (convertType == "sprite")
                {
                    // Generate MakeTex script
                    makeScriptPath = Path.Combine(QUtils.qTools, "makespr.qsc");
                    string tgaFilePath = Path.Combine(tgaConvPath, $"{sourceFileNameWithoutExt}.tga");
                    string sprFilePath = Path.Combine(outputDirectoryPath, $"{sourceFileNameWithoutExt}.spr");
                    makeTexCmd = $"MakeSprite(\"{$"{sourceFileNameWithoutExt}.tga"}\", \"{$"{sourceFileNameWithoutExt}.spr"}\");";
                    File.WriteAllText(makeScriptPath, makeTexCmd + "\r\n");
                    inputConvertPath = Path.GetDirectoryName(tgaFilePath);
                    outputConvertPath = Path.GetDirectoryName(sprFilePath);
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Added MakeTex command: {makeTexCmd}");
                }

                else if (convertType == "pic")
                {
                    // Generate MakeTex script
                    makeScriptPath = Path.Combine(QUtils.qTools, "makepic.qsc");
                    string tgaFilePath = Path.Combine(tgaConvPath, $"{sourceFileNameWithoutExt}.tga");
                    string picFilePath = Path.Combine(outputDirectoryPath, $"{sourceFileNameWithoutExt}.pic");
                    makeTexCmd = $"MakePicture(\"{$"{sourceFileNameWithoutExt}.tga"}\", \"{$"{sourceFileNameWithoutExt}.pic"}\");";
                    File.WriteAllText(makeScriptPath, makeTexCmd + "\r\n");
                    inputConvertPath = Path.GetDirectoryName(tgaFilePath);
                    outputConvertPath = Path.GetDirectoryName(picFilePath);
                    QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Added MakeTex command: {makeTexCmd}");
                }

                // Run GConv to generate game resource file
                string gconvPath = Path.Combine(QUtils.qTools, @"GConv\gconv.exe");
                string gconvArgs = $"\"{makeScriptPath}\" -InputPath={inputConvertPath} -OutputPath={outputConvertPath}";
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Running GConv: {gconvPath} {gconvArgs}");
                QUtils.ShellExec($"{gconvPath} {gconvArgs}");
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "GConv conversion completed");

                // Clean up directories
                File.Delete(makeScriptPath);
                QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "Cleaning up directories");

                string textureBoxImage = Path.GetFileName(textureBoxImagePath);
                outputConvertPath = outputDirectoryPath + Path.ChangeExtension(textureBoxImage, "tex");

                string destTexFile = sourceFileNameWithoutExt + ".tex";

                // Get the directory path of the output file
                string outputDirectory = Path.GetDirectoryName(outputConvertPath);

                // Combine the output directory with the new file name to create the new file path
                string newFilePath = Path.Combine(outputDirectory, destTexFile);

                // Check if the new file already exists, and delete it if it does
                if (File.Exists(outputConvertPath))
                {
                    File.Delete(outputConvertPath);
                }

                // Rename the output file to the new file name
                File.Move(newFilePath, outputConvertPath);


                QUtils.AddLog(MethodBase.GetCurrentMethod().Name,$"Resource {sourceFileNameWithoutExt} saved as texture successfully.");
            }
            catch (Exception ex)
            {
                QUtils.ShowLogException(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        internal static void UnpackResourceFile(string resourceFile)
        {
            // Create input and output directories
            string inputDirectoryPath = Path.GetDirectoryName(resourceFile);
            string outputDirectoryPath = inputDirectoryPath;
            string sourceFileNameWithoutExt = Path.GetFileNameWithoutExtension(resourceFile);

            // Generate decompile script
            string decompileScriptPath = Path.Combine(QUtils.qTools, "decompile.qsc");
            string decompileCmd = $"ExtractResource(\"{Path.GetFileName(resourceFile)}\");";
            File.WriteAllText(decompileScriptPath, decompileCmd);
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Created decompile script: {decompileCmd}");

            // Run GConv to decompile the resource file
            string gconvPath = Path.Combine(QUtils.qTools, @"GConv\gconv.exe");
            string gconvArgs = $"\"{decompileScriptPath}\" -InputPath=\"{inputDirectoryPath}\" -OutputPath=\"{outputDirectoryPath}\"";
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Running GConv: {gconvPath} {gconvArgs}");
            QUtils.ShellExec($"{gconvPath} {gconvArgs}");
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "GConv decompilation completed");

            // Delete decompile script
            File.Delete(decompileScriptPath);
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, $"Deleted decompile script: {decompileScriptPath}");
        }

        internal static void CleanUpTmpFiles()
        {
            // Cleaning up directories
            QUtils.AddLog(MethodBase.GetCurrentMethod().Name, "Cleaning up temp directories");
            string[] dconvFiles = Directory.GetFiles(Path.Combine(QUtils.qTools, @"DConv\input")).Concat(Directory.GetFiles(Path.Combine(QUtils.qTools, @"DConv\output"))).ToArray();
            string[] tgaConvFiles = Directory.GetFiles(Path.Combine(QUtils.qTools, @"TGAConv")).ToArray();
            foreach (string file in dconvFiles.Concat(tgaConvFiles))
            {
                try
                {
                    if (file.Contains(".exe")) continue; // Skip the TGAConv file.
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    QUtils.LogException(MethodBase.GetCurrentMethod().Name, ex);
                }
            }
        }

        //Execute shell command and get std-output.
        internal static string ShellExec(string cmdArgs, bool runAsAdmin = false, bool waitForExit = true, string shell = "cmd.exe")
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = true;
            startInfo.FileName = shell;
            startInfo.Arguments = "/c " + cmdArgs;
            startInfo.RedirectStandardOutput = !runAsAdmin;
            startInfo.RedirectStandardError = !runAsAdmin;
            startInfo.UseShellExecute = runAsAdmin;
            process.StartInfo = startInfo;
            if (runAsAdmin) process.StartInfo.Verb = "runas";
            process.Start();
            if (!waitForExit) return null;
            string output = (runAsAdmin) ? String.Empty : process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        //File Operation Utilities C# Version.
        internal static void FileMove(string sourcePath, string destPath)
        {
            try
            {
                if (File.Exists(sourcePath)) File.Move(sourcePath, destPath);
            }
            catch (Exception ex) { ShowLogException(MethodBase.GetCurrentMethod().Name, ex); }
        }

        internal static void FileCopy(string sourcePath, string destPath, bool overwirte = true)
        {
            try
            {
                if (File.Exists(sourcePath)) File.Copy(sourcePath, destPath, overwirte);
            }
            catch (Exception ex) { ShowLogException(MethodBase.GetCurrentMethod().Name, ex); }
        }

        internal static void FileDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception ex) { ShowLogException(MethodBase.GetCurrentMethod().Name, ex); }
        }

        //Directory Operation Utilities C#.
        internal static void DirectoryMove(string sourcePath, string destPath)
        {
            try
            {
                if (Directory.Exists(sourcePath)) Directory.Move(sourcePath, destPath);
            }
            catch (Exception ex) { ShowLogException(MethodBase.GetCurrentMethod().Name, ex); }
        }

        internal static void DirectoryMove(string sourcePath, string destPath, int __ignore)
        {
            var mvCmd = "mv " + sourcePath + " " + destPath;
            var moveCmd = "move " + sourcePath + " " + destPath + " /y";

            try
            {
                //#1 solution to move with same root directory.
                Directory.Move(sourcePath, destPath);
            }
            catch (IOException ex)
            {
                if (ex.Message.Contains("already exist"))
                {
                    DirectoryDelete(sourcePath);
                }
                else
                {
                    //#2 solution to move with POSIX 'mv' command.
                    ShellExec(mvCmd, true, true, "powershell.exe");
                    if (Directory.Exists(sourcePath))
                        //#3 solution to move with 'move' command.
                        ShellExec(moveCmd, true);
                }
            }
        }

        internal static void DirectoryDelete(string dirPath)
        {
            try
            {
                if (Directory.Exists(dirPath))
                {
                    DirectoryInfo di = new DirectoryInfo(dirPath);
                    foreach (FileInfo file in di.GetFiles())
                        file.Delete();
                    foreach (DirectoryInfo dir in di.GetDirectories())
                        dir.Delete(true);
                    Directory.Delete(dirPath);
                }
            }
            catch (Exception ex) { ShowLogException(MethodBase.GetCurrentMethod().Name, ex); }
        }


        //UI-Dialogs and MessageBox.
        internal static void ShowWarning(string warnMsg, string caption = "WARNING")
        {
            MessageBox.Show(warnMsg, caption, MessageBoxButtons.OK);
        }

        internal static void ShowError(string errMsg, string caption = "ERROR")
        {
            MessageBox.Show(errMsg, caption, MessageBoxButtons.OK);
        }

        internal static void LogException(string methodName, Exception ex)
        {
            methodName = methodName.Replace("Btn_Click", String.Empty).Replace("_SelectedIndexChanged", String.Empty).Replace("_SelectedValueChanged", String.Empty);
            AddLog(methodName, "Exception MESSAGE: " + ex.Message + "\nREASON: " + ex.StackTrace);
        }

        internal static void ShowException(string methodName, Exception ex)
        {
            ShowError("MESSAGE: " + ex.Message + "\nREASON: " + ex.StackTrace, methodName + " Exception");
        }

        internal static void ShowLogException(string methodName, Exception ex)
        {
            methodName = methodName.Replace("Btn_Click", String.Empty).Replace("_SelectedIndexChanged", String.Empty).Replace("_SelectedValueChanged", String.Empty);
            //Show and Log exception for method name.
            ShowException(methodName, ex);
            LogException(methodName, ex);
        }

        internal static void ShowLogError(string methodName, string errMsg, string caption = "ERROR")
        {
            methodName = methodName.Replace("Btn_Click", String.Empty).Replace("_SelectedIndexChanged", String.Empty).Replace("_SelectedValueChanged", String.Empty);
            //Show and Log error for method name.
            ShowError(methodName + "(): " + errMsg, caption);
            AddLog(methodName, errMsg);
        }

        internal static void ShowLogStatus(string methodName, string logMsg)
        {
            AddLog(methodName, logMsg);
        }

        internal static void ShowLogInfo(string methodName, string logMsg)
        {
            ShowInfo(logMsg);
            AddLog(methodName, logMsg);
        }

        internal static void ShowInfo(string infoMsg, string caption = "INFO")
        {
            MessageBox.Show(infoMsg, caption, MessageBoxButtons.OK);
        }

        internal static DialogResult ShowDialog(string infoMsg, string caption = "INFO")
        {
            return MessageBox.Show(caption, infoMsg, MessageBoxButtons.YesNo);
        }

        internal static void ShowConfigError(string keyword)
        {
            ShowError("Config has invalid property for '" + keyword + "'", ApplicationError.CAPTION_CONFIG_ERR);
        }

        internal static void ShowSystemFatalError(string errMsg)
        {
            ShowError(errMsg, ApplicationError.CAPTION_FATAL_SYS_ERR);
            Environment.Exit(1);
        }

        internal static void EnableLogs()
        {
            if (!logEnabled)
                logEnabled = true;
        }

        internal static void DisableLogs()
        {
            if (logEnabled)
                logEnabled = false;
        }

        internal static void AddLog(string methodName, string logMsg)
        {
            if (logEnabled)
            {
                methodName = methodName.Replace("Btn_Click", String.Empty).Replace("_SelectedIndexChanged", String.Empty).Replace("_SelectedValueChanged", String.Empty);
                File.AppendAllText(editorLogFile, "[" + DateTime.Now.ToString("yyyy-MM-dd - HH:mm:ss") + "] " + methodName + "(): " + logMsg + "\n");
            }
        }
    }
}
