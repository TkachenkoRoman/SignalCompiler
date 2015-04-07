using lexer.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lexer.AsmCodeGenerator
{
    class AssemblerCodeGenerator
    {
        public AssemblerCodeGenerator()
        {
            XMLSyntaxTree = SerializeTables.DeseriaizeNode();
            resultAsmCode = "";
            posInResultAsmCode = 0;
            dataSegmentPos = 0;
            codeSegmentPos = 0;
            labelNumber = 0;
        }
        private XMLNode XMLSyntaxTree;
        private string resultAsmCode;
        private int posInResultAsmCode;
        private int dataSegmentPos;
        private int codeSegmentPos;
        private static int labelNumber;

        public delegate void WorkDoneHandler(string output);
        public event WorkDoneHandler WorkDone;

        private string generateLabel()
        {
            labelNumber++;
            return String.Format("L{0}", labelNumber);
        }

        private void WriteHeader(string idn)
        {
            string header = ".386\n.MODEL\tsmall\n.STACK\t256\n";
            string codeSeg = String.Format(".CODE\n{0}\tPROC\n", idn);
            string endProg = String.Format("mov\tah,4Ch\nmov\tal,0\nint\t21h\n{0}\tENDP\nEND\t{0}", idn);
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, header); // insert header
            dataSegmentPos = resultAsmCode.Length; // set pos to continue writing declar if needed
            posInResultAsmCode = dataSegmentPos;
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, codeSeg); // insert code start point
            codeSegmentPos = resultAsmCode.Length;
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, endProg); // insert end of prog
        }

        private void WriteDataSeg()
        {
            string dataSeg = ".DATA\n";
            resultAsmCode = resultAsmCode.Insert(dataSegmentPos, dataSeg);
            dataSegmentPos += dataSeg.Length;
            codeSegmentPos += dataSeg.Length;
        }

        private void WriteDeclaration(string idn)
        {
            string declar = String.Format("{0}\tdd\t?\n", idn);
            resultAsmCode = resultAsmCode.Insert(dataSegmentPos, declar);
            dataSegmentPos += declar.Length;
            codeSegmentPos += declar.Length;
        }

        private string WriteCondExpr(List<XMLNode> expressions) // returns label
        {
            var operands = expressions.ToArray();
            string label = generateLabel();
            string condition = String.Format("mov\teax, {0}\nmov\tebx, {1}\ncmp\teax, ebx\njne\t{2}\n", operands[0].value, operands[1].value, label);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, condition);
            codeSegmentPos += condition.Length;
            return label;
        }

        private string WriteJumpToEndif()
        {
            string label = generateLabel();
            string jmp = String.Format("jmp\t{0} \n", label);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, jmp);
            codeSegmentPos += jmp.Length;
            return label;
        }

        private void WriteLabel(string label)
        {
            string writeLabel = String.Format("{0}:\n", label);
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, writeLabel);
            codeSegmentPos += writeLabel.Length;
        }

        private void ParseNode(XMLNode parentNode)
        {
            bool parseStatement = false; // if true then do custom parse not just straight going throw the tree
            foreach (var item in parentNode.nodes)
            {
                if (item.name == nodesTypes.procedure_idn)
                    WriteHeader(item.value);
                if (item.name == nodesTypes.var_declar && item.nodes.Count > 0)
                    WriteDataSeg();
                if (item.name == nodesTypes.declaration)
                    WriteDeclaration(item.nodes.First(x => x.name == nodesTypes.var_idn).value);
                if (item.name == nodesTypes.statement_list)
                {
                    parseStatement = true;
                    List<XMLNode> conditional_statement = item.nodes
                                        .First(x => x.name == nodesTypes.statement)
                                        .nodes.First(x => x.name == nodesTypes.conditional_statement).nodes; // get nodes of if cond statement
                    List<XMLNode> incomplete_cond = conditional_statement.First(x => x.name == nodesTypes.incomplete_conditional_statement).nodes;
                    List<XMLNode> cond_expression = incomplete_cond.First(x => x.name == nodesTypes.conditional_expression).nodes;

                    string label = WriteCondExpr(cond_expression.FindAll(x => x.name == nodesTypes.expression));

                    if (incomplete_cond.Exists(x => x.name == nodesTypes.statement_list)) // if after THEN there is another statement
                        ParseNode(conditional_statement.First(x => x.name == nodesTypes.incomplete_conditional_statement)); //parse statement

                    string labelEndif = "";
                    bool elsePartExists = conditional_statement.Exists(x => x.name == nodesTypes.alternative_part);
                    if (elsePartExists) // if ELSE part exist write jmp to endif if THEN part executes
                        labelEndif = WriteJumpToEndif();

                    WriteLabel(label);

                    if (elsePartExists) // parse statement in else part if exists
                        if (conditional_statement.First(x => x.name == nodesTypes.alternative_part).nodes.Exists(x => x.name == nodesTypes.statement_list))
                            ParseNode(conditional_statement.First(x => x.name == nodesTypes.alternative_part));

                    if (labelEndif != "")
                        WriteLabel(labelEndif);
                }
                if (!parseStatement)
                    ParseNode(item);
                else
                    continue;
            }
        }
        public void GenerateCode()
        {
            ParseNode(XMLSyntaxTree);
            if (WorkDone != null) WorkDone(resultAsmCode);
        }
    }
}
