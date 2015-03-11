using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace lexer
{
    class LexicalAnalizer
    {
        public LexicalAnalizer()
        {
            attributes = SerializeTables.DeserializeAttributes();
            identifiers = SerializeTables.DeserializeIdentifiers();
            keyWords = SerializeTables.DeserializeKeyWords();
            constants = new List<Constant>();
        }

        private List<Attributes> attributes;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private List<Constant> constants;

        public List<int> Analize(string filepath)
        {
            List<int> result = new List<int>();
            string[] lines;

            if (File.Exists(filepath))
            {
                lines = File.ReadAllLines(filepath);
            }
            else
            {
                throw new FileNotFoundException();
            }

            for

            return result;
        }
    }
}
