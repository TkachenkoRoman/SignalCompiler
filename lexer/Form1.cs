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

        private void ClearScreens()
        {
            Invoke((MethodInvoker)delegate { richTextBoxOutput.Text = ""; });
            Invoke((MethodInvoker)delegate { richTextBoxErrorList.Text = ""; }); 
        }

        private void oPENToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (openFileDialog1.FileName.Length > 0)
            {
                numberedRTBCode.RichTextBox.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void LexerWorkDone(List<LexicalAnalizerOutput> output, List<Error> errors, List<Constant> constants, List<Identifier> identifiers)
        {
            ClearScreens();
            foreach (var item in output)
            {
                string message = String.Format("Lexem: {0}\t\tCode: {1}\n", item.lexem, item.code);
                Debug.Print(message);
                Invoke((MethodInvoker)delegate { richTextBoxOutput.Text += message; });
            }
            Debug.Print("\n");

            if (errors.Count() > 0)
            {
                foreach (var item in errors)
                {
                    string message = String.Format(item.message + " in row {0}, position {1}\n", item.row.ToString(), item.pos.ToString());
                    Debug.Print(message);
                    Invoke((MethodInvoker)delegate { richTextBoxErrorList.Text += message; }); 
                }
            }
            else
            {
                Invoke((MethodInvoker)delegate { richTextBoxErrorList.Text += "Build succeeded"; }); 
            }

            Invoke((MethodInvoker)delegate { richTextBoxOutput.Text += "\n\nConstants:\n"; });
            foreach (var item in constants)
            {
                string message = String.Format("value: {0}\t\tid: {1}\n", item.value, item.id);
                Invoke((MethodInvoker)delegate { richTextBoxOutput.Text += message; });
            }

            Invoke((MethodInvoker)delegate { richTextBoxOutput.Text += "\n\nIdentifiers:\n"; });
            foreach (var item in identifiers)
            {
                string message = String.Format("name: {0}\t\tid: {1}\n", item.name, item.id);
                Invoke((MethodInvoker)delegate { richTextBoxOutput.Text += message; });
            }
        }
        private void buildSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string path = System.IO.Directory.GetCurrentDirectory() + @"\test.txt";
            LexicalAnalizer lexer = new LexicalAnalizer(numberedRTBCode.RichTextBox.Lines);
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
                numberedRTBCode.RichTextBox.SaveFile(saveFileDialog1.FileName, RichTextBoxStreamType.PlainText);
            }
        }
    }
}
