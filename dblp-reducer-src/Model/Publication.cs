using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace dblp_reducer_src.Model
{
    [XmlRoot("dblp")]
    [Serializable]  
    public class Publication
    {
        [XmlElement("article")]
        public Publication Article { get; set; }
        [XmlElement("id")]
        public int Id { get; set; }
        [XmlElement("title")]
        public string Title { get; set; }
        [XmlElement("journalId")]
        public int JournalId { get; set; }
        [XmlElement("ee")]
        public string ee { get; set; }
        [XmlElement("month")]
        public int month { get; set; }
        [XmlElement("year")]
        public int Year { get; set; }
        [XmlElement("timeId")]
        public int TimeId { get; set; }
        [XmlElement("est")]
        public string est { get; set; }
        [XmlElement("author")]
        public List<Author> AuthorsList { get; set; }
    }
}
