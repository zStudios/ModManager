﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Data.SQLite;
using System.Xml.Linq;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ModBuilder.Forms
{
    public partial class convertProject : Form
    {
        public convertProject()
        {
            InitializeComponent();
        }

        private void browseOutputDirectory_Click(object sender, EventArgs e)
        {
            // Get us a new FolderBrowserDialog
            CommonOpenFileDialog fb = new CommonOpenFileDialog();
            fb.IsFolderPicker = true;
            fb.Title = "Please select the directory where your project should be created.";
            fb.EnsurePathExists = true;
            CommonFileDialogResult rs = fb.ShowDialog();

            if (rs == CommonFileDialogResult.Cancel)
                return;

            // Get the path.
            string dir = fb.FileName;

            // Did we get a valid response?
            if (!String.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                // Being the nice program I am, I shall check if a project already exists in this directory.
                if (File.Exists(dir + "/data.sqlite") && File.Exists(dir + "/Package/package-info.xml"))
                {
                    DialogResult qresult = MessageBox.Show("Mod Manager has detected that a project already exists in the selected directory. Are you sure you want to continue and possibly overwrite the existing project?", "Options", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    // No? Quit.
                    if (qresult == DialogResult.No)
                        return;
                }

                // Update the textbox.
                outputDirectory.Text = dir;
            }
        }

        private void browseInputPackageDirectory_Click(object sender, EventArgs e)
        {
            // Get us a new FolderBrowserDialog
            CommonOpenFileDialog fb = new CommonOpenFileDialog();
            fb.IsFolderPicker = true;
            fb.Title = "Please select the directory where your package is located.";
            fb.EnsurePathExists = true;
            CommonFileDialogResult rs = fb.ShowDialog();

            if (rs == CommonFileDialogResult.Cancel)
                return;

            // Get the path.
            string dir = fb.FileName;

            // Did we get a valid response?
            if (!String.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                // Being the nice program I am, I shall check if a project already exists in this directory.
                if (!File.Exists(dir + "/package-info.xml"))
                {
                    DialogResult qresult = MessageBox.Show("Mod Manager has detected that there is no package-info.xml in the package, so you will have to manually select any files in the package. Do you still want to continue?", "Converting", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    // No? Quit.
                    if (qresult == DialogResult.No)
                        return;
                }

                // Clean out the old fields.
                packageInfoXMLPath.Text = "";
                installXmlPath.Text = "";
                readmeTXTPath.Text = "";
                installPHPPath.Text = "";
                uninstallPHPPath.Text = "";
                installDatabasePHPPath.Text = "";
                uninstallDatabasePHPPath.Text = "";

                // Update the textbox.
                packageInputPath.Text = dir;

                #region Read the package-info.xml, if it exists.
                if (File.Exists(dir + "/package-info.xml"))
                {
                    packageInfoXMLPath.Text = dir + "\\package-info.xml";

                    XmlDocument doc = new XmlDocument();
                    doc.Load(dir + "/package-info.xml");

                    foreach (XmlNode l_packageNode in doc.LastChild.ChildNodes)
                    {
                        if (l_packageNode.Name == "install")
                        {
                            foreach (XmlNode l_operationNode in l_packageNode.ChildNodes)
                            {
                                switch (l_operationNode.Name)
                                {
                                    case "modification":
                                        installXmlPath.Text = dir + "/" + l_operationNode.InnerText;
                                        break;
                                    case "code":
                                        installPHPPath.Text = dir + "/" + l_operationNode.InnerText;
                                        break;
                                    case "database":
                                        installDatabasePHPPath.Text = dir + "/" + l_operationNode.InnerText;
                                        break;
                                    case "readme":
                                        if (File.Exists(dir + "/" + l_operationNode.InnerText))
                                            readmeTXTPath.Text = dir + "/" + l_operationNode.InnerText;
                                        else
                                            readmeTXTPath.Text = "Inline";
                                        break;
                                }
                            }
                        }
                        if (l_packageNode.Name == "uninstall")
                        {
                            foreach (XmlNode l_operationNode in l_packageNode.ChildNodes)
                            {
                                switch (l_operationNode.Name)
                                {
                                    case "code":
                                        uninstallPHPPath.Text = dir + "/" + l_operationNode.InnerText;
                                        break;
                                    case "database":
                                        uninstallDatabasePHPPath.Text = dir + "/" + l_operationNode.InnerText;
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
        }

        #region Browse for files
        private string browseFile(string description, string filter)
        {
            // New file browser.
            OpenFileDialog of = new OpenFileDialog();

            // Set the description and alike.
            of.CheckFileExists = true;
            of.InitialDirectory = (!String.IsNullOrEmpty(packageInputPath.Text) ? packageInputPath.Text : "");
            of.Title = (!String.IsNullOrEmpty(description) ? description : "Select a file...");
            of.Filter = (!String.IsNullOrEmpty(filter) ? filter : "All files|*.*");

            // Show the dialog.
            of.ShowDialog();

            // Did we get a valid input?
            if (!string.IsNullOrEmpty(of.FileName) || !File.Exists(of.FileName))
                return "false";

            // We did. Now return it.
            return of.FileName;
        }

        private void browsePackageInfoXML_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the package-info.xml file...", "XML files|*.xml");

            if (result == "false")
                return;

            packageInfoXMLPath.Text = result;
        }

        private void browseReadmeTXT_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the file containing your readme...", "TXT files|*.txt");

            if (result == "false")
                return;

            readmeTXTPath.Text = result;
        }

        private void browseInstallXML_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the XML file containing your installation instructions...", "XML files|*.xml");

            if (result == "false")
                return;

            installXmlPath.Text = result;
        }

        private void browseInstallPHP_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the PHP file containing code to be run on installation...", "PHP files|*.php");

            if (result == "false")
                return;

            installPHPPath.Text = result;
        }

        private void browseUninstallPHP_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the PHP file containing code to be run on deinstallation...", "PHP files|*.php");

            if (result == "false")
                return;

            uninstallPHPPath.Text = result;
        }

        private void browseInstallDatabasePHP_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the PHP file containing database code to be run on installation...", "PHP files|*.php");

            if (result == "false")
                return;

            installDatabasePHPPath.Text = result;
        }

        private void browseUninstallDatabasePHP_Click(object sender, EventArgs e)
        {
            string result = browseFile("Select the PHP file containing code to be run on deinstallation...", "PHP files|*.php");

            if (result == "false")
                return;

            uninstallDatabasePHPPath.Text = result;
        }
        #endregion

        private void okButton_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(packageInputPath.Text))
            {
                MessageBox.Show("Your input directory does not exist. Please select a different one.", "Convert Project", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(outputDirectory.Text))
                Directory.CreateDirectory(outputDirectory.Text);

            // Make sure the user knows we're busy...
            System.Threading.Thread.Sleep(100);
            cvWorking.Visible = true;

            modEditor me = new modEditor();
            Dictionary<string, string> details = Classes.ModParser.parsePackageInfo(packageInfoXMLPath.Text);
            me.generateSQL(outputDirectory.Text, true, details);

            // Update a setting.
            string updatesql = "UPDATE settings SET value = \"false\" WHERE key = \"autoGenerateModID\"";
            SQLiteCommand ucomm = new SQLiteCommand(updatesql, me.conn);
            ucomm.ExecuteNonQuery();

            // Create a readme.txt if the text is inline.
            bool readmeTextInline = (readmeTXTPath.Text == "Inline");

            // Parse the XML.
            #region Read the install.xml, if it exists.
            if (File.Exists(installXmlPath.Text))
            {
                XmlTextReader xmldoc = new XmlTextReader(installXmlPath.Text);
                xmldoc.DtdProcessing = DtdProcessing.Ignore;
                string currfile = "";
                string sql = "INSERT INTO instructions(id, before, after, type, file, optional) VALUES(null, @beforeText, @afterText, @type, @fileEdited, @optional)";

                while (xmldoc.Read())
                {
                    if (xmldoc.NodeType.Equals(XmlNodeType.Element))
                    {
                        switch (xmldoc.LocalName)
                        {
                            case "file":
                                currfile = xmldoc.GetAttribute("name");
                                break;

                            case "operation":
                                SQLiteCommand command = new SQLiteCommand(sql, me.conn);
                                command.Parameters.AddWithValue("@fileEdited", currfile);
                                command.Parameters.AddWithValue("@optional", (xmldoc.GetAttribute("error") == "skip"));

                                xmldoc.ReadToDescendant("search");
                                command.Parameters.AddWithValue("@type", xmldoc.GetAttribute("position"));
                                command.Parameters.AddWithValue("@beforeText", xmldoc.ReadElementContentAsString().Replace("\r", "\n").Replace("\n", "\r\n"));

                                xmldoc.ReadToNextSibling("add");
                                command.Parameters.AddWithValue("@afterText", xmldoc.ReadElementContentAsString().Replace("\r", "\n").Replace("\n", "\r\n"));

                                command.ExecuteNonQuery();
                                break;
                        }
                    }
                }
            }
            #endregion

            // Copy over any remaining files.
            Dictionary<string, string> pfiles = new Dictionary<string, string>();
            Directory.CreateDirectory(outputDirectory.Text + "/Package");
            Directory.CreateDirectory(outputDirectory.Text + "/Source");

            pfiles.Add("Package/package-info.xml", packageInfoXMLPath.Text);
            if (!string.IsNullOrEmpty(readmeTXTPath.Text))
                pfiles.Add("Package/readme.txt", readmeTXTPath.Text);
            if (!string.IsNullOrEmpty(installPHPPath.Text))
                pfiles.Add("Package/install.php", installPHPPath.Text);
            if (!string.IsNullOrEmpty(uninstallPHPPath.Text))
                pfiles.Add("Package/uninstall.php", uninstallPHPPath.Text);
            if (!string.IsNullOrEmpty(installDatabasePHPPath.Text))
                pfiles.Add("Package/installDatabase.php", installDatabasePHPPath.Text);
            if (!string.IsNullOrEmpty(uninstallDatabasePHPPath.Text))
                pfiles.Add("Package/uninstallDatabase.php", uninstallDatabasePHPPath.Text);

            foreach (var pair in pfiles)
            {
                if (!File.Exists(pair.Value))
                    continue;

                if (File.Exists(outputDirectory.Text + "/" + pair.Key))
                    File.Delete(outputDirectory.Text + "/" + pair.Key);

                File.Copy(pair.Value, outputDirectory.Text + "/" + pair.Key);
            }

            #region Read the package.xml, if it exists, for any further information.
            
            if (File.Exists(packageInfoXMLPath.Text))
            {
                XmlDocument l_document = new XmlDocument();
                l_document.Load(packageInfoXMLPath.Text);

                SQLiteCommand sql;
                string sqlquery;
                foreach (XmlNode l_packageNode in l_document.LastChild.ChildNodes)
                {
                    Console.WriteLine("Test node name: " + l_packageNode.Name);
                    if (l_packageNode.Name == "install")
                    {
                        foreach (XmlNode l_operationNode in l_packageNode.ChildNodes)
                        {
                            Console.WriteLine("Test child node name: " + l_operationNode.Name);
                            switch (l_operationNode.Name)
                            {
                                case "readme":
                                    if (readmeTextInline)
                                        File.WriteAllText(outputDirectory.Text + "/Package/readme.txt", l_operationNode.InnerText.Replace("\r", "\n").Replace("\n", "\r\n"));
                                    break;
                                case "require-file":
                                     Console.WriteLine("File name: " + l_operationNode.Attributes["name"].Value);
                                     Console.WriteLine("Destination: " + l_operationNode.Attributes["destination"].Value);
                                     string[] pieces = l_operationNode.Attributes["name"].Value.Split('/');
                                     string lastpiece = pieces[pieces.Length - 1];

                                     if (!Directory.Exists(outputDirectory.Text + "/Source/" + l_operationNode.Attributes["name"].Value.Replace("/" + lastpiece, "")) && l_operationNode.Attributes["name"].Value != lastpiece)
                                         Directory.CreateDirectory(outputDirectory.Text + "/Source/" + l_operationNode.Attributes["name"].Value.Replace("/" + lastpiece, ""));

                                     File.Copy(packageInputPath.Text + "/" + l_operationNode.Attributes["name"].Value, outputDirectory.Text + "/Source/" + l_operationNode.Attributes["name"].Value, true);
                                     Console.WriteLine("Copied file");

                                    // Set up a query.
                                     sqlquery = "INSERT INTO files(id, file_name, destination) VALUES(null, @fileName, @destination)";
                                     sql = new SQLiteCommand(sqlquery, me.conn);

                                     sql.Parameters.AddWithValue("@fileName", l_operationNode.Attributes["name"].Value);
                                     sql.Parameters.AddWithValue("@destination", l_operationNode.Attributes["destination"].Value);

                                     sql.ExecuteNonQuery();

                                     break;
                                case "require-dir":
                                    // Just copy over the dir.
                                     DirectoryCopy(packageInputPath.Text + "/" + l_operationNode.Attributes["name"].Value, outputDirectory.Text + "/Source/" + l_operationNode.Attributes["name"].Value, true);

                                     // Set up a query.
                                     sqlquery = "INSERT INTO files(id, file_name, destination) VALUES(null, @fileName, @destination)";
                                     sql = new SQLiteCommand(sqlquery, me.conn);

                                     sql.Parameters.AddWithValue("@fileName", l_operationNode.Attributes["name"].Value);
                                     sql.Parameters.AddWithValue("@destination", l_operationNode.Attributes["destination"].Value);

                                     sql.ExecuteNonQuery();
                                    break;
                            }
                        }
                    }
                    if (l_packageNode.Name == "uninstall")
                    {
                        foreach (XmlNode l_operationNode in l_packageNode.ChildNodes)
                        {
                            switch (l_operationNode.Name)
                            {
                                case "remove-file":
                                    sqlquery = "INSERT INTO files_delete(id, file_name, type) VALUES(null, @fileName, @type)";
                                    sql = new SQLiteCommand(sqlquery, me.conn);
                                    sql.Parameters.AddWithValue("@fileName", l_operationNode.Attributes["name"].Value);
                                    sql.Parameters.AddWithValue("@type", "file");
                                    sql.ExecuteNonQuery();
                                    break;

                                case "remove-dir":
                                    sqlquery = "INSERT INTO files_delete(id, file_name, type) VALUES(null, @fileName, @type)";
                                    sql = new SQLiteCommand(sqlquery, me.conn);
                                    sql.Parameters.AddWithValue("@fileName", l_operationNode.Attributes["name"].Value);
                                    sql.Parameters.AddWithValue("@type", "dir");
                                    sql.ExecuteNonQuery();
                                    break;
                            }
                        }
                    }
                }
            }
            #endregion

            me.Close();

            DialogResult result = MessageBox.Show("The package has been converted, do you want to load it now?", "Converting Project", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                ModBuilder.Classes.PackageWorker.bootstrapLoad(outputDirectory.Text);
            Close();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            // Just close.
            Close();
        }

        private void cleanReadme_Click(object sender, EventArgs e)
        {
            readmeTXTPath.Text = "";
        }

        private void cleanInstall_Click(object sender, EventArgs e)
        {
            installXmlPath.Text = "";
        }

        private void cleanInstallCode_Click(object sender, EventArgs e)
        {
            installPHPPath.Text = "";
        }

        private void cleanDeinstallCode_Click(object sender, EventArgs e)
        {
            uninstallPHPPath.Text = "";
        }

        private void cleanDBInstall_Click(object sender, EventArgs e)
        {
            installDatabasePHPPath.Text = "";
        }

        private void cleanDBDeinstall_Click(object sender, EventArgs e)
        {
            uninstallDatabasePHPPath.Text = "";
        }
    }
}