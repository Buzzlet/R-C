using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	// JNR 2021-07-19
	// Table Row
	public class RCTableRow : IComparable
	{

		public static List<string> FieldNames;
		public List<object> Values;
		public RCTable Table;
		public Dictionary<string, string> IndicesValues;
		public string CurrentIndexValue
		{
			get
			{
				return IndicesValues[Table.CurrentIndex];
			}
		}

		// Indexer
		public dynamic this[string s]
		{
			get { return GetDynamic(s);  }
			set { SetValue(s, value);  }
		}

		public RCTableRow(RCTable table, List<string> fieldNames)
		{
			System.Console.WriteLine("RCTableRow");

			IndicesValues = new Dictionary<string, string>();
			Table = table;
			FieldNames = fieldNames;
			Values = new List<object>(fieldNames.Count);

			for (int i = 0; i < fieldNames.Count; i++)
			{
				Values.Add(new Object());
			}

			CalculateIndicesValues();

		}

		public void CalculateIndicesValues()
		{

			IndicesValues.Clear();

			foreach (RCTableIndex index in Table.Indices.Values)
			{
				string indexValue = "{";
				foreach (string field in index.Fields)
				{
					indexValue += ("\"" + Values[FieldNames.IndexOf(field)].ToString() + "\"");
					if (field != index.Fields.Last())
					{
						indexValue += ",";
					}
				}
				indexValue += "}";

				IndicesValues.Add(index.Name, indexValue);
			}
			
		}

		public object GetValue(int index)
		{
			return (index >= 0 && index < Values.Count) ? Values[index] : null;
		}

		public object GetValue(string fieldName)
		{
			return FieldNames.Contains(fieldName) ? Values[FieldNames.IndexOf(fieldName)] : null;
		}

		public dynamic GetDynamic(int index)
		{
			return (index >= 0 && index < Values.Count) ? Values[index] : null;
		}

		public dynamic GetDynamic(string fieldName)
		{
			return FieldNames.Contains(fieldName) ? Values[FieldNames.IndexOf(fieldName)] : null;
		}

		public void SetValue(string fieldName, object value)
		{
			if (FieldNames.Contains(fieldName) && FieldNames.IndexOf(fieldName) < Values.Count)
			{
				// Handle updating indices
				foreach (RCTableIndex index in Table.Indices.Values)
				{
					// 1. Find all affected indices
					if (index.Fields.Contains(fieldName))
					{
						// 2. Delete row from index
						index.Delete(this);
					}
				}

				// 3. Update field
				Values[FieldNames.IndexOf(fieldName)] = value;

				foreach (RCTableIndex index in Table.Indices.Values)
				{
					if (index.Fields.Contains(fieldName))
					{
						 // 4. Add to all affected indices
						 index.Add(this);
					}
				}
			}
		}

		public int CompareTo(object obj)
		{
			return CurrentIndexValue.CompareTo(((RCTableRow)obj).CurrentIndexValue);
		}
	}
}