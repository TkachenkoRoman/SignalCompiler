using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml.Serialization;
using System.IO;

namespace lexer
{
    enum attrType { whitespace, constant, identifier, oneSymbDelimiter, manySymbDelimiter, begCom, invalid };
    class SerializeTables
    {
        public static Dictionary<attrType, int> attributesTypes = new Dictionary<attrType, int>
        {
            { attrType.whitespace, 0 },
            { attrType.constant, 1},
            { attrType.identifier, 2},
            { attrType.oneSymbDelimiter, 3},
            { attrType.manySymbDelimiter, 4},
            { attrType.begCom, 5},
            { attrType.invalid, 6}
        };
        private static string path = System.IO.Directory.GetCurrentDirectory();
        private static string attributesTablePath = path + @"\AttributesTable.xml";
        private static void Serialize(object obj, string path)
        {
            Type objectType = obj.GetType();
            XmlSerializer writer = new XmlSerializer(objectType);
            StreamWriter file = new StreamWriter(path);
            writer.Serialize(file, obj);
            file.Close();
        }

        private static void Deserialize(ref object obj, string path)
        {
            Type objectType = obj.GetType();
            XmlSerializer reader = new XmlSerializer(objectType);
            StreamReader file = new StreamReader(path);
            obj = reader.Deserialize(file);
        }
        
        private static char[] GenerateCharArray(char start, char fin)
        {
            return Enumerable.Range(start, fin - start + 1).Select(x => (char)x).ToArray();
        }

        public static void SerializeAttributes()
        {
            char[] whitespaces = new char[]  // 0 in attributes
            {
                '\x20', // ascii code space
                '\xD', // carriage return
                '\xA', // line feed
                '\x9', // horizontal tab
                '\xB', // vertical tab 
                '\xC' // form feed
            };

            char[] constants = GenerateCharArray('0', '9'); // numbers 0..9

            char[] letters = GenerateCharArray('A', 'Z');
                
            List<Attributes> listAttributes = new List<Attributes>();
            for (int i = 0; i < 255; i++)
            {
                Attributes attributes = new Attributes();
                attributes.symbol = Convert.ToChar(i); // convert ascii to char

                if (whitespaces.Contains(attributes.symbol))
                    attributes.type = attributesTypes[attrType.whitespace];
                else if (constants.Contains(attributes.symbol))
                    attributes.type = attributesTypes[attrType.constant];
                else if (letters.Contains(attributes.symbol))
                    attributes.type = attributesTypes[attrType.identifier];
                else
                    attributes.type = attributesTypes[attrType.invalid];

                listAttributes.Add(attributes);
            }
            Serialize(listAttributes, attributesTablePath);
        }

        public static List<Attributes> DeserializeAttributes()
        {
            List<Attributes> listAttributes = new List<Attributes>();
            object obj = (object) listAttributes;
            Deserialize(ref obj, attributesTablePath);
            return (List<Attributes>) obj;
        }
    }
}
