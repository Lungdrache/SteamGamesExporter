using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SteamGamesExporter.Classes;

namespace SteamGamesExporter
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // the selected Json File
            string jsonFile = "";

            // open FileDialog to search for the Steam Json File
            jsonFile = OpenFileExplorer("JSON-File (*.json)|*.json");

            // Converts the Json to a Elementslist
            Applist list =
                JsonConvert.DeserializeObject<AllApps>(jsonFile).applist;

        }


        public void ShowMenu()
        {

        }


        // is used as simple FIleDialog Solution to start it whenever needed
        static string OpenFileExplorer(string filter,string startFolder = "c:\\")
        {
            // content of the selected File
            var fileContent = string.Empty;
            // Path of the whole File
            var filePath = string.Empty;

            // uses the filedialog to Open files
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Prepare all settings for the fileDialog
                openFileDialog.InitialDirectory = startFolder;
                openFileDialog.Filter = filter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                DialogResult diaResult;
                // Repeat by accidently clicking wrong or no file
                do
                {
                    diaResult = openFileDialog.ShowDialog();
                    // if a files was selected
                    if (!string.IsNullOrWhiteSpace(openFileDialog.FileName))
                    {

                        // get the path of specified file
                        filePath = openFileDialog.FileName;

                        // read the contents of the file into a stream
                        var fileStream = openFileDialog.OpenFile();

                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            fileContent = reader.ReadToEnd();
                        }

                        // if the result was negative by selecting a wrong file
                        if (diaResult != DialogResult.OK)
                        {
                            DialogResult result = MessageBox.Show("Want to try again?", "Wrong File", MessageBoxButtons.RetryCancel);
                            switch (result)
                            {
                                case DialogResult.Cancel:
                                    Environment.Exit(0);
                                    break;
                                case DialogResult.Retry:

                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    // if the result was negative by canceling or exiting the file dialog
                    else
                    {

                        DialogResult result = MessageBox.Show("Want to Exit?", "Error", MessageBoxButtons.YesNo);
                        switch (result)
                        {
                            case DialogResult.Yes:
                                Environment.Exit(0);
                                break;
                            case DialogResult.No:
                                break;
                            default:
                                break;
                        }
                    }
                } while (diaResult != DialogResult.OK || string.IsNullOrWhiteSpace(fileContent));

                // return Json string text file
                return fileContent;
            }
        }
        
    }
}
