using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace lexer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //SerializeTables.SerializeAttributes();
            //SerializeTables.SeriaizeKeyWords();
            //SerializeTables.SeriaizeIdentifiers();

            //SerializeTables.DeserializeAttributes();
            //SerializeTables.DeserializeKeyWords();
            //SerializeTables.DeserializeIdentifiers();

            LexicalAnalizer lexer = new LexicalAnalizer();
            string path = System.IO.Directory.GetCurrentDirectory() + @"\test.txt";
            List<LexicalAnalizerOutput> result = lexer.Analize(path);
            foreach (var item in result)
            {
                Debug.Print("Lexem: {0}\tCode: {1}", item.lexem, item.code);
            }
            Debug.Print("\n");
            if (lexer.errors.Count() > 0)
            {
                foreach (var item in lexer.errors)
                {
                    Debug.Print(item.message + " in row {0}, position {1}", item.row.ToString(), item.pos.ToString());
                }
            }
        }
    }
}
