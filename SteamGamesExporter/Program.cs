using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        // all loaded apps (appids and their names)
        static public Applist allapps;
        // all detailed appviews to save Space
        static public List<SteamData> detailedAppList = new List<SteamData>();
        // all Numbers who the user want to Export
        static public List<int> markedForExport = new List<int>();



        [STAThread]
        static void Main(string[] args)
        {
            // the selected Json File
            string jsonFile = "";
            string programmPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string filePath = programmPath + "/Gamelist.json";

            if (File.Exists(programmPath + "/filteredList.txt"))
            {
                DialogResult result = MessageBox.Show("Do you want to download a new gamelist?", "Download or Import", MessageBoxButtons.YesNo);
                switch (result)
                {
                    case DialogResult.No:

                        string[] allFilteredFiles = File.ReadAllLines(programmPath + "/filteredList.txt");

                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine("  Load old Gamelist: ");
                        allapps = new Applist(){ apps = new List<App>()};

                        for (int i = 0; i < allFilteredFiles.Length; i++)
                        {

                            Console.SetCursorPosition(0, 2);
                            Console.WriteLine("  Progress: " + i + "|" + allFilteredFiles.Length + " " + Math.Round((float)i / ((float)allFilteredFiles.Length / 100)) + "%");


                            string[] textpart = allFilteredFiles[i].Split(',');
                            allapps.apps.Add(new App() { appid = int.Parse(textpart[0]), name = textpart[1] });
                            detailedAppList.Add(new SteamData());
                        }
                        break;
                    case DialogResult.Yes:
                        jsonFile = GetJsonHttpRequest("https://api.steampowered.com/ISteamApps/GetAppList/v2/").Result;
                        File.Delete(filePath);
                        File.Delete(programmPath + "/filteredList.txt");
                        using (StreamWriter sw = File.AppendText(filePath))
                        {
                            sw.Write(jsonFile);
                            sw.Flush();
                            sw.Close();
                        }
                        // Converts the Json to a Elementslist
                        allapps = JsonConvert.DeserializeObject<AllApps>(jsonFile).applist;

                        allapps.FilterList();
                        using (StreamWriter sw = File.AppendText(programmPath + "/filteredList.txt"))
                        {
                            foreach (App app in allapps.apps)
                            {
                                sw.WriteLine(app.appid + "," + app.name);
                                detailedAppList.Add(new SteamData());
                            }
                            sw.Flush();
                            sw.Close();
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                jsonFile = GetJsonHttpRequest("https://api.steampowered.com/ISteamApps/GetAppList/v2/").Result;
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.Write(jsonFile);
                    sw.Flush();
                    sw.Close();
                }
                // Converts the Json to a Elementslist
                allapps = JsonConvert.DeserializeObject<AllApps>(jsonFile).applist;

                allapps.FilterList();
                using (StreamWriter sw = File.AppendText(programmPath + "/filteredList.txt"))
                {
                    foreach (App app in allapps.apps)
                    {
                        sw.WriteLine(app.appid + "," + app.name);
                    }
                    sw.Flush();
                    sw.Close();
                }
            }



            SelectionMenu();
        }


        public static void SelectionMenu()
        {
            // if the User want to exit the menu
            bool isExiting = false;
            // shows selected page number
            int pageNumber = 0;
            // shows selected Button
            int cursorHeight = 0;

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
                SteamData currentlySelectedApp = detailedAppList[pageNumber];
                
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
                        if (currentlySelectedApp != null && currentlySelectedApp.name != null)
                        {
                            cursorHeight = (cursorHeight < 7) ? cursorHeight + 1 : 0;
                        }
                        break;
                    case (ConsoleKey.UpArrow):
                        // goes a menu button up
                        if (currentlySelectedApp != null && currentlySelectedApp.name != null)
                        {
                            cursorHeight = (cursorHeight > 0) ? cursorHeight - 1 : 7;
                        }
                        break;
                    case (ConsoleKey.Spacebar):
                        // Menu:
                        // 0 Show image
                        // 1 Show trailer
                        // 2 Mark for export
                        // 3 Show all details
                        // 4 Export this game
                        // 5 Export all marked gamefiles
                        // 6 Jump to Position
                        // 7 Open Steam page
                        
                        // select the button
                        if (currentlySelectedApp != null && currentlySelectedApp.name != null)
                        {
                            if (cursorHeight == 0 && currentlySelectedApp.screenshots != null)
                            {
                                foreach (Screenshot path in currentlySelectedApp.screenshots)
                                {
                                    Process.Start(path.path_full);
                                }
                            }
                            else if (cursorHeight == 1 && currentlySelectedApp.movies != null)
                            {
                                foreach (Movy path in currentlySelectedApp.movies)
                                {
                                    if (string.IsNullOrWhiteSpace(path.webm._480))
                                    {
                                        if (string.IsNullOrWhiteSpace(path.webm.max))
                                        {

                                            if (string.IsNullOrWhiteSpace(path.mp4._480))
                                            {

                                                if (string.IsNullOrWhiteSpace(path.mp4.max))
                                                {

                                                }
                                                else
                                                {
                                                    Process.Start(path.mp4.max);
                                                }
                                            }
                                            else
                                            {
                                                Process.Start(path.mp4._480);
                                            }
                                        }
                                        else
                                        {
                                            Process.Start(path.webm.max);
                                        }
                                    }
                                    else
                                    {
                                        Process.Start(path.webm._480);
                                    }
                                }
                            }
                            else if (cursorHeight == 2)
                            {
                                if (markedForExport.Contains(pageNumber))
                                {
                                    markedForExport.Remove(pageNumber);
                                }
                                else
                                {
                                    markedForExport.Add(pageNumber);
                                }
                            }
                            else if (cursorHeight == 3)
                            {
                                Process.Start(newDetailLink);
                            }
                            else if (cursorHeight == 4)
                            {
                                ExportApp(currentlySelectedApp);
                            }
                            else if (cursorHeight == 5)
                            {
                                ExportAllMarkedApps();
                            }
                            else if (cursorHeight == 6)
                            {
                                Console.Clear();
                                Console.WriteLine();
                                Console.WriteLine("0-" + allapps.apps.Count);
                                Console.Write("Where do you wanna go?:");
                                string result = Console.ReadLine();
                                if (int.TryParse(result, out int parsed))
                                {
                                    pageNumber = parsed;
                                }
                            }
                            else if (cursorHeight == 7)
                            {
                                Process.Start("https://store.steampowered.com/app/" + allapps.apps[pageNumber].appid.ToString() + "/" + currentlySelectedApp.name + "/");
                            }

                        }
                        break;
                    default:
                        break;
                }
                if (detailedAppList[pageNumber] == null || detailedAppList[pageNumber].name == null)
                {
                    newDetailLink = "https://store.steampowered.com/api/appdetails?appids=" + allapps.apps[pageNumber].appid.ToString();
                    try
                    {
                        jsonNewData = GetJsonHttpRequest(newDetailLink).Result;
                        number = jsonNewData.Substring(0, 20);
                        jsonNewData = jsonNewData.Remove(0, 20);
                        number = number.Replace(allapps.apps[pageNumber].appid.ToString(), "game");
                        jsonNewData = number + jsonNewData;
                    }
                    catch (Exception)
                    {
                        DialogResult result = MessageBox.Show("An Error happend, do you want to export the marked games?", "Error", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            ExportAllMarkedApps();
                        }
                        Environment.Exit(0);
                    }


                    try
                    {
                        SteamRoot newGame = JsonConvert.DeserializeObject<SteamRoot>(jsonNewData);
                        newData = newGame.game.data;
                        detailedAppList[pageNumber] = newData;
                    }
                    catch (Newtonsoft.Json.JsonSerializationException)
                    {
                        detailedAppList[pageNumber] = new SteamData() { name = "$NoGame$"};
                    }

                }
                ReloadSelectionMenu(pageNumber, cursorHeight);





            } while (isExiting == false);






        }

        public static void ExportAllMarkedApps()
        {
            DialogResult result = MessageBox.Show("Do you want to include the Movie File?", "File or Path", MessageBoxButtons.YesNo);

            bool includeMovieFile = false;

            switch (result)
            {
                case DialogResult.Yes:
                    includeMovieFile = true;
                    break;
                default:
                    break;
            }

            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("  Filter Gamelist: (Searching for invalid Names)");
            for (int i = 0; i < markedForExport.Count; i++)
            {
                Console.SetCursorPosition(0, 2);
                Console.WriteLine("  Fortschritt: " + i + "|" + markedForExport.Count + " " + Math.Round((float)i / ((float)markedForExport.Count / 100)) + "%");
                if (detailedAppList[markedForExport[i]].name != "$NoGame$")
                {
                    ExportApp(detailedAppList[markedForExport[i]], includeMovieFile);
                }
            }
            markedForExport.Clear();
        }


        public static void ExportApp(SteamData data, bool includeMovieFile = true)
        {
            string programmPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            // folder where all Images will be saved
            string imageFolder = programmPath + "/AssetFiles";
            // folder where all Files will be saved
            string fileFolder = programmPath + "/dataOutput";
            // Loading Bar
            Console.SetCursorPosition(0, 3);
            Console.WriteLine(" |oooooooooo|");
            Console.SetCursorPosition(2, 3);
            Console.ForegroundColor = ConsoleColor.Green;
            Directory.CreateDirectory(imageFolder);
            Directory.CreateDirectory(fileFolder);

            string nameFolder = System.Text.RegularExpressions.Regex.Replace(data.name, "[^A-Za-z^0-9]", "");

            // if the files exists they delete it to update all files
            if (Directory.Exists(imageFolder + "/" + nameFolder))
            {
                Directory.Delete(imageFolder + "/" + nameFolder,true);
            }
            if (File.Exists(fileFolder + "/" + nameFolder + ".txt"))
            {
                File.Delete(fileFolder + "/" + nameFolder + ".txt");
            }

            Directory.CreateDirectory(imageFolder + "/" + nameFolder);

            // Loading Progress 10%
            Console.Write("o");

            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(data.header_image), imageFolder + "/" + nameFolder + "/headerimage" + ".png");
                // Loading Progress 20%
                Console.Write("o");
                if (data.screenshots != null)
                {

                    foreach (Screenshot photo in data.screenshots)
                    {
                        client.DownloadFile(new Uri(photo.path_full), imageFolder + "/" + nameFolder + "/" + photo.id + ".png");
                    }
                }
                // Loading Progress 30%
                Console.Write("o");
                
                if (includeMovieFile && data.movies != null)
                {
                    foreach (Movy path in data.movies)
                    {

                        if (string.IsNullOrWhiteSpace(path.webm._480))
                        {
                            if (string.IsNullOrWhiteSpace(path.webm.max))
                            {

                                if (string.IsNullOrWhiteSpace(path.mp4._480))
                                {

                                    if (string.IsNullOrWhiteSpace(path.mp4.max))
                                    {

                                    }
                                    else
                                    {
                                        client.DownloadFile(new Uri(path.mp4.max), imageFolder + "/" + nameFolder + "/" + path.id + ".mp4");
                                    }
                                }
                                else
                                {
                                    client.DownloadFile(new Uri(path.mp4._480), imageFolder + "/" + nameFolder + "/" + path.id + ".mp4");
                                }
                            }
                            else
                            {
                                client.DownloadFile(new Uri(path.webm.max), imageFolder + "/" + nameFolder + "/" + path.id + ".wbm");
                            }
                        }
                        else
                        {
                            client.DownloadFile(new Uri(path.webm._480), imageFolder + "/" + nameFolder + "/" + path.id + ".wbm");
                        }
                    }
                }
                // Loading Progress 40%
                Console.Write("o");

            }

            // Loading Progress 50%
            Console.Write("o");

            using (StreamWriter sw = File.AppendText(fileFolder + "/" + nameFolder + ".txt"))
            {

                //data.name;
                //data.price_overview.initial;
                //data.header_image;
                //data.short_description;
                //data.publishers[0];
                //data.genres[0];
                sw.WriteLine(data.name);

                if (data.price_overview == null)
                {
                    if (data.release_date.coming_soon)
                    {
                        sw.WriteLine("NoPrice");
                    }
                    else
                    {
                        sw.WriteLine("Free");
                    }
                }
                else
                {
                    sw.WriteLine(data.price_overview.final_formatted.Replace("€", ""));
                }

                sw.WriteLine(data.short_description);
                sw.WriteLine(data.publishers[0]);
                sw.WriteLine(data.genres[0].description);
                sw.WriteLine("19"); // Taxrate

                // Loading Progress 60%
                Console.Write("o");
                if (data.screenshots != null)
                {

                    foreach (Screenshot photo in data.screenshots)
                    {
                        sw.Write("/" + data.name + "/" + photo.id + ".png|");
                    }
                }
                sw.WriteLine();
                // Loading Progress 70%
                Console.Write("o");
                if (data.movies != null)
                {

                    if (includeMovieFile)
                    {

                        foreach (Movy movie in data.movies)
                        {
                            if (string.IsNullOrWhiteSpace(movie.webm._480))
                            {
                                if (string.IsNullOrWhiteSpace(movie.webm.max))
                                {

                                    if (string.IsNullOrWhiteSpace(movie.mp4._480))
                                    {

                                        if (string.IsNullOrWhiteSpace(movie.mp4.max))
                                        {

                                        }
                                        else
                                        {
                                            sw.Write("/" + data.name + "/" + movie.id + ".mp4|");
                                        }
                                    }
                                    else
                                    {
                                        sw.Write("/" + data.name + "/" + movie.id + ".mp4|");
                                    }
                                }
                                else
                                {
                                    sw.Write("/" + data.name + "/" + movie.id + ".wbm|");
                                }
                            }
                            else
                            {
                                sw.Write("/" + data.name + "/" + movie.id + ".wbm|");
                            }
                        }
                    }
                    else
                    {

                        foreach (Movy movie in data.movies)
                        {
                            if (string.IsNullOrWhiteSpace(movie.webm._480))
                            {
                                if (string.IsNullOrWhiteSpace(movie.webm.max))
                                {

                                    if (string.IsNullOrWhiteSpace(movie.mp4._480))
                                    {

                                        if (string.IsNullOrWhiteSpace(movie.mp4.max))
                                        {

                                        }
                                        else
                                        {
                                            sw.Write(movie.mp4.max + "|");
                                        }
                                    }
                                    else
                                    {
                                        sw.Write(movie.mp4._480 + "|");
                                    }
                                }
                                else
                                {
                                    sw.Write(movie.webm.max + "|");
                                }
                            }
                            else
                            {
                                sw.Write(movie.webm._480 + "|");
                            }
                        }
                    }
                }
                // Loading Progress 80%
                Console.Write("o");
                sw.WriteLine();

            }
            // Loading Progress 90%
            Console.Write("o");
            // Loading Progress 100%
            Console.Write("o");
            Console.ResetColor();




            //data.name;
            //data.price_overview.initial;
            //data.header_image;
            //data.short_description;
            //data.publishers[0];
            //data.genres[0];




            // ProductName
            // NetUnitPrice
            // ImagePath
            // Description
            // ManufacturerName
            // CategoryName
            // Tax 19 %
        }
        public static void ReloadSelectionMenu(int selectedPage, int cursorHeight)
        {
            SteamData selectedApp = detailedAppList[selectedPage];
            Console.Clear();
            Console.WriteLine("                                 ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(" <- Page: ");

            string spaceAfterPage = "          ";
            spaceAfterPage = spaceAfterPage.Remove(spaceAfterPage.Length - selectedPage.ToString().Length);
            spaceAfterPage = spaceAfterPage.Remove(spaceAfterPage.Length - markedForExport.Count.ToString().Length);

            Console.WriteLine(selectedPage + spaceAfterPage + " Marked:" + markedForExport.Count +" -> ");

            if (selectedApp != null && 
                selectedApp.name != null && 
                selectedApp.name != "$NoGame$" &&
                selectedApp.header_image != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                string gameName = selectedApp.name;
                if (gameName.Length > 31)
                {
                    gameName = gameName.Remove(30) + ".";
                }

                Console.WriteLine(" " + gameName);
                Console.WriteLine((selectedApp.movies != null) ? " Trailers:" + selectedApp.movies.Count : " No Trailers");
                Console.WriteLine((selectedApp.screenshots != null) ? " Screenshots:" + selectedApp.screenshots.Count : " No Screenshots");
                Console.WriteLine((selectedApp.release_date.coming_soon) ? " Not Out Yet" : " Released since: " + selectedApp.release_date.date);
                Console.WriteLine((selectedApp.price_overview != null) ? " " + selectedApp.price_overview.final_formatted.Replace("€", " EUR"): 
                                 /* if the Game is not out but it's free */(selectedApp.release_date.coming_soon)?" No Prize avaible":" Free");
                if (selectedApp.publishers.Count > 0)
                {
                    Console.WriteLine(" Publisher:");
                    foreach (string company in selectedApp.publishers)
                    {
                        Console.WriteLine("       " + company);
                    }

                }
                Console.WriteLine("                                 ");

                if (selectedApp.screenshots == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine((cursorHeight == 0) ? ">No screenshots<" : " No screenshots");
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine((cursorHeight == 0) ? ">Show screenshots<" : " Show screenshots");
                }
                if ( selectedApp.movies == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine((cursorHeight == 1) ? ">No trailer<" : " No trailer");
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine((cursorHeight == 1) ? ">Show trailer<" : " Show trailer");
                }
                Console.ForegroundColor = ConsoleColor.White;

                if (selectedApp.release_date.coming_soon == false)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" Recomended to Export");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" Not Recomended for Export");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write((cursorHeight == 2) ? ">Mark for export<" : " Mark for export");
                Console.WriteLine((markedForExport.Contains(selectedPage) ? " (Marked)" : " (Not Marked)"));
                Console.WriteLine((cursorHeight == 3) ? ">Show all details<" : " Show all details");
                Console.WriteLine((cursorHeight == 4) ? ">Export this game<" : " Export this game");
                Console.WriteLine((cursorHeight == 5) ? ">Export all marked gamefiles<" : " Export all marked gamefiles");
                Console.WriteLine((cursorHeight == 6) ? ">Jump to page<" : " Jump to page");
                Console.WriteLine((cursorHeight == 7) ? ">Open Steam page<" : " Open Steam page");

                Console.ResetColor();

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Game is Invalid");
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
