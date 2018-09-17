using dblp_reducer_src.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace dblp_reducer_src.Loader
{
    public class XmlLoader
    {
        public Authors LoadAuthors( string fileName )
        {
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

            return parsedData;
        }

        public Publications LoadPublications( string fileName)
        {
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

            return publications;
        }
    }
}
