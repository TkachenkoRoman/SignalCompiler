using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.SyntaxTree
{
    class XMLNodeToGLEE
    {
        public XMLNodeToGLEE()
        {
            XMLSyntaxTree = SerializeTables.DeseriaizeNode();
            nodes = new List<Node>();
            links = new List<Link>();
            graph = new Microsoft.Glee.Drawing.Graph("graph");
            graph.AddNode(XMLSyntaxTree.Id);
            graph.FindNode(XMLSyntaxTree.Id).Attr.Label = XMLSyntaxTree.name.ToString();
            graph.FindNode(XMLSyntaxTree.Id).Attr.Fillcolor = Microsoft.Glee.Drawing.Color.Salmon;
        }
        private XMLNode XMLSyntaxTree;
        private List<Node> nodes;
        private List<Link> links;
        Microsoft.Glee.Drawing.Graph graph;
        private void ParseNode(XMLNode parentNode)
        {
            foreach (var item in parentNode.nodes)
            {
                string label = item.name.ToString();
                if (item.value != "")
                {
                    label += ": ";
                    label += item.value;
                }
                graph.AddNode(item.Id);
                Microsoft.Glee.Drawing.Node n = graph.FindNode(item.Id);
                n.Attr.Label = label;
                n.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightSkyBlue;

                Microsoft.Glee.Drawing.Edge edge = graph.AddEdge(parentNode.Id, item.Id);
                //edge.Attr.Weight = 0;
                
                //if (parentNode.nodes.Exists(x => Convert.ToInt32(x.Id) > Convert.ToInt32(item.Id)))
                //    graph.AddEdge(item.Id, parentNode.nodes.First(x => Convert.ToInt32(x.Id) > Convert.ToInt32(item.Id)).Id);
                
                //n.Attr.Shape = Microsoft.Glee.Drawing.Shape.DoubleCircle;
                
                ParseNode(item);
            }
        }
        public Microsoft.Glee.Drawing.Graph GetGraph()
        {
            ParseNode(XMLSyntaxTree);
            return graph;
        }
    }
}
