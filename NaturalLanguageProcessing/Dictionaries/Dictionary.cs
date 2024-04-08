using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NaturalLanguageProcessing.TextData;

namespace NaturalLanguageProcessing.Dictionaries
{
    public class Dictionary
    {
        private List<DictionaryItem> itemList;

        public Dictionary()
        {
            itemList = new List<DictionaryItem>();  
        }

        // This method should build the dictionary (from the text data set) AND count
        // the number of instances of each token.
        // This can be done in (at least) two different ways: (you must implement only one).
        //
        // Method 1: Make a list of all tokens (i.e. a list of strings) in all sentences,
        // then sort the list. Next, build the list of dictionary items, counting the
        // number of instances in the process.
        //
        // Method 2: Here, you will need a DictionaryItemComparer that compares (in alphabetical order)
        // the tokens of two DictionaryItems. You will use the dictionary item comparer to carry out
        // a binary search, to find the index (in the growing dictionary) of the item
        // corresponding to the token under consideration. If that index is negative,
        // the token is not yet represented in the dictionary: If so, insert it AT THE
        // RIGHT point in the dictionary (to maintain the sort order). Try to figure it
        // out, otherwise ask the examiner or the assistant.
        // If instead the index is non-negative, then increment the count of the item.
        //
        // The running time of the two methods is roughly the same: On a reasonably
        // fast computer, it takes around one minute (in debug mode, much faster in
        // release mode) for a data set of the size considered here.
        public void Build(TextDataSet dataSet)
        {

            // Add code here:
            DictionaryItemComparer comparer = new DictionaryItemComparer();
            foreach (Sentence sentence in dataSet.SentenceList)
            {
                foreach (String token in sentence.TokenList)
                {
                    DictionaryItem token_item = new DictionaryItem(token);
                    int idx = itemList.BinarySearch(token_item, comparer);
                    if (idx < 0)
                    {
                        token_item.Count = 1;
                        itemList.Insert(~idx, token_item);
                    } else
                    {
                        itemList[idx].Count++;
                    }
                }
            }
        }

        public List<DictionaryItem> ItemList
        {
            get { return itemList; }
            set { itemList = value; }
        }
    }
}
