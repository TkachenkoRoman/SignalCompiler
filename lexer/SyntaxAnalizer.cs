using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer
{
    class SyntaxAnalizer
    {
        public SyntaxAnalizer(List<LexicalAnalizerOutput> lexems, List<Constant> constants, List<Identifier> identifiers, List<KeyWord> keyWords)
        {
            errors = new List<Error>();
            this.lexems = lexems;
            this.constants = constants;
            this.identifiers = identifiers;
            this.keyWords = keyWords;
            program = new SyntaxTree.Node("program");
            positionInLexems = -1;
        }

        private List<Error> errors;
        private List<LexicalAnalizerOutput> lexems;
        private List<Constant> constants;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private SyntaxTree.Node program;
        private int positionInLexems; // current pos in lexems

        public delegate void WorkDoneHandler(List<Error> errors);
        public event WorkDoneHandler WorkDone;

        private LexicalAnalizerOutput GetNextToken()
        {
            positionInLexems++;
            return lexems[positionInLexems];
        }

        private bool ParseProgram()
        {
            LexicalAnalizerOutput currentToken = GetNextToken();

            if (currentToken.lexem == "PROGRAM")
            {
                if (ParseProcedureIdn())
                {
                    currentToken = GetNextToken();
                    if (currentToken.lexem == ";")
                    {
                        if (!ParseBlock())
                            return false;
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** ';' expected", row = currentToken.row });
                    }

                    currentToken = GetNextToken();
                    if (currentToken.lexem != ".")
                    {
                        errors.Add(new Error { message = "**Error** '.' expected", row = currentToken.row });
                    }
                }
                else
                {
                    errors.Add(new Error { message = "**Error** Identifier expected", row = currentToken.row });
                    return false;
                }
                return true;
            }
            else
                errors.Add(new Error { message = "**Error** PROGRAM expected", row = currentToken.row});
            return false;
        }

        private bool ParseProcedureIdn()
        {
            string identifier = ParseIdentifier();
            if (identifier != "")
            {
                program.AddNode(new SyntaxTree.Node() { name = "procedure_idn", value = identifier });
                return true;
            }
            else
            {
                return false;
            }
        }

        private string ParseIdentifier() // return empty string if not parsed else return value
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (identifiers.Find(x => x.id == currentToken.code) != null)
                return currentToken.lexem;
            else return "";
        }

        private bool ParseBlock()
        {
            program.AddNode(new SyntaxTree.Node() { name = "block" });

            if (parseVarDeclarations())
            {
                // continue parsing BEGIN
            }
                
            return true;

        }

        private bool parseVarDeclarations()
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (currentToken.lexem == "VAR" && keyWords.Find(x => x.id == currentToken.code) != null)
            {
                if (program.nodes.Find(x => x.name == "block") != null)
                {
                    program.nodes.Find(x => x.name == "block")
                                 .AddNode(new SyntaxTree.Node() { name = "var_declar" });
                }
                
                program.nodes.Find(x => x.name == "block")
                       .nodes.Find(x => x.name == "var_declar")
                       .AddNode(new SyntaxTree.Node() { name = "declar_list" });

                if (parseDeclarationList())
                {
                    return true;
                }
                else
                {
                    errors.Add(new Error { message = "**Error** No declarations found", row = currentToken.row });
                    return false;
                }
            }   
            return true;    
        }

        private bool parseDeclarationList()
        {
            if (parseDeclaration())
                parseDeclarationList();
            if (program.nodes.Find(x => x.name == "block")
                       .nodes.Find(x => x.name == "var_declar")
                       .nodes.Find(x => x.name == "declar_list")
                       .nodes.Count > 0)
                return true;
            else // no declarations found
            {
                program.nodes.Find(x => x.name == "block")
                       .nodes.Find(x => x.name == "var_declar")
                       .nodes.Clear();
                return false;
            }
        }

        private bool parseDeclaration()
        {
            LexicalAnalizerOutput currentToken = new LexicalAnalizerOutput();
            string expectedDeclarationIdentifier = ParseVarIdn();
            if (expectedDeclarationIdentifier != "")//identifiers.Find(x => x.type == identifierType.user && x.id == currentToken.code) != null)
            {
                currentToken = GetNextToken();
                if (currentToken.lexem == ":")
                {
                    string expectedDeclarationType = ParseAttribute();
                    if (expectedDeclarationType != "")
                    {
                        currentToken = GetNextToken();
                        if (currentToken.lexem == ";")
                        {
                            program.nodes.Find(x => x.name == "block")
                                   .nodes.Find(x => x.name == "var_declar")
                                   .nodes.Find(x => x.name == "declar_list")
                                   .nodes.Find(x => x.name == "declaration" && x.nodes.Find(y => y.name == "var_idn" && y.value == expectedDeclarationIdentifier) != null)
                                   .AddNode(new SyntaxTree.Node() { name = "attribute", value = expectedDeclarationType });

                            return true;
                        }
                        else 
                            errors.Add(new Error { message = "**Error** ';' expected", row = currentToken.row });
                    }
                    else
                        errors.Add(new Error { message = "**Error** Type expected", row = currentToken.row });
                }
                else
                {
                    errors.Add(new Error { message = "**Error** ':' expected", row = currentToken.row });
                }
            }
            return false;
        }

        private string ParseAttribute()
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (identifiers.Find(x => x.type == identifierType.system && x.id == currentToken.code) != null)
                return currentToken.lexem;
            else return "";
        }

        private string ParseVarIdn()
        {
            string identifier = ParseIdentifier();
            if (identifier != "")
            {
                program.nodes.Find(x => x.name == "block")
                       .nodes.Find(x => x.name == "var_declar")
                       .nodes.Find(x => x.name == "declar_list")
                       .AddNode(new SyntaxTree.Node() { name = "declaration" })
                       .AddNode(new SyntaxTree.Node() { name = "var_idn", value = identifier});
                return identifier;
            }
            else
            {
                return "";
            }
        }

        public void Analize()
        {
            ParseProgram();
            SerializeTables.SeriaizeNode(program);
            if (WorkDone != null) WorkDone(errors);
        }
    }
}
