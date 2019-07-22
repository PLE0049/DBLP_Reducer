using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace dblp_reducer_src.Exporter
{
    public class GEXFExporter
    {
        bool TEMPORAL_EDGES = true;
        int EDGE_LIFESPAN = 0;

        Dictionary<string, int> NameAuthor;
        Dictionary<int, Dictionary<Tuple<int, int>, int>> TemporalNet;
        Dictionary<Tuple<int, int>, int> UniqueEdges;
        Dictionary<int, int> FirstPersongAppearance;
        const string NS_VIZ = "http:///www.gexf.net/1.2draft/viz";

        public bool Export(string fileName, Dictionary<string, int> nameAuthor, Dictionary<int,int> firstPersongAppearance,  Dictionary<Tuple<int, int>, int> uniqueEdges, Dictionary<int, Dictionary<Tuple<int, int>, int>> temporalNet)
        {
            NameAuthor = nameAuthor;
            TemporalNet = temporalNet;
            UniqueEdges = uniqueEdges;
            FirstPersongAppearance = firstPersongAppearance;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.ConformanceLevel = ConformanceLevel.Auto;

            XmlWriter xmlWriter = XmlWriter.Create(fileName, settings);

            xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
            WriteHeader(xmlWriter);
                xmlWriter.WriteStartElement("attributes");
                    xmlWriter.WriteAttributeString("class", "edge");
                    xmlWriter.WriteAttributeString("mode", "dynamic");
                    xmlWriter.WriteStartElement("attribute");
                    xmlWriter.WriteAttributeString("id", "weight");
                    xmlWriter.WriteAttributeString("title", "weight");
                    xmlWriter.WriteAttributeString("type", "float");
                xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            xmlWriter.WriteStartElement("attributes");
                    xmlWriter.WriteAttributeString("class", "node");
                    xmlWriter.WriteStartElement("attribute");
                    xmlWriter.WriteAttributeString("id", "school");
                    xmlWriter.WriteAttributeString("title", "school");
                    xmlWriter.WriteAttributeString("type", "string");
                xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("graph");
                    xmlWriter.WriteAttributeString("mode", "dynamic");
                    xmlWriter.WriteAttributeString("defaultedgetype", "undirected");
                    xmlWriter.WriteAttributeString("timeformat", "date");
                    WriteNodes(xmlWriter); // Nodes

                    if(TEMPORAL_EDGES)
                    {
                        WriteEdgesTemporal(xmlWriter);  // Temporal Edges
                    } 
                    else
                    {
                        WriteEdge(xmlWriter);  // Edges
                    }              
                xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
            //xmlWriter.WriteEndDocument();
            xmlWriter.Close();
            return true;
        }

        private void WriteEdge(XmlWriter xmlWriter)
        {
            bool isFirstAppearance;
            int las_year;
            xmlWriter.WriteStartElement("Edges");

            foreach (KeyValuePair<Tuple<int, int>, int> uniqueEdge in UniqueEdges)
            {
                isFirstAppearance = true;
                xmlWriter.WriteStartElement("Edge");
                xmlWriter.WriteAttributeString("source", uniqueEdge.Key.Item1.ToString());
                xmlWriter.WriteAttributeString("target", uniqueEdge.Key.Item2.ToString());
                // for each year

                int Weight = 0;
                for (int year = 1900; year < 2020; year++)
                {
                    if (TemporalNet.ContainsKey(year))
                    {
                        Dictionary<Tuple<int, int>, int> edges = TemporalNet[year];


                        if (edges.ContainsKey(uniqueEdge.Key) && isFirstAppearance)
                        {
                            Weight += TemporalNet[year][uniqueEdge.Key];

                            xmlWriter.WriteAttributeString("start", year.ToString());
                            xmlWriter.WriteStartElement("attvalues");
                            xmlWriter.WriteStartElement("attvalue");
                            xmlWriter.WriteAttributeString("for", "weight");
                            xmlWriter.WriteAttributeString("value", Math.Ceiling((double)Weight / 2).ToString());
                            xmlWriter.WriteAttributeString("start", year.ToString());
                            isFirstAppearance = false;
                        }

                        if (edges.ContainsKey(uniqueEdge.Key) && !isFirstAppearance)
                        {
                            Weight += TemporalNet[year][uniqueEdge.Key];

                            xmlWriter.WriteAttributeString("end", year.ToString());
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteStartElement("attvalue");
                            xmlWriter.WriteAttributeString("for", "weight");
                            xmlWriter.WriteAttributeString("value", Math.Ceiling((double)Weight / 2).ToString());
                            xmlWriter.WriteAttributeString("start", year.ToString());
                        }
                    }
                }
                xmlWriter.WriteAttributeString("end", "2019");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        private void WriteEdgesTemporal(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Edges");

            foreach (KeyValuePair<Tuple<int, int>, int> uniqueEdge in UniqueEdges)
            {
                int Weight = 0;

                for (int year = 1900; year < 2020; year++)
                {
                    if (TemporalNet.ContainsKey(year))
                    {
                        Dictionary<Tuple<int, int>, int> edges = TemporalNet[year];


                        if (edges.ContainsKey(uniqueEdge.Key))
                        {
                            xmlWriter.WriteStartElement("Edge");
                            xmlWriter.WriteAttributeString("source", uniqueEdge.Key.Item1.ToString());
                            xmlWriter.WriteAttributeString("target", uniqueEdge.Key.Item2.ToString());
                            Weight = TemporalNet[year][uniqueEdge.Key];
                            xmlWriter.WriteAttributeString("start", year.ToString());
                            xmlWriter.WriteAttributeString("end", (year + EDGE_LIFESPAN).ToString());
                            xmlWriter.WriteStartElement("attvalues");
                            xmlWriter.WriteStartElement("attvalue");
                            xmlWriter.WriteAttributeString("for", "weight");
                            xmlWriter.WriteAttributeString("value", Math.Ceiling((double)Weight / 2).ToString());
                            xmlWriter.WriteAttributeString("start", year.ToString());
                            xmlWriter.WriteAttributeString("end", (year + EDGE_LIFESPAN).ToString());
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement();
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
            }
            xmlWriter.WriteEndElement();
        }

        private void WriteNodes(XmlWriter xmlWriter)
        {
            string school = "Berkeley";
            xmlWriter.WriteStartElement("Nodes");
            foreach ( KeyValuePair<string,int> kvp in this.NameAuthor)
            {
                if (kvp.Key == "-B")
                    continue;
                if (kvp.Key == "-S")
                {
                    school = "Stanford";
                    continue;
                }

                // One node
                if (FirstPersongAppearance.ContainsKey(kvp.Value))
                {
                    xmlWriter.WriteStartElement("Node");
                    xmlWriter.WriteAttributeString("id", kvp.Value.ToString());
                    xmlWriter.WriteAttributeString("label", kvp.Key);                  
                    xmlWriter.WriteAttributeString("start", FirstPersongAppearance[kvp.Value].ToString());

                    WriteVizualisations(xmlWriter, school);

                        xmlWriter.WriteStartElement("attvalues");
                            xmlWriter.WriteStartElement("attvalue");
                                xmlWriter.WriteAttributeString("for", "school");
                                xmlWriter.WriteAttributeString("value", school);
                            xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
            }
            xmlWriter.WriteEndElement();
        }

        private void WriteVizualisations(XmlWriter xmlWriter, string school)
        {
            xmlWriter.WriteStartElement("viz","color", NS_VIZ);
            if (school == "Stanford")
            {
                xmlWriter.WriteAttributeString("r", "239");
                xmlWriter.WriteAttributeString("g", "173");
                xmlWriter.WriteAttributeString("b", "66");
            }
            else
            {
                xmlWriter.WriteAttributeString("r", "50");
                xmlWriter.WriteAttributeString("g", "50");
                xmlWriter.WriteAttributeString("b", "220");
            }
            xmlWriter.WriteEndElement();
            /*      
             < viz:color r = "239" g = "173" b = "66" a = "0.6" />
             < viz:position x = "15.783598" y = "40.109245" z = "0.0" />
             < viz:size value = "2.0375757" />
             < viz:shape value = "disc" />*/
        }

        private void WriteHeader(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("gexf");
            xmlWriter.WriteAttributeString("version", "1.2");
        }
    }
}
