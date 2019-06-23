using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using dblp_reducer_src.Model;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections;
using dblp_reducer_src.Exporter;

namespace dblp_reducer_src
{
    class Program
    {
       

        static void Main(string[] args)
        {
            // string authorstxt = @"StanBerk.txt";

            string authorstxt = @"StanBerk.txt";

            // TODO: load list of restricted authors
            List<string> AuthorsName = Loader.TextListLoader.LoadAuthors(authorstxt);
            Dictionary<int, Dictionary<Tuple<int, int>, int>> TemporalNet = new Dictionary<int, Dictionary<Tuple<int, int>, int>>();
            Dictionary<string, int> NameAuthor = new Dictionary<string, int>();
            Dictionary<Tuple<int, int>, int> UniqueEdges= new Dictionary<Tuple<int, int>, int>();
            Dictionary<int, int> FirstPersongAppearance = new Dictionary<int, int>();

            int index = 0;
            foreach(string name in AuthorsName)
            {
                NameAuthor.Add(name, index);
                index++;
            }

                string html = string.Empty;           

            List<string> FoundAuthors = new List<string>();
            Dictionary<string, string> DictAuthorName = new Dictionary<string, string>();
            Dictionary<string, XDocument> Documents = new Dictionary<string, XDocument>();
            HashSet<Publication> DicPublication = new HashSet<Publication>();
            List<Publication> NewPublications = new List<Publication>();
            

            Publications publications = new Publications();
            foreach (string author_name in AuthorsName)
            {
                System.Threading.Thread.Sleep(1000);

                XDocument doc = GetAuthorDetail(author_name, FoundAuthors, null);

                while (!(bool)doc.XPathEvaluate("boolean(/dblpperson/r)"))
                {
                    if ((bool)doc.XPathEvaluate("boolean(/dblpperson/homonyms )"))
                    {
                        var urlh = ((IEnumerable)doc.XPathEvaluate("/dblpperson/homonyms/h/@f")).Cast<XAttribute>().FirstOrDefault()?.Value;
                        doc = GetAuthorDetail(author_name, FoundAuthors, urlh);
                        break;
                    }

                    string pname = ((IEnumerable)doc.XPathEvaluate("/dblpperson/@pname")).Cast<XAttribute>().FirstOrDefault()?.Value;
                    string url = ((IEnumerable)doc.XPathEvaluate("/dblpperson/@f")).Cast<XAttribute>().FirstOrDefault()?.Value;

                    doc = GetAuthorDetail(pname, FoundAuthors, url);
                }

                var query = doc.XPathEvaluate("/dblpperson/r");

                Documents.Add(author_name, doc);
            }

            foreach (string author_name in AuthorsName)
            {
                XDocument doc = Documents[author_name];

                var query = doc.XPathEvaluate("/dblpperson/r");
                foreach (var pub in (IEnumerable)query)
                {
                    MemoryStream strm = new MemoryStream(Encoding.UTF8.GetBytes(pub.ToString()));
                    XDocument docNav = XDocument.Load(strm);
                    var names = docNav.XPathEvaluate("/r/*/author");
                    var year = docNav.XPathEvaluate("string(/r/*/year)");
                    Tuple<int, int> Edge;

                    foreach (var current_row in (IEnumerable)names)
                    {
                        var name = current_row.ToString().Substring(current_row.ToString().IndexOf('>') + 1, current_row.ToString().LastIndexOf('<') - current_row.ToString().IndexOf('>') - 1);
                        if (author_name != name && NameAuthor.ContainsKey(name))
                        {
                            int yearInt = Int32.Parse(year.ToString());

                            if(!TemporalNet.ContainsKey(yearInt))
                                TemporalNet.Add(yearInt, new Dictionary<Tuple<int, int>, int>());

                            if(NameAuthor[name.ToString()] < NameAuthor[author_name])
                                Edge = new Tuple<int, int>(NameAuthor[name.ToString()], NameAuthor[author_name]);
                            else
                                Edge = new Tuple<int, int>(NameAuthor[author_name], NameAuthor[name.ToString()]);

                            if (!FirstPersongAppearance.ContainsKey(Edge.Item1))
                            {
                                FirstPersongAppearance.Add(Edge.Item1, yearInt);
                            }
                            else if(FirstPersongAppearance[Edge.Item1] > yearInt)
                            {
                                FirstPersongAppearance[Edge.Item1] = yearInt;
                            }

                            if (!FirstPersongAppearance.ContainsKey(Edge.Item2))
                            {
                                FirstPersongAppearance.Add(Edge.Item2, yearInt);
                            }
                            else if (FirstPersongAppearance[Edge.Item2] > yearInt)
                            {
                                FirstPersongAppearance[Edge.Item2] = yearInt;
                            }

                            if (TemporalNet[yearInt].ContainsKey(Edge))
                                TemporalNet[yearInt][Edge]++;
                            else
                            {
                                TemporalNet[yearInt].Add(Edge, 1);

                                if(!UniqueEdges.ContainsKey(Edge))
                                    UniqueEdges.Add(Edge, 1);
                            }
                                
                        }
                    }
                }
            }



            GEXFExporter export = new GEXFExporter();
            export.Export("stanberk.gexf", NameAuthor , FirstPersongAppearance, UniqueEdges, TemporalNet);

        }

        public static string publications_url(string author_url)
        {
            return @"https://dblp.uni-trier.de/pers/xx/" + author_url + "?h=1000";
        }

        public static string pub_detail_url(string dblpkey)
        {
            return @"https://dblp.uni-trier.de/rec/xml/" + dblpkey + ".xml";
        }

        public static XDocument GetAuthorDetail(string author_name, List<string> FoundAuthors, string url)
        {
            string search_url = @"http://dblp.uni-trier.de/search/author?xauthor=";

            if(url == null)
            {
                string html = GetHtmlFromURL(search_url + author_name);

                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
                XmlSerializer serializer = new XmlSerializer(typeof(Authors));
                Authors parsedData2 = (Authors)serializer.Deserialize(memStream);

                Author author = parsedData2.AuthorsList[0];

                var item = author_name == author.NameW ? author_name : author.NameW;
                FoundAuthors.Add(item);
                html = GetHtmlFromURL(publications_url(author.url));
                memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
                XDocument doc = XDocument.Load(memStream);
                return doc;
            }
            else
            {
                string html = GetHtmlFromURL(publications_url(url));
                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));            
                XDocument doc = XDocument.Load(memStream);
                return doc;
            }          
        }

        public static string GetHtmlFromURL(string url)
        {
            string html;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            return html;
        }

        static void Main2(string[] args)
        {
            Console.Write("hello \n");

            string authorstxt = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors.txt";

            // TODO: load list of restricted authors
            List<string> AuthorsName = Loader.TextListLoader.LoadAuthors(authorstxt);

            string html = string.Empty;
            string url = @"https://dblp.uni-trier.de/search/author?xauthor=Jure%20Leskovec";


            string search_url = @"http://dblp.uni-trier.de/search/author?xauthor=";       

            string coauthors_url( string author_url)
            {
                return @"http://dblp.uni-trier.de/rec/pers/"+ author_url + "/xc";
            }

            List<Author> FoundAuthors = new List<Author>();
            Dictionary<string, int> DictAuthor = new Dictionary<string, int>();
            Dictionary<int, string> NameAuthor = new Dictionary<int, string>();

            int index = 0;
            foreach (string author_name in AuthorsName)
            {
                System.Threading.Thread.Sleep(1000);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(search_url+author_name);
                //request.AutomaticDecompression = DecompressionMethods.GZip;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
                XmlSerializer serializer = new XmlSerializer(typeof(Authors));
                Authors parsedData = (Authors)serializer.Deserialize(memStream);
                if(parsedData.AuthorsList.Count > 0)
                {
                    Author NewAuthor = parsedData.AuthorsList[0];
                    NewAuthor.Id = index;
                    NewAuthor.Name = NewAuthor.NameW;

                    if(!DictAuthor.ContainsKey(NewAuthor.url))
                    {
                        FoundAuthors.Add(NewAuthor);
                        DictAuthor.Add(NewAuthor.url, index);
                        NameAuthor.Add(NewAuthor.Id, NewAuthor.NameW);
                        index++;
                    }
                }
                    
            }

            System.Threading.Thread.Sleep(5000);
            /*
            using (StreamWriter outputFile = new StreamWriter("FoundAuthors.txt"))
            {
                foreach (Author a in FoundAuthors)
                    outputFile.WriteLine(a.url);
            }*/

            int[,] WeightedAdjacencyMatrix = new int[FoundAuthors.Count, FoundAuthors.Count];

            foreach (Author author in FoundAuthors)
            {
                System.Threading.Thread.Sleep(1000);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(coauthors_url(author.url));
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    html = reader.ReadToEnd();
                }

                MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(html));
                XmlSerializer serializer = new XmlSerializer(typeof(Coauthors));
                Coauthors parsedData = (Coauthors)serializer.Deserialize(memStream);
                if (parsedData.AuthorsList.Count > 0)
                {

                    foreach(Author coauthor in parsedData.AuthorsList)
                    {
                        if( DictAuthor.ContainsKey(coauthor.url))
                        {
                            Console.WriteLine(author.Name + " - " + NameAuthor[DictAuthor[coauthor.url]] + " " + coauthor.count);
                            WeightedAdjacencyMatrix[author.Id, DictAuthor[coauthor.url]] = coauthor.count;
                        }
                    }
                }
            }

            GmlExporter exporter = new GmlExporter();
            exporter.Export("stanford.gml", FoundAuthors, WeightedAdjacencyMatrix);
            Console.WriteLine("DONE");     
        }

        static void Run1(){ 
            Console.Write("hello \n");

            string authorstxt = @"C:\Users\jakub\Documents\Visual Studio 2017\dblp-reducer\data\Authors_short.txt";
            string authorsxml = @"C:\dblp_result\636733187090414687\Authors.xml";
            string publicationsxml = @"C:\dblp_result\636733187090414687\Publications.xml";

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
