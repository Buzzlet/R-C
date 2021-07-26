using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using R_C.Data;

namespace R_CTester
{
    class Program
    {
        private static readonly Random _random = new Random();
        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting R-CTester!");

            RCTable myTable = new RCTable();

            myTable.AddField("Field1");
            myTable.AddField("Field2");
            myTable.AddField("Field3");

            RCTableRow myRow = myTable.AddNewRow();

            myRow.SetValue("Field1", "4");
            myRow.SetValue("Field2", "2");
            myRow.SetValue("Field3", "3");

            myRow = myTable.AddNewRow();

            myRow.SetValue("Field1", "1");
            myRow.SetValue("Field2", "5");
            myRow.SetValue("Field3", "6");

            myRow = myTable.AddNewRow();

            myRow.SetValue("Field1", "7");
            myRow.SetValue("Field2", "8");
            myRow.SetValue("Field3", "9");

            myRow = myTable.AddNewRow();
            myRow.SetValue("Field1", "1");
            myRow.SetValue("Field2", "5");
            myRow.SetValue("Field3", "7");
            myRow = myTable.AddNewRow();
            myRow.SetValue("Field1", "1");
            myRow.SetValue("Field2", "5");
            myRow.SetValue("Field3", "5");

            myTable.CreateIndexOn("testIndex", IndexType.StronglyUnique, "Field1", "Field3");
            myTable.SetIndex("testIndex");

            using (StreamWriter writer = File.CreateText("F:\\Documents\\localProgramming\\tmp\\myTree.out"))
            {
                myTable.Indices[myTable.CurrentIndex].CompareTree.WriteGraphviz(writer);
            }
            
            /*
            foreach (RCTableRow row in myTable)
            {
                Console.WriteLine("Field2:" + row.GetValue("Field2"));
            }

            using (StreamWriter writer = File.CreateText("F:\\Documents\\localProgramming\\R-C\\myTree.out"))
			{
                RCRBTree<int> myTree = new RCRBTree<int>();
                for (int i = 0; i < 5000; i++)
			    {
                    myTree.Add(_random.Next(-1000,1000));
			    }
                myTree.WriteGraphviz(writer);
			}
            */
                
        }

        public static string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }
    }
}
