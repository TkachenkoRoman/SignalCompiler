using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.SyntaxTree
{
    class ProgramBranch
    {
        public ProgramBranch()
        {
            progName = "";
        }
        public string progName;
        public BlockBranch block;
    }
}
