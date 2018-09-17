using System;
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

            // TODO: load list of restricted authors
            string fileName = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors.txt";
            List<string> AuthorsName = new List<string>();

            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if(line != "" && line[0] != '/')
                        AuthorsName.Add(line);
                }
            }

            // TODO: load authors.xml
            fileName = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors.xml";
            Authors parsedData = null;

            using (TextReader textReader = new StreamReader(fileName))
            {
                using (XmlTextReader reader = new XmlTextReader(textReader))
                {
                    reader.Namespaces = false;
                    XmlSerializer serializer = new XmlSerializer(typeof(Authors));
                    parsedData = (Authors)serializer.Deserialize(reader);
                }
            }

            // TODO: Find authors from restricted list in xml file and return list of ids
            List<Author> AuthorsList = parsedData.AuthorsList;
            HashSet<Author> FoundAuthorsList = new HashSet<Author>();
            foreach (string queryName in AuthorsName)
            {
                var item = AuthorsList.Where(x => x.Name == queryName).FirstOrDefault();
                
                if(item != null)
                {
                    FoundAuthorsList.Add(item);
                }
            }

            // TODO: Print stats
            Console.WriteLine("Count of found = " + FoundAuthorsList.Count + " of " + AuthorsName.Count);


            // TODO: Load publications.xml

            fileName = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Publications.xml";
            Publications publications = null;

            using (TextReader textReader = new StreamReader(fileName))
            {
                using (XmlTextReader reader = new XmlTextReader(textReader))
                {
                    reader.Namespaces = false;
                    XmlSerializer serializer = new XmlSerializer(typeof(Publications));
                    publications = (Publications)serializer.Deserialize(reader);
                }
            }

            Console.Write("loaded");

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

            int maxcount = 0;
            Dictionary < int, List<int> > AnotherDic = new Dictionary<int, List<int>>();
            foreach (KeyValuePair<int,List<int>> item in CooperationNetwork)
            {
                if (item.Value.Count >= 2)
                {
                    AnotherDic.Add(item.Key, item.Value);
                }

                if (item.Value.Count > maxcount)
                    maxcount = item.Value.Count;
            }

            Console.WriteLine("DONE");

            // TODO: Export network of coauthors to file
        }
    }
}
