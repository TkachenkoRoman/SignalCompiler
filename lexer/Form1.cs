using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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

            
        }

        private void oPENToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.FileName.Length > 0)
            {
                richTextBoxCode.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void LexerWorkDone(List<LexicalAnalizerOutput> output, List<Error> errors)
        {
            foreach (var item in output)
            {
                Debug.Print("Lexem: {0}\tCode: {1}", item.lexem, item.code);
            }
            Debug.Print("\n");
            if (errors.Count() > 0)
            {
                foreach (var item in errors)
                {
                    Debug.Print(item.message + " in row {0}, position {1}", item.row.ToString(), item.pos.ToString());
                }
            }
        }
        private void buildSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string path = System.IO.Directory.GetCurrentDirectory() + @"\test.txt";
            LexicalAnalizer lexer = new LexicalAnalizer(richTextBoxCode.Lines);
            lexer.WorkDone += LexerWorkDone;
            Thread lexerThread = new Thread(new ThreadStart(lexer.Analize));

            lexerThread.Start();

            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (saveFileDialog1.FileName.Length > 0)
            {
                richTextBoxCode.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }
    }
}
