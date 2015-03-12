using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace lexer
{
    enum attrType { whitespace, constant, identifier, oneSymbDelimiter, manySymbDelimiter, begCom, invalid };
    class LexicalAnalizer
    {
        public LexicalAnalizer()
        {
            attributes = SerializeTables.DeserializeAttributes();
            identifiers = SerializeTables.DeserializeIdentifiers();
            keyWords = SerializeTables.DeserializeKeyWords();
            constants = new List<Constant>();
            errors = new List<Error>();
        }

        private List<Attributes> attributes;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private List<Constant> constants;

        public static string commentSymbol = "*";
        public static string endCom = ")";
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

        public List<Error> errors;
        public List<LexicalAnalizerOutput> Analize(string filepath)
        {
            List<LexicalAnalizerOutput> result = new List<LexicalAnalizerOutput>();
            string[] lines;

            if (File.Exists(filepath))
            {
                lines = File.ReadAllLines(filepath);
            }
            else
            {
                throw new FileNotFoundException();
            }

            int i = 0; // row number
            int j = 0; // symbol number
            bool supressedOutput = false; // true if whitespaced
            string buffer = "";
            int lexCode = 0;
            string currLexem = ""; // used only for logging

            if (attributes.Count != 0)
            {
                for (i = 0; i < lines.Count(); i++) 
                {
                    string currentLine = lines[i];
                    j = 0;

                    while (j < currentLine.Length)
                    {
                        supressedOutput = false;
                        char currentSymbol = currentLine[j];
                        int symbolAttr = GetSymbolAttr(currentSymbol);

                        if (symbolAttr == attributesTypes[attrType.whitespace]) // whitespace
                        {
                            while (++j < currentLine.Length)
                            {
                                currentSymbol = currentLine[j];
                                symbolAttr = GetSymbolAttr(currentSymbol);
                                if (symbolAttr != attributesTypes[attrType.whitespace])
                                    break;
                            }
                            supressedOutput = true;
                        }
                        else if (symbolAttr == attributesTypes[attrType.constant]) // constant
                        {
                            buffer = makeBuffer(currentLine, attrType.constant, ref j);
                            currLexem = buffer;
                            lexCode = CheckConst(buffer);
                        }
                        else if (symbolAttr == attributesTypes[attrType.identifier]) // identifier
                        {
                            buffer = makeBuffer(currentLine, attrType.identifier, ref j);
                            currLexem = buffer;
                            lexCode = CheckIdentifier(buffer);
                        }
                        else if (symbolAttr == attributesTypes[attrType.oneSymbDelimiter]) // divider
                        {
                            lexCode = (int)currentSymbol;
                            currLexem = currentSymbol.ToString();
                            j++;
                        }
                        else if (symbolAttr == attributesTypes[attrType.begCom]) // Comment
                        {
                            SkipComment(lines, ref i, ref j);
                            supressedOutput = true;
                        }
                        else
                        {
                            j++;
                            errors.Add(new Error { message = "**Error** Invalid symbol", row = i, pos = j});
                        }
                        if (!supressedOutput)
                        {
                            result.Add(new LexicalAnalizerOutput { code = lexCode, lexem = currLexem });
                        }
                    }

                }
            }

            return result;
        }

        private void SkipComment(string[] lines, ref int i, ref int j)
        {
            int entry_i = i;
            int entry_j = j;
            string currentLine = lines[i];
            char currentSymbol = currentLine[j];
            bool commentSkipped = false;
            j++;

            if (j < currentLine.Length)
            {
                currentSymbol = currentLine[j];
                entry_j = j;
            }
            else // error ??      
            {
                errors.Add(new Error { message = "**Error** BegCom symbol without '*'", row = i, pos = j });
                j++; // Analize method will iterate to next row
                //entry_i = i;
                //entry_j = j;
                return;
            }

            if (currentSymbol == (char)commentSymbol[0]) // if (*
            {
                for (int k = i; k < lines.Count(); k++)
                {
                    currentLine = lines[k];
                    while (++j < currentLine.Length - 1)
                    {
                        currentSymbol = currentLine[j];
                        char nextSymbol = currentLine[j + 1];
                        if (currentSymbol == commentSymbol[0] && nextSymbol == endCom[0]) // end of Comment found
                        {
                            i = k;
                            j += 2; // skip "*)"
                            return;
                        }
                    }
                    j = 0;
                }
                // ERROR end of comment not found
                i = entry_i; // skip begCom and continue parsing
                j = entry_j;
                errors.Add(new Error { message = "**Error** End of comment not found", row = i, pos = j });
                return;
            }
            else
            {
                // ERROR ("Do u mean comment? '*' missing")
                errors.Add(new Error { message = "**Error** Do u mean comment? '*' missing", row = i, pos = j });
            }
        }

        private string makeBuffer(string currentLine, attrType type, ref int j) // makes buffer for constant or identifier 
        {
            string buffer = "";
            char currentSymbol = currentLine[j];
            buffer += currentSymbol.ToString();
            while (++j < currentLine.Length)
            {
                currentSymbol = currentLine[j];
                int symbolAttr = GetSymbolAttr(currentSymbol);
                if (symbolAttr == attributesTypes[type])
                    buffer += currentSymbol.ToString();
                else break;
            }
            return buffer;
        }

        private int CheckIdentifier(string buffer) // returns lexCode of identifier in buffer
        {
            int lexCode = 0;

            if (keyWords.Count() != 0)
            {
                if (keyWords.Any(x => x.keyWord == buffer))
                {
                    lexCode = keyWords.First(x => x.keyWord == buffer).id;
                    return lexCode;
                }
            }
            if (identifiers.Count() != 0)
            {
                if (identifiers.Any(x => x.name == buffer))
                {
                    lexCode = identifiers.First(x => x.name == buffer).id;
                    return lexCode;
                }
                else
                {
                    // creates new identifier with id = maxId + 1
                    Identifier identifier = new Identifier(buffer, identifierType.user, identifiers.OrderByDescending(x => x.id).First().id + 1);
                    identifiers.Add(identifier);
                    lexCode = identifier.id;
                    return lexCode;
                }
            }
            else
            {
                Identifier identifier = new Identifier(buffer, identifierType.user);
                identifiers.Add(identifier);
                lexCode = identifier.id;
                return lexCode;
            }
        }

        private int CheckConst(string buffer) // returns lexCode, if not present in constants returns new id
        {
            int lexCode = 0;
            if (constants.Count() == 0)
            {
                Constant constant = new Constant(Convert.ToInt32(buffer));
                constants.Add(constant);
                lexCode = constant.id;
            }
            else
            {
                if (!constants.Any(x => x.value == Convert.ToInt32(buffer))) // if no consts has the same value
                {
                    // creates new const with id = maxId + 1
                    Constant constant = new Constant(Convert.ToInt32(buffer), constants.OrderByDescending(x => x.id).First().id + 1);
                    constants.Add(constant);
                    lexCode = constant.id;
                }
                else lexCode = constants.First(x => x.value == Convert.ToInt32(buffer)).id; // if exists get id
            }
            return lexCode;
        }

        private int GetSymbolAttr(char symbol)
        {
            if (attributes.Count != 0)
                return attributes.First(x => x.symbol == symbol).type;
            else
                return attributesTypes[attrType.invalid]; 
        }
    }
}

struct LexicalAnalizerOutput
{
    public int code;
    public string lexem;
}