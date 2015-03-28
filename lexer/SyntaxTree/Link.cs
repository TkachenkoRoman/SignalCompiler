using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lexer.SyntaxTree
{
    public class Link
    {
        [XmlAttribute]
        public nodesTypes Source;
        [XmlAttribute]
        public nodesTypes Target;
        [XmlAttribute]
        public string Label;

        public Link(nodesTypes source, nodesTypes target, string label)
        {
            this.Source = source;
            this.Target = target;
            this.Label = label;
        }
        public Link()
        {
        }
    }
}
