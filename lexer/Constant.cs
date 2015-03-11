using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer
{
    public class Constant
    {
        public Constant()
        {
            value = 0;
            id = 501;
        }
        public Constant(int value, int id)
        {
            this.value = value;
            this.id = id;
        }
        public int value;
        public int id;
    }
}
