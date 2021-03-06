using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	// JNR 2021-07-20
	// Table Index using a Red-black tree
	public class RCTableIndex
	{

		public IndexType Type;
		public string[] Fields;
		public RCTable Table;
		public RCRBTree<RCTableRow> CompareTree;
		public string Name;

		public RCTableIndex(RCTable table, string name, IndexType indexType, string[] fields)
		{
			Name = name;
			Type = indexType;
			Fields = fields;
			Table = table;
			CompareTree = new RCRBTree<RCTableRow>();
		}

		public void CreateIndex()
		{
			foreach (RCTableRow row in Table.Rows)
			{
				Add(row);
			}
		}

		public void Add(RCTableRow row)
		{
			if (CompareTree.Seek(row) && Type == IndexType.StronglyUnique)
			{
				// Error... no duplicates allowed
			}
			else
			{
				CompareTree.Add(row);
			}
		}

		public void Delete(RCTableRow row)
		{
			CompareTree.Delete(row);
		}
	}
}
