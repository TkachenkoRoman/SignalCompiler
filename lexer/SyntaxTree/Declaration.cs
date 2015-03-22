using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.SyntaxTree
{
    class Declaration
    {
        public Declaration()
        {
            identifier = "";
            type = "";
        }
        public string identifier;
        public string type;
    }
}
