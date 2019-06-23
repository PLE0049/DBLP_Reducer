using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace dblp_reducer_src
{
    class DynamicXmlParser : DynamicObject
    {
        XElement element;

        public DynamicXmlParser(MemoryStream stream)
        {
            element = XElement.Load(stream);
        }

        private DynamicXmlParser(XElement el)
        {
            element = el;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (element == null)
            {
                result = null;
                return false;
            }

            XElement sub = element.Element(binder.Name);

            if (sub == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = new DynamicXmlParser(sub);
                return true;
            }

        }

        public override string ToString()
        {
            if (element != null)
            {
                return element.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        public string this[string attr]
        {        
            get
            {
                if (element == null)

                {
                    return string.Empty;
               }
                return element.Attribute(attr).Value;
            }
        }
    }
}
