using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dblp_reducer_src.Model
{ 
    [XmlRoot("dblpperson")]
    public class Dblpperson
    {
        [XmlElement("dblpkey")]
        public List<string> dblpkeys { get; set; }
    }
}
