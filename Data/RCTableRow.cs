using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	public class RCTableRow
	{

		public static List<string> FieldNames;
		public List<string> Values;

		public RCTableRow(List<string> fieldNames)
		{
			System.Console.WriteLine("RCTableRow");

			FieldNames = fieldNames;
			Values = new List<string>(fieldNames.Count);

		}

		public string GetValue(int index)
        {
			return (index >= 0 && index < Values.Count) ? Values[index] : "";
        }

		public string GetValue(string fieldName)
        {
            return FieldNames.Contains(fieldName) ? Values[FieldNames.IndexOf(fieldName)] : "";
        }

		public void SetValue(string fieldName, string value)
        {
            if (FieldNames.Contains(fieldName) && FieldNames.IndexOf(fieldName) < Values.Count)
            {
				Values[FieldNames.IndexOf(fieldName)] = value;
            }
        }

	}
}