﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturalLanguageProcessing.NGrams
{
    public class NGramSet
    {
        private List<NGram> itemList;
        private NGramComparer comparer;

        public NGramSet()
        {
            itemList = new List<NGram>();
            comparer = new NGramComparer();
        }

        public void AppendUnigram(string token, int count)
        {
            NGram unigram = new NGram(token);
            unigram.NumberOfInstances = count;
            itemList.Add(unigram);
        }

        public void Append(List<string> tokenList)
        {
            NGram nGram = new NGram(tokenList);

            // Option 1: Just add 2-grams to the itemList. Then, in the end, sort them based
            // on the tokenString, then count (i.e. as in Method1 in the Dictionary.Build() method). 
            // I have not tried this method here, but it should be fine,
            // perhaps even faster than Option 1.

            // Option 2: Use binary search (on the tokenString of the nGram)
            // to find its index. If the index is negative, then this nGram is not
            // yet in the list. If so, insert it *in the appropriate location* to
            // keep the nGramList sorted (based on the tokenString). Try to figure
            // out how to do that (e.g. on StackOverflow) - if you cannot, then ask 
            // the examiner or the assistant.
            // Only a few lines of code are needed for this method... :)
            // However, it will take quite a while to run -- the binary search
            // becomes increasingly slower as the list grows.
            //
            //
            // (There may be other options too...)


            // Add code here
            int idx = itemList.BinarySearch(nGram, comparer);
            if (idx < 0)
            {
                nGram.NumberOfInstances = 1;
                itemList.Insert(~idx, nGram);
            }
            else
            {
                itemList[idx].NumberOfInstances++;
            }
            
        }

        // This method you get for free... :)
        public void SortOnFrequency()
        {
            itemList = itemList.OrderByDescending(n => n.NumberOfInstances).ToList();
        }

        public List<NGram> ItemList
        {
            get { return itemList; }
            set { itemList = value; }
        }
    }
}
