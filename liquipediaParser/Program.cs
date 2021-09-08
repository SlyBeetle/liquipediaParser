using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace liquipediaParser
{
    class StarCraftObject : IComparable
    {
        public string name;
        public string minerals;
        public string gas;
        public string time;
        public string supply;

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            StarCraftObject otherStarCraftObject = obj as StarCraftObject;
            if (otherStarCraftObject != null)
                return this.name.CompareTo(otherStarCraftObject.name);
            else
                throw new ArgumentException("Object is not a StarCraftObject");
        }
    }

    class Program
    {
        static void Main()
        {
            const string LIQUIPEDIA = "http://wiki.teamliquid.net";
            string[] startPages = {
                                      LIQUIPEDIA + "/starcraft2/Protoss_Units",
                                      LIQUIPEDIA + "/starcraft2/Terran_Units",
                                      LIQUIPEDIA + "/starcraft2/Zerg_Units"
                                  };
            SortedDictionary<string, SortedSet<StarCraftObject>> starCraftObjectsByRace = new SortedDictionary<string, SortedSet<StarCraftObject>>();
            foreach (string startPage in startPages)
            {
                HtmlWeb htmlWeb = new HtmlWeb();
                HtmlDocument htmlDocument = htmlWeb.Load(startPage);
                const string CONTENT_DIV_ID = "mw-content-text";
                HtmlNode contentNode = htmlDocument.GetElementbyId(CONTENT_DIV_ID);
                HtmlNode table = contentNode.SelectSingleNode("//table");

                SortedSet<string> urls = new SortedSet<string>();
                foreach (HtmlNode descendant in table.Descendants())
                {
                    if (descendant.Name == "a" && descendant.WriteContentTo() != "edit")
                    {
                        urls.Add(descendant.Attributes["href"].Value);
                    }
                }
                SortedSet<StarCraftObject> starCraftObjects = new SortedSet<StarCraftObject>();
                foreach (string url in urls)
                {
                    htmlDocument = htmlWeb.Load(LIQUIPEDIA + url);

                    contentNode = htmlDocument.GetElementbyId(CONTENT_DIV_ID);
                    contentNode = contentNode.SelectSingleNode("div/div[@class=\"infobox\"]//b");

                    StarCraftObject starCraftObject = new StarCraftObject();
                    starCraftObject.name = contentNode.WriteContentTo();
                    if (starCraftObject.name == "MULE" || starCraftObject.name.StartsWith("Cocoon") || starCraftObject.name == "Larva")
                    {
                        continue;
                    }

                    contentNode = htmlDocument.GetElementbyId(CONTENT_DIV_ID);
                    table = contentNode.SelectSingleNode("div/div[@class=\"infobox\"]//table");
                    contentNode = table.SelectSingleNode("tr/td/a[img and @title = \"Minerals\"]");

                    contentNode = contentNode.NextSibling;
                    starCraftObject.minerals = contentNode.WriteTo().Trim();
                    contentNode = contentNode.NextSibling.NextSibling;
                    starCraftObject.gas = contentNode.WriteTo().Trim();
                    contentNode = contentNode.NextSibling.NextSibling;
                    starCraftObject.time = contentNode.WriteTo().Trim();
                    try
                    {
                        contentNode = contentNode.NextSibling.NextSibling;
                        starCraftObject.supply = contentNode.WriteTo().Trim();
                    }
                    catch (NullReferenceException)
                    {
                        starCraftObject.supply = "0";
                    }
                    starCraftObjects.Add(starCraftObject);
                }
                starCraftObjectsByRace.Add(startPage, starCraftObjects);
                Console.WriteLine("{0}", startPage);
                foreach (StarCraftObject sco in starCraftObjects)
                {
                    Console.WriteLine("     {0} {1} {2} {3} {4}", sco.name, sco.minerals, sco.gas, sco.time, sco.supply);
                }
            }
            Console.ReadKey();
        }
    }
}
