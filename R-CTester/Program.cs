using System;
using R_C.Data;

namespace R_CTester
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Starting R-CTester!");

            RCTable myTable = new RCTable();

            myTable.AddField("Field1");
            myTable.AddField("Field2");
            myTable.AddField("Field3");

            RCTableRow myRow = myTable.AddNewRow();

            myRow.SetValue("Field1", "1");
            myRow.SetValue("Field2", "2");
            myRow.SetValue("Field3", "3");

            myRow = myTable.AddNewRow();

            myRow.SetValue("Field1", "4");
            myRow.SetValue("Field2", "5");
            myRow.SetValue("Field3", "6");

            foreach (RCTableRow row in myTable)
            {
                Console.WriteLine("Field2:" + row.GetValue("Field2"));
            }
        }
    }
}
