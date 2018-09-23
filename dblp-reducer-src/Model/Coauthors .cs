using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dblp_reducer_src.Model
{
    [XmlRoot("coauthors")]
    public class Coauthors
    {
        [XmlAttribute("author", Namespace = "")]
        public string author { get; set; }

        [XmlAttribute("urlpt", Namespace = "")]
        public string url { get; set; }

        [XmlElement("author")]
        public List<Author> AuthorsList { get; set; }
    }
}
