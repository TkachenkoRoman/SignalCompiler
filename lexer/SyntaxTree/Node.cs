using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.SyntaxTree
{
    public class Node
    {
        public Node()
        {
            name = "";
            value = "empty";
            nodes = new List<Node>();
        }

        public Node(string name)
        {
            this.name = name;
            value = "empty";
            nodes = new List<Node>();
        }

        public string name;
        public string value;
        public List<Node> nodes;

        public Node AddNode(Node node)
        {
            nodes.Add(node);
            return node;
        }
    }
}
