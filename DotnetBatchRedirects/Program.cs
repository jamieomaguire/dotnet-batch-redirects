using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using DotnetBatchRedirects.Models;

namespace DotnetBatchRedirects
{
    class Program
    {
        static void Main(string[] args)
        {
            RenderWelcomeMessage();
            RunApp();
        }

        /// <summary>
        /// Main application loop that can call itself to restart the program if a user encounters an exception.
        /// </summary>
        static void RunApp()
        {
            Console.WriteLine(Environment.NewLine + "Enter the full path to your redirects file");
            string redirectsPath = Console.ReadLine();
            Console.WriteLine("Enter the full path to your web.config file");
            string webconfigPath = Console.ReadLine();

            try
            {
                List<URLPair> urlPairs = GetRedirects(redirectsPath);

                XmlDocument doc = new XmlDocument();
                doc.Load(webconfigPath);
                XmlNode rewriteMaps = doc.GetElementsByTagName("rewriteMap")[0];

                Console.WriteLine("Adding redirects...");

                int totalRedirectsAdded = 0;

                // The first row in the csv is just the words 'old' and 'new'
                foreach (URLPair urlPair in urlPairs.Skip(1))
                {
                    // If the rewrite has already been added, skip to the next one
                    if (RedirectExists(doc, urlPair))
                    {
                        Console.WriteLine("Redirect already exists");
                        continue;
                    }

                    AddRedirect(doc, urlPair, rewriteMaps, ref totalRedirectsAdded);
                }

                doc.Save(webconfigPath);

                Console.WriteLine($"Completed! {totalRedirectsAdded} redirects added.");
                Console.WriteLine("Push any key to exit");
                Console.ReadKey();
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Either the redirects or web.config file could not be found, please try again.");
                CheckToContinueApp();
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                CheckToContinueApp();
            }
            catch (IndexOutOfRangeException e)
            {
                RenderRedirectFileException(e);
                CheckToContinueApp();
            }
            catch (XmlException)
            {
                Console.WriteLine("Your web.config file is either using invalid XML or the wrong file type.");
                Console.WriteLine("Please make sure to use a web.config that adheres to correct XML standards.");
                CheckToContinueApp();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Null reference exception. It looks as though the XML provided is not a web.config. Please try again.");
                CheckToContinueApp();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine($"{e.Message} Please try again.");
                CheckToContinueApp();
            }
        }

        /// <summary>
        /// Uses an XPath query to determine whether a redirect already exists in the XML file.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="urlPair"></param>
        /// <returns></returns>
        static bool RedirectExists(XmlDocument doc, URLPair urlPair)
        {
            return doc.SelectSingleNode($"//add[@key='{urlPair.OldURL}' and @value='{urlPair.NewURL}']") != null;
        }

        /// <summary>
        /// Check that the user wants to continue if they encounter an exception. If not, exit the application.
        /// </summary>
        static void CheckToContinueApp()
        {
            Console.WriteLine(Environment.NewLine + "Push Enter to continue or any other key to quit...");

            if (Console.ReadKey().Key == ConsoleKey.Enter)
                RunApp();

            Environment.Exit(0);
        }

        /// <summary>
        /// Create and return a list of URL pairs. Pairs consist of an old and new URL.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static List<URLPair> GetRedirects(string path)
        {
            List<URLPair> urlPairs = new List<URLPair>();

            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] columns = line.Split(',');

                    urlPairs.Add(new URLPair { OldURL = columns[0], NewURL = columns[1] });
                }
            }
            return urlPairs;
        }

        /// <summary>
        /// Create and populate a redirect node, then append it to the xml document.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="urlPair"></param>
        /// <param name="rewriteMaps"></param>
        /// <param name="count"></param>
        static void AddRedirect(XmlDocument document, URLPair urlPair, XmlNode rewriteMaps, ref int count)
        {
            XmlNode node = document.CreateNode("element", "add", "");

            XmlAttribute keyAttr = document.CreateAttribute("key");
            keyAttr.Value = urlPair.OldURL;

            XmlAttribute valueAttr = document.CreateAttribute("value");
            valueAttr.Value = urlPair.NewURL;

            node.Attributes.Append(keyAttr);
            node.Attributes.Append(valueAttr);

            rewriteMaps.AppendChild(node);
            count++;
            Console.WriteLine("Redirect added");
        }

        /// <summary>
        /// Display an error message to the user when the redirect file is incorrect. Also show an example of how to format the file.
        /// </summary>
        /// <param name="e"></param>
        static void RenderRedirectFileException(IndexOutOfRangeException e)
        {
            Console.WriteLine(e.Message + Environment.NewLine);
            Console.WriteLine($"Please use a CSV file for your redirects using the following format:" + Environment.NewLine);

            Console.WriteLine("====================================================================");
            Console.WriteLine("== Old ==================== New ====================================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("== /old/path/to/content === /new/path/to/content ===================");
            Console.WriteLine("====================================================================");
        }

        /// <summary>
        /// Welcome the user with some sweet ascii art.
        /// </summary>
        static void RenderWelcomeMessage()
        {
            Console.WriteLine();
            Console.WriteLine(@"______       _       _      ______         _ _               _       ");
            Console.WriteLine(@"| ___ \     | |     | |     | ___ \       | (_)             | |      ");
            Console.WriteLine(@"| |_/ / __ _| |_ ___| |__   | |_/ /___  __| |_ _ __ ___  ___| |_ ___ ");
            Console.WriteLine(@"| ___ \/ _` | __/ __| '_ \  |    // _ \/ _` | | '__/ _ \/ __| __/ __|");
            Console.WriteLine(@"| |_/ / (_| | || (__| | | | | |\ \  __/ (_| | | | |  __/ (__| |_\__ \");
            Console.WriteLine(@"\____/ \__,_|\__\___|_| |_| \_| \_\___|\__,_|_|_|  \___|\___|\__|___/");
            Console.WriteLine();
        }
    }
}
