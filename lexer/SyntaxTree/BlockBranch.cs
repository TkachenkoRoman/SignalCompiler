using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.SyntaxTree
{
    class BlockBranch
    {
        public BlockBranch()
        {
            declarations = new List<Declaration>();
            statements = new List<Statement>();
        }
        public List<Declaration> declarations;
        public List<Statement> statements;
    }
}
