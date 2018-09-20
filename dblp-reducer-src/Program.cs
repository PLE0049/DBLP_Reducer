﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using dblp_reducer_src.Model;

namespace dblp_reducer_src
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("hello \n");

            string authorstxt = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors.txt";
            string authorsxml = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors.xml";
            string publicationsxml = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Publications.xml";

            // TODO: load list of restricted authors
            List<string> AuthorsName = Loader.TextListLoader.LoadAuthors(authorstxt);

            // TODO: load authors.xml
            Loader.XmlLoader XmlParser = new Loader.XmlLoader();
            Authors parsedData = XmlParser.LoadAuthors(authorsxml);
           
            // TODO: Find authors from restricted list in xml file and return list of ids
            List<Author> AuthorsList = parsedData.AuthorsList;
            int TotalCount = AuthorsName.Count;
            HashSet<Author> FoundAuthorsList = new HashSet<Author>();
            Dictionary<string, string> FoundNames = new Dictionary<string, string>();
            foreach (string queryName in AuthorsName)
            {
                var item = AuthorsList.Where(x => Compare( x.Name, queryName)).FirstOrDefault();

                if (item != null && !FoundNames.ContainsKey(queryName))
                {
                    FoundNames.Add(queryName, item.Name);
                    FoundAuthorsList.Add(item);
                }
            }
            
            bool Compare(string nameA, string nameB)
            {
                if(nameA == nameB)
                {
                    return true;
                }
                else
                {
                    int LastSpaceIndexA = nameA.LastIndexOf(' ');
                    int LastSpaceIndexB = nameB.LastIndexOf(' ');

                    if (LastSpaceIndexA > 0 && LastSpaceIndexB > 0)
                    {
                        string nameALastName = nameA.Substring(LastSpaceIndexA, (nameA.Length - LastSpaceIndexA));
                        string nameBLastName = nameB.Substring(LastSpaceIndexB, (nameB.Length - LastSpaceIndexB));

                        if (nameALastName == nameBLastName)
                        {
                            if (nameA.Substring(0, (nameA.IndexOf(' '))) == nameB.Substring(0, (nameB.IndexOf(' '))))
                            {
                                return true;
                            }
                        }
                    }              
                }
                return false;
            }

            // TODO: Print stats
            Console.WriteLine("Count of found = " + FoundAuthorsList.Count + " of " + TotalCount);

            foreach (KeyValuePair<string,string> kvp in FoundNames)
            {
                Console.WriteLine(kvp.Key + " - " + kvp.Value);
            }
            Console.WriteLine("-------------------");
            foreach (string name in AuthorsName)
            {
                if (!FoundNames.ContainsKey(name))
                {
                    Console.WriteLine(name);
                }
            }

            // TODO: Load publications.xml
            Publications publications = XmlParser.LoadPublications(publicationsxml);

            // TODO: Walkthrou publications and find coauthors.
            
            bool first = true;
            Dictionary<int, List<int>> CooperationNetwork = new Dictionary<int, List<int>>();
            foreach(Publication publication in publications.PublicationsList)
            {
                first = true;
                foreach ( Author author in publication.AuthorsList)
                {
                    if(FoundAuthorsList.Contains(author))
                    {
                        if(first)
                        {
                            CooperationNetwork.Add(publication.Id, new List<int>());
                            CooperationNetwork[publication.Id].Add(author.Id);
                            first = false;
                        }
                        else
                        {
                            CooperationNetwork[publication.Id].Add(author.Id);
                        }
                    }
                }
            }

            Dictionary<int, List<int>> AnotherDic = new Dictionary<int, List<int>>();
            Dictionary<int, int> NodeToIndexDict = new Dictionary<int, int>();
            Dictionary<int, int> IndexToNodeDict = new Dictionary<int, int>();
            int index = 0;
            foreach (KeyValuePair<int,List<int>> item in CooperationNetwork)
            {
                if (item.Value.Count >= 2)
                {
                    AnotherDic.Add(item.Key, item.Value);

                    foreach (int author in item.Value)
                    {
                        if(!NodeToIndexDict.ContainsKey(author))
                        {
                            NodeToIndexDict.Add(author, index);
                            IndexToNodeDict.Add(index, author);
                            index++;
                        }
                    }
                }
            }

            int[,] WeightedAdjacencyMatrix = new int[NodeToIndexDict.Count, NodeToIndexDict.Count];
            foreach (KeyValuePair<int, List<int>> item in CooperationNetwork)
            {
                for(int i = 0; i < item.Value.Count-1; i++)
                {
                    WeightedAdjacencyMatrix[NodeToIndexDict[item.Value[i]], NodeToIndexDict[item.Value[i+1]]]++;
                }
            }

            // TODO: Export network of coauthors to file
            GmlExporter exporter = new GmlExporter();
            exporter.Export("stanford.gml", FoundAuthorsList.ToList(), WeightedAdjacencyMatrix, IndexToNodeDict);
            Console.WriteLine("DONE");
        }
    }
}
