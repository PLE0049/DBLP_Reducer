using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dblp_reducer_src.Model
{
    [XmlRoot("authors")]
    public class Authors
    {
        [XmlElement("author")]
        public List<Author> AuthorsList { get; set; }
    }
}
