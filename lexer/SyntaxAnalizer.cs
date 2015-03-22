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
            program = new SyntaxTree.ProgramBranch();
            positionInLexems = -1;
        }

        private List<Error> errors;
        private List<LexicalAnalizerOutput> lexems;
        private List<Constant> constants;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private SyntaxTree.ProgramBranch program;
        private int positionInLexems; // current pos in lexems

        private LexicalAnalizerOutput getNextToken()
        {
            positionInLexems++;
            return lexems[positionInLexems];
        }

        private bool parseProgram()
        {
            LexicalAnalizerOutput currentToken = getNextToken();
            if (currentToken.lexem == "PROGRAM")
            {
                currentToken = getNextToken();

                if (identifiers.Find(x => x.id == currentToken.code) != null)
                {
                    program.progName = currentToken.lexem;

                    currentToken = getNextToken();
                    if (currentToken.lexem == ";")
                    {
                        if (!parseBlock())
                            return false;
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** ';' expected", row = currentToken.row });
                    }

                    currentToken = getNextToken();
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

        private bool parseBlock()
        {
            

            parseVarDeclarations();

            return true;

        }

        private bool parseVarDeclarations()
        {
            LexicalAnalizerOutput currentToken = getNextToken();
            if (currentToken.lexem == "VAR" && keyWords.Find(x => x.id == currentToken.code) != null)
                if (parseDeclarationList())
                {
                    return true;
                }
                else return false;
            return true;    
        }

        private bool parseDeclarationList()
        {
            if (parseDeclaration())
                parseDeclarationList();
            if (program.block.declarations.Count != 0)
                return true;
            else return false;
        }

        private bool parseDeclaration()
        {
            LexicalAnalizerOutput currentToken = getNextToken();
            LexicalAnalizerOutput expectedDeclarationIdentifier = currentToken;
            if (identifiers.Find(x => x.type == identifierType.user && x.id == currentToken.code) != null)
            {
                currentToken = getNextToken();
                if (currentToken.lexem == ":")
                {
                    currentToken = getNextToken();
                    if (identifiers.Find(x => x.type == identifierType.system) != null)
                    {
                        LexicalAnalizerOutput expectedDeclarationType = currentToken;
                        currentToken = getNextToken();
                        if (currentToken.lexem == ";")
                        {
                            program.block.declarations.Add(new SyntaxTree.Declaration() 
                                                                { identifier = expectedDeclarationIdentifier.lexem, 
                                                                  type = expectedDeclarationType.lexem });
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

        public void Analize()
        {
            parseProgram();
        }
    }
}
