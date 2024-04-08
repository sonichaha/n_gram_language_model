using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaturalLanguageProcessing.Dictionaries;

namespace NaturalLanguageProcessing.TextData
{
    public class TextDataSet
    {
        private List<Sentence> sentenceList;
        private Dictionary dictionary;

        public TextDataSet()
        {
            sentenceList = new List<Sentence>();
        }

        public void Tokenize()
        {
            foreach (Sentence sentence in sentenceList)
            { 
                sentence.Tokenize();
            }
        }


        // =================================
        // Write this method!
        //
        // Note: On a data set of the size used here, it takes a while to run
        // (a few minutes, in Release mode, more in Debug mode).
        public void MakeDictionaryAndIndex()
        {
            dictionary = new Dictionary();
            dictionary.Build(this);  // You must write this method

            // Run through all tokens in all sentences, and find the corresponding
            // index (in the dictionary) of each token, and then add it to the
            // tokenIndexList (of the sentence in question). Note that, for
            // *this* problem, we don't really need the tokenIndexList (in the
            // sentences), but it's a good exercise to learn how to make it.
            //
            // Here, you must use binary search, for which you, in turn, need
            // DictionaryItemComparer. See if you can figure out how to use it,
            // otherwise ask the examiner or the assistant.

            DictionaryItemComparer comparer = new DictionaryItemComparer();
            foreach (Sentence sentence in sentenceList)
            {
                
                // Add code here
                foreach (String token in sentence.TokenList)
                {
                    DictionaryItem token_item = new DictionaryItem(token);
                    int idx = dictionary.ItemList.BinarySearch(token_item, comparer);
                    sentence.TokenIndexList.Add(idx);
                }
            }
        }

        public List<Sentence> SentenceList
        {
            get { return sentenceList; }
        }

        public Dictionary Dictionary
        {
            get { return dictionary; }
        }
    }
}
