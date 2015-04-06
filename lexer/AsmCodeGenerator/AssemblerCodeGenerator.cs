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
        }
        private XMLNode XMLSyntaxTree;
        private string resultAsmCode;
        private int posInResultAsmCode;
        private int dataSegmentPos;
        private int codeSegmentPos;

        public delegate void WorkDoneHandler(string output);
        public event WorkDoneHandler WorkDone;

        private void WriteHeader(string idn)
        {
            string header = ".8086\n.MODEL\tsmall\n.STACK\t256\n";
            string codeSeg = String.Format("\n.CODE\n{0}\tPROC\n", idn);
            string endProg = String.Format("mov\tah,4Ch\nmov\tal,0\nint\t21h\n{0}\tENDP\nEND\t{0}", idn);
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, header); // insert header
            dataSegmentPos = resultAsmCode.Length; // set pos to continue writing declar if needed
            posInResultAsmCode = dataSegmentPos;
            resultAsmCode = resultAsmCode.Insert(posInResultAsmCode, codeSeg); // insert code start point
            codeSegmentPos = resultAsmCode.Length;
            resultAsmCode = resultAsmCode.Insert(codeSegmentPos, endProg); // insert end of prog
        }

        private void ParseNode(XMLNode parentNode)
        {
            foreach (var item in parentNode.nodes)
            {
                if (item.name == nodesTypes.procedure_idn)
                    WriteHeader(item.value);

                ParseNode(item);
            }
        }
        public void GenerateCode()
        {
            ParseNode(XMLSyntaxTree);
            if (WorkDone != null) WorkDone(resultAsmCode);
        }
    }
}
