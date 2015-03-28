using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer
{
    public enum nodesTypes
    {
        node,
        token,
        program,
        procedure_idn,
        block,
        var_declar,
        declar_list,
        declaration,
        attribute,
        var_idn,
        statement_list,
        statement,
        conditional_statement,
        incomplete_conditional_statement,
        alternative_part,
        conditional_expression,
        expression
    }
    class SyntaxAnalizer
    {
        public SyntaxAnalizer(List<LexicalAnalizerOutput> lexems, List<Constant> constants, List<Identifier> identifiers, List<KeyWord> keyWords)
        {
            errors = new List<Error>();
            this.lexems = lexems;
            this.constants = constants;
            this.identifiers = identifiers;
            this.identifiersExtended = new List<IdentifierExt>();
            this.keyWords = keyWords;
            graphNodes = new List<SyntaxTree.GraphNode>();
            links = new List<SyntaxTree.Link>();

            program = new SyntaxTree.Node(nodesTypes.program);
            graphNodes.Add(new SyntaxTree.GraphNode(nodesTypes.program, "empty"));
            positionInLexems = -1;

            
        }

        private List<Error> errors;
        private List<LexicalAnalizerOutput> lexems;
        private List<Constant> constants;
        private List<Identifier> identifiers;
        private List<KeyWord> keyWords;
        private SyntaxTree.Node program;
        private int positionInLexems; // current pos in lexems
        private List<IdentifierExt> identifiersExtended;

        public delegate void WorkDoneHandler(List<Error> errors);
        public event WorkDoneHandler WorkDone;

        private List<SyntaxTree.GraphNode> graphNodes;
        private List<SyntaxTree.Link> links;

        private void addGraphNode(SyntaxTree.GraphNode g)
        {
            graphNodes.Add(g);
        }

        private void addLink(SyntaxTree.Link l)
        {
            links.Add(l);
        }

        private LexicalAnalizerOutput GetNextToken()
        {
            positionInLexems++;
            if (positionInLexems < lexems.Count)
                return lexems[positionInLexems];
            else return new LexicalAnalizerOutput() { code = -1, row = -1, lexem = ""}; // end of program
        }

        private bool ParseProgram()
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            SyntaxTree.Node currentNode = program;

            if (currentToken.lexem == "PROGRAM")
            {
                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem});

                addGraphNode(new SyntaxTree.GraphNode(nodesTypes.token, currentToken.lexem));
                addLink(new SyntaxTree.Link(nodesTypes.program, nodesTypes.token, ""));

                if (ParseProcedureIdn())
                {
                    currentToken = GetNextToken();
                    if (currentToken.lexem == ";")
                    {
                        currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                        if (!ParseBlock())
                            return false;
                    }
                    else
                    {
                        errors.Add(new Error { message = "**Error** ';' expected", row = currentToken.row });
                    }

                    currentToken = GetNextToken();
                    if (currentToken.lexem == ".")
                    {
                        currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                        currentToken = GetNextToken();
                        if (currentToken.code != -1) // if any lexems exists
                            errors.Add(new Error { message = "**Error** Expected end of program", row = currentToken.row });
                    }
                    else
                        errors.Add(new Error { message = "**Error** '.' expected", row = currentToken.row });
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
            LexicalAnalizerOutput identifier = ParseIdentifier();
            if (identifier.lexem != "")
            {
                program.AddNode(new SyntaxTree.Node() { name = nodesTypes.procedure_idn, value = identifier.lexem });
                return true;
            }
            else
            {
                errors.Add(new Error { message = "**Error** Expected user identifier", row = identifier.row });
                return false;
            }
        }

        private LexicalAnalizerOutput ParseIdentifier() // return empty string if not parsed else return value
        {
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (identifiers.Find(x => x.id == currentToken.code && x.type != identifierType.system) != null)
                return currentToken;
            else
            {
                //errors.Add(new Error { message = "**Error** Expected user identifier", row = currentToken.row });
                return new LexicalAnalizerOutput() { lexem = "" };
            }
        }

        private bool ParseBlock()
        {
            SyntaxTree.Node currentNode = program.AddNode(new SyntaxTree.Node() { name = nodesTypes.block });


            if (parseVarDeclarations())
            {
                // continue parsing BEGIN
                // decrement positionInLexems cause while parsing VAR block it stops on first not "identifier"
                //
                positionInLexems--;
                LexicalAnalizerOutput currentToken = GetNextToken(); //BEGIN expected
                if (currentToken.lexem == "BEGIN")
                {
                    currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                    // continue parsing statementList
                    if (parseStatementList(program.nodes.Find(x => x.name == nodesTypes.block)))
                    {
                        currentToken = GetNextToken();
                        if (currentToken.lexem == "END")
                        {
                            currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                            return true;
                        }
                        else
                        {
                            errors.Add(new Error { message = "**Error** END or statement expected", row = currentToken.row });
                        }
                    }
                    else
                        return false;
                }
                else
                {
                    errors.Add(new Error { message = "**Error** BEGIN expected", row = currentToken.row });
                }
            }
                
            return false;

        }

        private bool parseStatementList(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.statement_list });

            if (parseStatement(currentNode))
                parseStatementList(curr);
            else
            {
                positionInLexems--;
                curr.nodes.Remove(currentNode);
            }
                

            //positionInLexems--;  
            return true;
        }

        private bool parseStatement(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.statement });
            if (parseConditionStatement(currentNode))
            {
                // if parse ENDIF;
                
                LexicalAnalizerOutput currentToken = GetNextToken();
                if (currentToken.lexem == "ENDIF")
                {
                    currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                    currentToken = GetNextToken();
                    if (currentToken.lexem == ";")
                    {
                        currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                        return true;
                    }
                    else
                        errors.Add(new Error { message = "**Error** Expected ';'", row = currentToken.row });
                }
                else
                    errors.Add(new Error { message = "**Error** Expected 'ENDIF'", row = currentToken.row });
            }
            return false;
        }

        private bool parseConditionStatement(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.conditional_statement});
            if (parseIncompleteConditionStatement(currentNode))
            {
                if (parseAlternativePart(currentNode))
                    return true;
            }
            return false;
        }

        private bool parseIncompleteConditionStatement(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.incomplete_conditional_statement });
            // IF conditionalExpression THEN statement_list
            LexicalAnalizerOutput currentToken = GetNextToken();

            if (currentToken.lexem == "IF")
            {
                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                if (parseconditionalExpression(currentNode))
                {
                    currentToken = GetNextToken();
                    if (currentToken.lexem == "THEN")
                    {
                        currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                        if (parseStatementList(currentNode))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    //errors.Add(new Error { message = "**Error** conditional statement expected ", row = currentToken.row });
                    return false;
                }
            }

            return false;
        }

        private bool parseconditionalExpression(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.conditional_expression });
            if (parseExpression(currentNode))
            {
                LexicalAnalizerOutput currentToken = GetNextToken();
                if (currentToken.lexem == "=")
                {
                    currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                    if (parseExpression(currentNode))
                        return true;
                }
                else
                    errors.Add(new Error { message = "**Error** Expected '=' ", row = currentToken.row });
            }
            return false;
        }

        private bool parseExpression(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.expression });
            LexicalAnalizerOutput currentToken = GetNextToken();

            if (constants.Find(x => x.id == currentToken.code) != null)
            {
                currentNode.value = currentToken.lexem;
                return true;
            }
                

            if (identifiersExtended.Find(x => x.name == currentToken.lexem) != null)
            {
                currentNode.value = currentToken.lexem;
                return true;
            }
            else
                errors.Add(new Error { message = "**Error** Undeclared identifier", row = currentToken.row });
            
            errors.Add(new Error { message = "**Error** Expected identifier or constant", row = currentToken.row });
            return false;   
        }

        private bool parseAlternativePart(SyntaxTree.Node curr)
        {
            SyntaxTree.Node currentNode = curr.AddNode(new SyntaxTree.Node() { name = nodesTypes.alternative_part });
            LexicalAnalizerOutput currentToken = GetNextToken();
            if (currentToken.lexem == "ELSE")
            {
                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                if (parseStatementList(currentNode))
                    return true;
                else
                {
                    errors.Add(new Error { message = "**Error** Expected statement", row = currentToken.row });
                }
            }
            positionInLexems--; // no alternative part
            curr.nodes.Remove(currentNode);
            return true;
        }

        private bool parseVarDeclarations()
        {
            SyntaxTree.Node currentNode = program.nodes.Find(x => x.name == nodesTypes.block).AddNode(new SyntaxTree.Node() { name = nodesTypes.var_declar });

            LexicalAnalizerOutput currentToken = GetNextToken();
            if (currentToken.lexem == "VAR" && keyWords.Find(x => x.id == currentToken.code) != null)
            {
                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });

                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.declar_list });

                if (parseDeclarationList())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }   
            return true;    
        }

        private bool parseDeclarationList()
        {
            SyntaxTree.Node currentNode = program.nodes.Find(x => x.name == nodesTypes.block)
                                                 .nodes.Find(x => x.name == nodesTypes.var_declar);

            if (parseDeclaration())
                parseDeclarationList();
            if (currentNode.nodes.Find(x => x.name == nodesTypes.declar_list)
                           .nodes.Count > 0)
                return true;
            else // no declarations found
            {
                currentNode.nodes.Clear();
                return false;
            }
        }

        private bool parseDeclaration()
        {
            LexicalAnalizerOutput currentToken = new LexicalAnalizerOutput();
            SyntaxTree.Node currentNode = ParseVarIdn();
            if (currentNode != null)//identifiers.Find(x => x.type == identifierType.user && x.id == currentToken.code) != null)
            {
                currentToken = GetNextToken();
                if (currentToken.lexem == ":")
                {
                    currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
                    string expectedDeclarationType = ParseAttribute();
                    if (expectedDeclarationType != "")
                    {
                        currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.attribute, value = expectedDeclarationType });
                        if (!identifiersExtended.Exists(x => x.name == currentNode.nodes.Find(y => y.name == nodesTypes.var_idn).value))
                        {
                            identifiersExtended.Add(new IdentifierExt()
                            {
                                name = currentNode.nodes.Find(x => x.name == nodesTypes.var_idn).value,
                                typeAttribute = expectedDeclarationType,
                                type = identifierType.user
                            });
                        }
                        else
                        {
                            errors.Add(new Error { message = "**Error** Variable is already declared ", row = currentToken.row });
                            return false;
                        }
                        currentToken = GetNextToken();
                        if (currentToken.lexem == ";")
                        {
                            currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.token, value = currentToken.lexem });
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

        private SyntaxTree.Node ParseVarIdn()
        {
            LexicalAnalizerOutput identifier = ParseIdentifier();
            if (identifier.lexem != "")
            {
                SyntaxTree.Node currentNode = program.nodes.Find(x => x.name == nodesTypes.block)
                                                 .nodes.Find(x => x.name == nodesTypes.var_declar)
                                                 .nodes.Find(x => x.name == nodesTypes.declar_list)
                                                 .AddNode(new SyntaxTree.Node() { name = nodesTypes.declaration });

                currentNode.AddNode(new SyntaxTree.Node() { name = nodesTypes.var_idn, value = identifier.lexem});
                return currentNode;
            }
            else
            {
                return null;
            }
        }

        public void Analize()
        {
            ParseProgram();
            SerializeTables.SeriaizeNode(program);
            SyntaxTree.Graph graph = new SyntaxTree.Graph();
            graph.Nodes = graphNodes.ToArray();
            graph.Links = links.ToArray();
            SerializeTables.SeriaizeNode(program);
            SerializeTables.SeriaizeNodeGraph(graph);
            if (WorkDone != null) WorkDone(errors);
        }
    }
}
