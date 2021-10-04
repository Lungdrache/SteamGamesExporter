using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using SteamGamesExporter.Classes;
using SteamGamesExporter.Classes.Steam;

namespace SteamGamesExporter
{
    class Program
    {
        static public Applist allapps;
        static public List<SteamData> detailedAppList = new List<SteamData>();




        [STAThread]
        static void Main(string[] args)
        {
            // the selected Json File
            string jsonFile = "";

            DialogResult result = MessageBox.Show("Do you want to import a Steam json file? else we download a new one.", "Download or Import", MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.Yes:
                    // open FileDialog to search for the Steam Json File
                    jsonFile = OpenFileExplorer("JSON-File (*.json)|*.json");
                    break;
                case DialogResult.No:
                    jsonFile = GetJsonHttpRequest("https://api.steampowered.com/ISteamApps/GetAppList/v2/").Result;
                    break;
                default:
                    break;
            }


            // Converts the Json to a Elementslist
            allapps = JsonConvert.DeserializeObject<AllApps>(jsonFile).applist;
            allapps.FilterList();
            SelectionMenu();
        }


        public static void SelectionMenu()
        {
            // if the User want to exit the menu
            bool isExiting = false;
            // if the User wish to export the selected Files
            bool isExporting = false;
            // shows selected page number
            int pageNumber = 0;
            // shows selected Button
            int cursorHeight = 0;
            // all Numbers who the user want to Export
            Queue<int> markedForExport = new Queue<int>();

            //preparing the First GameData
            string number = "";

            string newDetailLink = "https://store.steampowered.com/api/appdetails?appids=" + allapps.apps[pageNumber].appid.ToString();
            string jsonNewData = GetJsonHttpRequest(newDetailLink).Result;
            number = jsonNewData.Substring(0, 20);
            jsonNewData = jsonNewData.Remove(0, 20);
            number = number.Replace(allapps.apps[pageNumber].appid.ToString(), "game");
            jsonNewData = number + jsonNewData;
            SteamData newData = JsonConvert.DeserializeObject<SteamRoot>(jsonNewData).game.data;

            detailedAppList.Add(newData);

            ReloadSelectionMenu(pageNumber, cursorHeight);

            do
            {
                ConsoleKeyInfo pressed = Console.ReadKey();

                switch (pressed.Key)
                {
                    case (ConsoleKey.LeftArrow):
                        // goes a page return if it isn't the first
                        pageNumber -= (pageNumber > 0)?1: 0;
                        break;
                    case (ConsoleKey.RightArrow):
                        // goes a page forward if it isn't the last
                        pageNumber += (pageNumber < allapps.apps.Count())?1: 0;
                        break;
                    case (ConsoleKey.DownArrow):
                        // goes a menu button down
                        cursorHeight = (cursorHeight < 5) ? cursorHeight+1 : 0;
                        break;
                    case (ConsoleKey.UpArrow):
                        // goes a menu button up
                        cursorHeight = (cursorHeight > 0) ? cursorHeight-1 : 5;
                        break;
                    case (ConsoleKey.Spacebar):
                        // Menu:
                        // 0 Show image
                        // 1 Show trailer
                        // 2 Mark for export
                        // 3 Show all details
                        // 4 Export this game
                        // 5 Export all marked gamefiles
                        // select the button
                        
                        break;
                    default:
                        break;
                }
                if (pageNumber > detailedAppList.Count()-1)
                {
                    newDetailLink = "https://store.steampowered.com/api/appdetails?appids=" + allapps.apps[pageNumber].appid.ToString();
                    jsonNewData = GetJsonHttpRequest(newDetailLink).Result;
                    number = jsonNewData.Substring(0,20);
                    jsonNewData = jsonNewData.Remove(0, 20);
                    number = number.Replace(allapps.apps[pageNumber].appid.ToString(), "game");
                    jsonNewData = number + jsonNewData;
                    try
                    {
                        SteamRoot newGame = JsonConvert.DeserializeObject<SteamRoot>(jsonNewData);
                        newData = newGame.game.data;
                        detailedAppList.Add(newData);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException)
                    {
                        detailedAppList.Add(new SteamData());
                    }

                }
                ReloadSelectionMenu(pageNumber, cursorHeight);





            } while (isExiting == false);






        }
        public static void ReloadSelectionMenu(int selectedPage, int cursorHeight)
        {
            SteamData selectedApp = detailedAppList[selectedPage];
            Console.Clear();
            Console.WriteLine("                                 ");
            Console.Write(" <- Page: ");

            string spaceAfterPage = "                    ";
            spaceAfterPage = spaceAfterPage.Remove(spaceAfterPage.Length - selectedPage.ToString().Length);

            Console.WriteLine(selectedPage + spaceAfterPage +"-> ");
            //31
            if (selectedApp != null && selectedApp.name != null)
            {

            string gameName = selectedApp.name;
            if (gameName.Length > 31)
            {
                gameName = gameName.Remove(30) + ".";
            }

            Console.WriteLine(" " + gameName);
            Console.WriteLine((selectedApp.movies != null)? " Trailers:" + selectedApp.movies.Count:" No Trailers");
            Console.WriteLine((selectedApp.screenshots != null)? " Screenshots:" + selectedApp.screenshots.Count: " No Screenshots");
            Console.WriteLine((cursorHeight == 0) ? ">Show screenshots" : " Show screenshots") ;
            Console.WriteLine((cursorHeight == 1) ? ">Show trailer" : " Show trailer") ;
            Console.WriteLine((cursorHeight == 2) ? ">Mark for export" : " Mark for export") ;
            Console.WriteLine((cursorHeight == 3) ? ">Show all details" : " Show all details") ;
            Console.WriteLine((cursorHeight == 4) ? ">Export this game" : " Export this game") ;
            Console.WriteLine((cursorHeight == 5) ? ">Export all marked gamefiles" : " Export all marked gamefiles") ;

            }
            else
            {
                Console.WriteLine(" No Existing Game Found");
            }











        }



        static async Task<string> GetJsonHttpRequest(string link)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(link);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
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
