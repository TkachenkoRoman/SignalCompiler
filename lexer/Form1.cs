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
            this.numberedRTBCode.RichTextBox.TextChanged += new System.EventHandler(this.numberedRTBCodeRichTextBoxTextChanged);
            //SerializeTables.SerializeAttributes();
            //SerializeTables.SeriaizeKeyWords();
            //SerializeTables.SeriaizeIdentifiers();

            //SerializeTables.DeserializeAttributes();
            //SerializeTables.DeserializeKeyWords();
            //SerializeTables.DeserializeIdentifiers();

            lexer = new LexicalAnalizer(numberedRTBCode.RichTextBox.Lines);
            programBuilded = false;
        }

        LexicalAnalizer lexer;
        bool programBuilded;

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
             // add space to call TextChanged and highlight syntax
            Invoke((MethodInvoker)delegate { numberedRTBCode.RichTextBox.Text += " "; });
            

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

            //
            // CALL SYNTAX_ANALIZEER WHEN LEXER FINISES
            //
            SyntaxAnalizer syntaxer = new SyntaxAnalizer(output, constants, identifiers, lexer.keyWords);
            syntaxer.WorkDone += new SyntaxAnalizer.WorkDoneHandler(SyntaxerWorkDone);
            Thread syntaxerThread = new Thread(new ThreadStart(syntaxer.Analize));
            syntaxerThread.Start();
        }


        private void SyntaxerWorkDone(List<Error> errors, List<IdentifierExt> identifiersExt) 
        {          
            if (identifiersExt.Count() > 0)
            {
                Invoke((MethodInvoker)delegate { this.richTextBoxOutput.Text += "\nIdentifiers extended table:\n"; });
                foreach (var item in identifiersExt)
                {
                    string message = String.Format(String.Format("name: {0}\t\ttype: {1}\n", item.name, item.typeAttribute));
                    Debug.Print(message);
                    Invoke((MethodInvoker)delegate { this.richTextBoxOutput.Text += message; });
                }
            }
            
            if (errors.Count() > 0)
            {
                Invoke((MethodInvoker)delegate { richTextBoxErrorList.Text += "\n\nSyntax errors:\n"; });
                foreach (var item in errors)
                {
                    string message = String.Format(item.message + " in row {0}\n", item.row.ToString());
                    Debug.Print(message);
                    Invoke((MethodInvoker)delegate { richTextBoxErrorList.Text += message; });
                }
            }
            

            programBuilded = true;
        }
        


        private void buildSolutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string path = System.IO.Directory.GetCurrentDirectory() + @"\test.txt";
            lexer = new LexicalAnalizer(numberedRTBCode.RichTextBox.Lines);
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

        //
        //numberedRTB
        //
        private void numberedRTBCodeRichTextBoxTextChanged(object sender, EventArgs e) // highlight syntax
        {
            programBuilded = false;

            int start = 0;
             // avoid blinking
            Invoke((MethodInvoker)delegate { labelOutput.Focus(); });

            // saving original caret position + forecolor
            int originalIndex = numberedRTBCode.RichTextBox.SelectionStart; 
            int originalLength = numberedRTBCode.RichTextBox.SelectionLength; 
            Color originalColor = numberedRTBCode.RichTextBox.SelectionColor;

            // removes any previous highlighting (so modified words won't remain highlighted)
            numberedRTBCode.RichTextBox.SelectionStart = 0;
            numberedRTBCode.RichTextBox.SelectionLength = numberedRTBCode.RichTextBox.Text.Length;
            numberedRTBCode.RichTextBox.SelectionColor = Color.Black;

          
            if (numberedRTBCode.RichTextBox.Text.Length > 0)
            {
                if (lexer.keyWords.Count > 0)
                {
                    foreach (var item in lexer.keyWords) // highlight keyWords
                    {
                        start = 0;
                        while (start < numberedRTBCode.RichTextBox.Text.Length)
                        {
                            start = SelectWordsInCode(start, item.keyWord, Color.Blue);
                            if (start == -1)
                            {
                                break;
                            }
                        }
                    }
                }
                if (lexer.constants.Count > 0)
                {
                    foreach (var item in lexer.constants) // highlight constants
                    {
                        start = 0;
                        while (start < numberedRTBCode.RichTextBox.Text.Length)
                        {
                            start = SelectWordsInCode(start, item.value.ToString(), Color.MediumVioletRed);
                            if (start == -1)
                            {
                                break;
                            }
                        }
                    }
                }
                if (lexer.identifiers.Count > 0)
                {
                    foreach (var item in lexer.identifiers.Where(x => x.type == identifierType.system)) // highlight identifiers
                    {
                        start = 0;
                        while (start < numberedRTBCode.RichTextBox.Text.Length)
                        {
                            start = SelectWordsInCode(start, item.name, Color.CadetBlue);
                            if (start == -1)
                            {
                                break;
                            }
                        }
                    }
                }
                start = 0;
                while (start < numberedRTBCode.RichTextBox.Text.Length) // highlight comments
                {
                    int begComFindIndex = -1;
                    int endComFindIndex = -1;
                    begComFindIndex = numberedRTBCode.RichTextBox.Find("(*", start, numberedRTBCode.RichTextBox.Text.Length, RichTextBoxFinds.MatchCase);
                    if (begComFindIndex != -1)
                        start = begComFindIndex + 2; // skip begcom symb
                    endComFindIndex = numberedRTBCode.RichTextBox.Find("*)", start, numberedRTBCode.RichTextBox.Text.Length, RichTextBoxFinds.MatchCase);
                    if (begComFindIndex >= 0 && endComFindIndex > 0 )
                    {
                        if (begComFindIndex < endComFindIndex)
                        {
                            int selectionLength = endComFindIndex - begComFindIndex + 2;
                            numberedRTBCode.RichTextBox.SelectionStart = begComFindIndex;
                            numberedRTBCode.RichTextBox.SelectionLength = selectionLength;
                            numberedRTBCode.RichTextBox.SelectionColor = Color.Green;
                            start = endComFindIndex;
                        }
                    }
                    if (endComFindIndex == -1 || begComFindIndex == -1)
                        break;
                }
                
            }

            // restoring the original colors, for further writing
            numberedRTBCode.RichTextBox.SelectionStart = originalIndex;
            numberedRTBCode.RichTextBox.SelectionLength = originalLength;
            numberedRTBCode.RichTextBox.SelectionColor = originalColor;

            // restore focus
            Invoke((MethodInvoker)delegate { numberedRTBCode.RichTextBox.Focus(); });
        }

        private int SelectWordsInCode(int start, string word, Color color)
        {
            int startIndex = -1;
            if (start >= 0)
                startIndex = numberedRTBCode.RichTextBox.Find(word, start, numberedRTBCode.RichTextBox.Text.Length, RichTextBoxFinds.MatchCase);
            if (startIndex >= 0)
            {
                int selectionLength = word.Length;
                numberedRTBCode.RichTextBox.SelectionStart = startIndex;
                numberedRTBCode.RichTextBox.SelectionLength = selectionLength;
                numberedRTBCode.RichTextBox.SelectionColor = color;
                return startIndex + selectionLength;
            }
            return -1;
        }

        private void syntaxTreeGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form graphForm = new Form();
            graphForm.Width = 1280;
            graphForm.Height = 720;

            if (programBuilded)
            {
                Microsoft.Glee.GraphViewerGdi.GViewer viewer = new Microsoft.Glee.GraphViewerGdi.GViewer();
           

                //create a graph object
                SyntaxTree.XMLNodeToGLEE GleeCreator = new SyntaxTree.XMLNodeToGLEE();
                Microsoft.Glee.Drawing.Graph graph = GleeCreator.GetGraph();

                System.Windows.Forms.Layout.LayoutEngine layout = viewer.LayoutEngine;
                graph.GraphAttr.OptimizeLabelPositions = false;
                graph.GraphAttr.LayerDirection = Microsoft.Glee.Drawing.LayerDirection.TB;
                graph.GraphAttr.LabelFloat = (Microsoft.Glee.Drawing.LabelFloat)1;
                

                //graph.GraphAttr.AspectRatio = 5.0;
                //graph.MinNodeWidth = 300;

                viewer.Graph = graph;

                graphForm.SuspendLayout();
                viewer.Dock = System.Windows.Forms.DockStyle.Fill;
                graphForm.Controls.Add(viewer);
                graphForm.ResumeLayout();

                graphForm.ShowDialog(); 
            }
        }
    }
}
