using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NaturalLanguageProcessing.Dictionaries;
using NaturalLanguageProcessing.NGrams;
using NaturalLanguageProcessing.TextData;

namespace NGramsApplication
{
    public partial class MainForm : Form
    {
        private const string TEXT_FILTER = "Text files (*.txt)|*.txt";

        private TextDataSet writtenDataSet;
        private TextDataSet spokenDataSet;
        private NGramSet writtenUniGramSet; 
        private NGramSet writtenBiGramSet;
        private NGramSet writtenTriGramSet;
        private NGramSet spokenUniGramSet;
        private NGramSet spokenBiGramSet;
        private NGramSet spokenTriGramSet;

        private Thread tokenizationThread;
        private Thread indexingThread;
        private Thread processingThread;

        private List<string> analysisList;

        public MainForm()
        {
            InitializeComponent();
        }

        private void ImportTextData(string fileName)
        {
            writtenDataSet = new TextDataSet();
            spokenDataSet = new TextDataSet();
            using (StreamReader dataReader = new StreamReader(fileName))
            {
                while (!dataReader.EndOfStream)
                {
                    string line = dataReader.ReadLine();
                    List<string> lineSplit = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Sentence sentence = new Sentence();
                    sentence.Text = lineSplit[1];
                    sentence.Text = sentence.Text.Replace(" , ", " ");

                    if (lineSplit[0] == "0") // Spoken sentence (Class 0)
                    {
                        spokenDataSet.SentenceList.Add(sentence);
                    }
                    else // Written sentence (Class 1)
                    {
                        writtenDataSet.SentenceList.Add(sentence);
                    }
                }
                dataReader.Close();
            }
        }

        private void importTextDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = TEXT_FILTER;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ImportTextData(openFileDialog.FileName);
                    tokenizeButton.Enabled = true;  
                }
            }
        }

        // this methdo should display items in the analysis text box (it should be
        // called by ThreadSafeShowAnalysis().
        private void ShowAnalysis()
        {

            // Add code here
            foreach (string text in analysisList)
            {
                analysisTextBox.Text += text + "\n";
            }
        }

        // This method should, in turn, call ShowAnalysis() in a thread-safe manner
        private void ThreadSafeShowAnalysis()
        {

            // Add code here
            if (InvokeRequired) { BeginInvoke(new MethodInvoker(() => ShowAnalysis())); }
            else { ShowAnalysis(); }
        }

        private void ThreadSafeToggleButtonEnabled(ToolStripButton button, Boolean enabled)
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => button.Enabled = enabled)); }
            else { button.Enabled = enabled; }
        }

        private void ThreadSafeToggleMenuItemEnabled(ToolStripMenuItem menuItem, Boolean enabled)
        {
            if (InvokeRequired) { this.Invoke(new MethodInvoker(() => menuItem.Enabled = enabled)); }
            else { menuItem.Enabled = enabled; }
        }

        private void TokenizationLoop()
        {
            writtenDataSet.Tokenize();
            spokenDataSet.Tokenize();
            ThreadSafeToggleButtonEnabled(makeDictionaryAndIndexButton, true);
        }

        private void tokenizeButton_Click(object sender, EventArgs e)
        {
            tokenizeButton.Enabled = false;
            tokenizationThread = new Thread(new ThreadStart(() => TokenizationLoop()));
            tokenizationThread.Start();
        }

        private void IndexingLoop()
        {
            writtenDataSet.MakeDictionaryAndIndex();
            spokenDataSet.MakeDictionaryAndIndex();
            ThreadSafeToggleButtonEnabled(processButton, true);
        }

        private void makeDictionaryAndIndexButton_Click(object sender, EventArgs e)
        {
            makeDictionaryAndIndexButton.Enabled = false;
            indexingThread = new Thread(new ThreadStart(() => IndexingLoop()));
            indexingThread.Start();
        }


        // ========================================================
        // Here, write methods for
        // ========================================================
        //
        // * Finding the 300 most common 1-grams, 2-grams, 3-grams in each set
        //     For the unigrams (1-grams), use the dictionary that you generated above.
        //     You must generate the sets of bigrams (2-grams) and trigrams (3-grams), using
        //     the NGramSet and its methods (see NGramSet.Append(...) ...)
        // * Finding the number of distinct tokens (unigrams) in the spoken set (i.e. the
        //   number of items in its dictionary) and the written set, as well as the number of shared (distinct) tokens.
        // * Finding the 50 tokens with the largest values of r (see the problem formulation)
        //     and the 50 tokens with the smallest values of r.
        // 
        private void ProcessingLoop()
        {
            const int COUNT_CUTOFF = 500;
            const int NUMBER_OF_N_GRAMS_SHOWN = 300;

            // Add your analysis as a list of strings (that can then be shown on screen and saved to file).
            analysisList = new List<string>();
            
            // Step (1)
            // Find the 300 most common unigrams (from the dictionaries, after sorting
            //     the dictionary items based on the number of instances
            writtenUniGramSet = new NGramSet();

            // Add code here

            foreach (DictionaryItem dic_item in writtenDataSet.Dictionary.ItemList)
            {
                if (dic_item.Count > COUNT_CUTOFF)
                {
                    writtenUniGramSet.AppendUnigram(dic_item.Token, dic_item.Count);
                }
            }
            writtenUniGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Written unigrams: ");
            analysisList.Add("=========================================");
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < writtenUniGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(writtenUniGramSet.ItemList[ii].AsString());
            }

            spokenUniGramSet = new NGramSet();

            // Add code here

            foreach (DictionaryItem dic_item in spokenDataSet.Dictionary.ItemList)
            {
                if (dic_item.Count > COUNT_CUTOFF)
                {
                    spokenUniGramSet.AppendUnigram(dic_item.Token, dic_item.Count);
                }
            }

            spokenUniGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Spoken unigrams: ");
            analysisList.Add("=========================================");
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < spokenUniGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(spokenUniGramSet.ItemList[ii].AsString());
            }
            
            
            // Step (2) 
            // Find the 300 most common bigrams, after generating the bigram sets,
            //     one for the written data and one for the spoken using the NGramSet class 
            //    (see the NGramSet class).
            writtenBiGramSet = new NGramSet();
            
             // Add code here for generating and sorting the written bigram set.
             // Before sorting, run through the list and remove rare bigrams (speeds up
             // the sorting - we are only interested in the most frequent bigrams anyway;
             // see also the assignment text.

            foreach (Sentence sentence in writtenDataSet.SentenceList)
            {
                for (int idx = 0; idx < sentence.TokenList.Count() - 2; idx++)
                {
                    List<string> two_tokens = new List<string>();
                    two_tokens.Add(sentence.TokenList[idx]);
                    two_tokens.Add(sentence.TokenList[idx + 1]);
                    writtenBiGramSet.Append(two_tokens);
                }
            }

            writtenBiGramSet.ItemList = (from item in writtenBiGramSet.ItemList
                                         where item.NumberOfInstances > COUNT_CUTOFF
                                         select item).ToList();

            writtenBiGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Written 2-grams: ");
            analysisList.Add("=========================================");
            analysisList.Add(writtenBiGramSet.ItemList.Count().ToString());
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < writtenBiGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(writtenBiGramSet.ItemList[ii].AsString());
            }
            

            spokenBiGramSet = new NGramSet();

            // Add code here for generating and sorting the spoken bigram set.
            // Before sorting, run through the list and remove rare bigrams (speeds up
            // the sorting - we are only interested in the most frequent bigrams anyway;
            // see also the assignment text.

            foreach (Sentence sentence in spokenDataSet.SentenceList)
            {
                for (int idx = 0; idx < sentence.TokenList.Count() - 2; idx++)
                {
                    List<string> two_tokens = new List<string>();
                    two_tokens.Add(sentence.TokenList[idx]);
                    two_tokens.Add(sentence.TokenList[idx + 1]);
                    spokenBiGramSet.Append(two_tokens);
                }
            }

            spokenBiGramSet.ItemList = (from item in spokenBiGramSet.ItemList
                                         where item.NumberOfInstances > COUNT_CUTOFF
                                         select item).ToList();

            spokenBiGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Spoken 2-grams: ");
            analysisList.Add("=========================================");
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < spokenBiGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(spokenBiGramSet.ItemList[ii].AsString());
            }
            
            
            // Step (3) 
            //     Find the 300 most common trigrams (after generating the trigram set.
            //     using the NGramSet class (where you have to write code for appending
            //     n-grams, making sure to keep them sorted in alphabetical order based
            //     on the full token string (see the NGramSet class)

            // Add code here for generating and sorting the written trigrams set.
            // Before sorting, run through the list and remove rare trigrams (speeds up
            // the sorting - we are only interested in the most frequent trigram anyway;
            // see also the assignment text.

            writtenTriGramSet = new NGramSet();

            foreach (Sentence sentence in writtenDataSet.SentenceList)
            {
                for (int idx = 0; idx < sentence.TokenList.Count() - 3; idx++)
                {
                    List<string> three_tokens = new List<string>();
                    three_tokens.Add(sentence.TokenList[idx]);
                    three_tokens.Add(sentence.TokenList[idx + 1]);
                    three_tokens.Add(sentence.TokenList[idx + 2]);
                    writtenTriGramSet.Append(three_tokens);
                }
            }

            writtenTriGramSet.ItemList = (from item in writtenTriGramSet.ItemList
                                         where item.NumberOfInstances > COUNT_CUTOFF
                                         select item).ToList();

            writtenTriGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Written 3-grams: ");
            analysisList.Add("=========================================");
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < writtenTriGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(writtenTriGramSet.ItemList[ii].AsString());
            }
            
            spokenTriGramSet = new NGramSet();

            // Add code here for generating and sorting the spoken trigram set.
            // Before sorting, run through the list and remove rare trigrams (speeds up
            // the sorting - we are only interested in the most frequent trigrams anyway;
            // see also the assignment text.

            foreach (Sentence sentence in spokenDataSet.SentenceList)
            {
                for (int idx = 0; idx < sentence.TokenList.Count() - 3; idx++)
                {
                    List<string> three_tokens = new List<string>();
                    three_tokens.Add(sentence.TokenList[idx]);
                    three_tokens.Add(sentence.TokenList[idx + 1]);
                    three_tokens.Add(sentence.TokenList[idx + 2]);
                    spokenTriGramSet.Append(three_tokens);
                }
            }

            spokenTriGramSet.ItemList = (from item in spokenTriGramSet.ItemList
                                          where item.NumberOfInstances > COUNT_CUTOFF
                                          select item).ToList();

            spokenTriGramSet.SortOnFrequency();

            analysisList.Add("=========================================");
            analysisList.Add("Spoken 3-grams: ");
            analysisList.Add("=========================================");
            for (int ii = 0; ii < NUMBER_OF_N_GRAMS_SHOWN && ii < spokenTriGramSet.ItemList.Count(); ii++)
            {
                analysisList.Add(spokenTriGramSet.ItemList[ii].AsString());
            }
            
            // Step (4) Using the dictionaries (one for the written set and one for the spoken),
            // Find the 50 tokens with the largest values of r and the 50 tokens with the
            // smallest values of r. Consider only tokens that are present (with at least
            // one instance) in both the written and the spoken data sets.
            //
            // Here, you can run through one of the dictionaries (e.g. the spoken one, which
            // is probably smaller) token by token, then check if the corresponding token
            // can be found in the other dictionary. If yes, then compute the ratio r,
            // and store the information. To that end, you can define new classes, 
            // similar to the NGramSet and NGram classes, but with items
            // consisting of (i) the token and (ii) the value of r. Then sort on r,
            // and get the top 50 elements as well as the 50 bottom elements ...
            // You'll need a custom comparer too which uses the 
            // value of r as the basis for comparison.
            //
            // There are other ways - this is just a suggestion... You can, for example,
            // use the "Tuple" concept in C#, defining a tuple (string, double) with
            // tokens and r-values, then make a list of such tuples and sort it (based on r, i.e. "Item2";
            // see the definition of the Tuple concept e.g. on MSDN.
            // In that case, use: List<Tuple<string, double>> tokenRTupleList = new List<Tuple<string, double>>();

            //
            // Add code here:
            //
            DictionaryItemComparer comparer = new DictionaryItemComparer();
            List<KeyValuePair<string, double>> shared_with_r_list = new List<KeyValuePair<string, double>>();
            foreach (DictionaryItem spoken_item in spokenDataSet.Dictionary.ItemList)
            {
                int idx_wri_item = writtenDataSet.Dictionary.ItemList.BinarySearch(spoken_item, comparer);
                if (idx_wri_item < 0)
                {
                    continue;
                }
                int number_spoken = spoken_item.Count;
                int number_written = writtenDataSet.Dictionary.ItemList[idx_wri_item].Count;
                double r = (double)number_written / (double)number_spoken;
                KeyValuePair<string, double> item_with_r = new KeyValuePair<string, double>(spoken_item.Token, r);
                shared_with_r_list.Add(item_with_r);
            }

            shared_with_r_list.Sort((item1, item2) => item1.Value.CompareTo(item2.Value));
            var small_r_tokens = shared_with_r_list.Take(50).ToList();
            shared_with_r_list.Reverse();
            var high_r_tokens = shared_with_r_list.Take(50).ToList();

            // Then store information about the number of tokens in each set, as well as the number of shared tokens:
            analysisList.Add("=========================================");
            analysisList.Add("Number of tokens:");
            analysisList.Add("=========================================");
            analysisList.Add("Spoken set:  " + spokenDataSet.Dictionary.ItemList.Count.ToString());
            analysisList.Add("Written set: " + writtenDataSet.Dictionary.ItemList.Count.ToString());
            analysisList.Add("Shared:      " + shared_with_r_list.Count.ToString());

            // Then store information about the 50 tokens with the highest r-values:
            analysisList.Add("=========================================");
            analysisList.Add("High-r tokens: ");
            analysisList.Add("=========================================");

            // Add code here
            for (int ii = 0; ii < 50; ii++)
            {
                analysisList.Add(high_r_tokens[ii].ToString());
            }

            // Then store information about the 50 tokens with the lowest r-values:
            analysisList.Add("=========================================");
            analysisList.Add("Small-r tokens: ");
            analysisList.Add("=========================================");

            // Add code here
            for (int ii = 0; ii < 50; ii++)
            {
                analysisList.Add(small_r_tokens[ii].ToString());
            }
            
            // =================================================================
            // (5) Write a thread-safe method here for
            // display the analysis in the analysisTextBox
            // see also ThreadSafeToggleMenuItemEnabled(), to see how it's done. 
            // Hint: The ThreadSafeMethod (that uses "Invoke..." can itself
            // call a standard (non-thread safe) method, in case one needs to
            // carry out more operations than just a single assignment...)
            // See also Appendix A.4 in the compendium.
            // ==================================================================
            ThreadSafeShowAnalysis();

            // =============================
            // Don't change below this line:
            // =============================
            ThreadSafeToggleMenuItemEnabled(saveAnalysisToolStripMenuItem, true);
        }

        private void processButton_Click(object sender, EventArgs e)
        {
            processButton.Enabled = false;
            processingThread = new Thread(new ThreadStart(() => ProcessingLoop()));
            processingThread.Start();
        }

        // Here, the results shown in the analysisTextBox are shown. One can also
        // (equivalently) just save the contents of the analysisList (which is indeed
        // what is shown in the textBox...)
        private void saveAnalysisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = TEXT_FILTER;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (StreamWriter dataWriter = new StreamWriter(saveFileDialog.FileName))
                    {
                        for (int ii = 0; ii < analysisTextBox.Lines.Count(); ii++)
                        {
                            dataWriter.WriteLine(analysisTextBox.Lines[ii]);
                        }
                        dataWriter.Close();
                    }
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
