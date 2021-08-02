using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	// JNR 2021-07-19
	// Table
	public class RCTable : IEnumerable<RCTableRow>
	{

		public List<string> FieldNames;
		public List<RCTableRow> Rows;
		public Dictionary<string, RCTableIndex> Indices;
		public string CurrentIndex;
		public RCTableRow Current
		{
			get
			{
				return Enumerator.Current;
			}
		}

		private RCTableEnum _enumerator;
		public RCTableEnum Enumerator
		{
			
			get 
			{
				if (_enumerator == null)
				{
					RCTableIndex tmp;
					Indices.TryGetValue(CurrentIndex, out tmp);
					_enumerator = new RCTableEnum(Rows, tmp); 
				}
				return _enumerator;
			}
		}

		public RCTable()
		{
			System.Console.WriteLine("RCTable");

			CurrentIndex = "";
			FieldNames = new List<string>();
			Rows = new List<RCTableRow>();
			Indices = new Dictionary<string, RCTableIndex>();
		}

		public RCTableRow AddNewRow()
		{

			// Only add rows after a table structure has been defined
			if (FieldNames.Count > 0)
			{
				Rows.Add(new RCTableRow(this, FieldNames));
				return Rows[Rows.Count - 1];
			}
			else
			{
				return null;
			}

		}

		public void AddField(string name)
		{
			FieldNames.Add(name);
		}

		public RCTableIndex CreateIndexOn(string indexName, IndexType indexType, params string[] fields)
		{
			string saveCurrentIndex = CurrentIndex;
			CurrentIndex = indexName;
			if (Indices.ContainsKey(indexName)) 
			{
				Indices[indexName] = new RCTableIndex(this, indexName, indexType, fields);
				Indices[indexName].CreateIndex();
			}
			else
			{
				Indices.Add(indexName, new RCTableIndex(this, indexName, indexType, fields));
				Indices[indexName].CreateIndex();
			}
			CurrentIndex = saveCurrentIndex;
			return Indices[indexName];
		}

		public void SetIndex(string indexName)
		{
			RCTableIndex index;
			CurrentIndex = indexName;
			Indices.TryGetValue(CurrentIndex, out index);
			Enumerator.Index = index;
			Enumerator.Reset();
		}


		public void SetIndex(RCTableIndex index)
		{
			if (Indices.ContainsKey(index.Name))
			{
				CurrentIndex = index.Name;
				Enumerator.Index = index;
				Enumerator.Reset();
			}
			else
			{
				Indices.Add(index.Name, index);
				CurrentIndex = index.Name;
				Enumerator.Index = index;
				Enumerator.Reset();
			}
		}
		public void CreateAndSetIndex(string indexName, IndexType indexType, params string[] fields)
		{
			SetIndex(CreateIndexOn(indexName, indexType, fields));
		}

		public bool FindFirst(RCTableIndex index, params IComparable[] searchValues)
		{
			Enumerator.Index = index;
			Enumerator.Reset();
			bool exists = false;
			while (!exists && Enumerator.MoveNext())
			{
				bool allMatch = true;
				for (int i = 0; i < searchValues.Length; i++)
				{
					if (searchValues[i].CompareTo(Enumerator.Current[index.Fields[i]]) != 0)
					{
						allMatch = false;
						break;
					}
				}

				if (allMatch)
				{
					exists = true;
				}
			}
			return exists;
		}

		public bool FindFirst(params IComparable[] searchValues)
		{
			RCTableIndex currentIndex;
			Indices.TryGetValue(CurrentIndex, out currentIndex);
			return FindFirst(currentIndex, searchValues);
		}

		public bool FindNext(RCTableIndex index, params IComparable[] searchValues)
		{
			Enumerator.Index = index;
			bool exists = false;
			while (!exists && Enumerator.MoveNext())
			{
				bool allMatch = true;
				for (int i = 0; i < searchValues.Length; i++)
				{
					if (searchValues[i].CompareTo(Enumerator.Current[index.Fields[i]]) != 0)
					{
						allMatch = false;
						break;
					}
				}

				if (allMatch)
				{
					exists = true;
				}
			}
			return exists;
		}

		public bool FindNext(params IComparable[] searchValues)
		{
			RCTableIndex currentIndex;
			Indices.TryGetValue(CurrentIndex, out currentIndex);
			return FindNext(currentIndex, searchValues);
		}

		public IEnumerable<RCTableRow> Search(RCTableIndex index, params IComparable[] searchValues)
		{
			RCRBTree<RCTableRow> rows = new RCRBTree<RCTableRow>();
			bool addRow;
			foreach (RCTableRow row in index.CompareTree)
			{
				addRow = true;
				for (int i = 0; i < searchValues.Length; i++)
				{
					if (searchValues[i].CompareTo(row[index.Fields[i]]) != 0)
					{
						addRow = false;
						break;
					}
				}

				if (addRow)
				{
					rows.Add(row);
				}
				else if (rows.Count != 0)
				{
					// All of the search values should be together, so if we've already found some
					// and we just didn't find one, we should have all of them, so save some time
					return rows;
				}
			}
			return rows;
		}

		public IEnumerable<RCTableRow> Search(params IComparable[] searchValues)
		{
			RCTableIndex currentIndex;
			Indices.TryGetValue(CurrentIndex, out currentIndex);
			return Search(currentIndex, searchValues);
		}

		public void ConnectDB()
		{

		}

		public void DisconnectDB()
		{

		}

		public void Commit()
		{

		}

		public void Select()
		{

		}

		public void FetchAll()
		{

		}


		IEnumerator<RCTableRow> IEnumerable<RCTableRow>.GetEnumerator()
		{
			return (IEnumerator<RCTableRow>)GetEnumerator();
		}

		public RCTableEnum GetEnumerator()
		{
			return Enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)Rows).GetEnumerator();
		}
	}

	public class RCTableEnum : IEnumerator<RCTableRow>
	{
		public List<RCTableRow> Rows;
		public RCTableIndex Index;
		public RCRBTreeEnumerator<RCTableRow> RCRBEnumerator;
		int position = -1;
		private bool disposedValue;

		public RCTableEnum(List<RCTableRow> rows, RCTableIndex index)
		{
			Rows = rows;
			Index = index;
			if (Index != null)
			{
				RCRBEnumerator = (RCRBTreeEnumerator<RCTableRow>)Index.CompareTree.GetEnumerator();
			}
		}

		public bool MoveNext()
		{
			if (Index == null)
			{
				position++;
				return (position < Rows.Count);
			}
			else
			{
				if (Index != null && RCRBEnumerator == null)
				{
					RCRBEnumerator = (RCRBTreeEnumerator<RCTableRow>)Index.CompareTree.GetEnumerator();
				}
				return RCRBEnumerator.MoveNext();
			}
			
		}

		public void Reset()
		{
			if (Index == null)
			{
				position = -1;
			}
			else
			{
				if (Index != null && RCRBEnumerator == null)
				{
					RCRBEnumerator = (RCRBTreeEnumerator<RCTableRow>)Index.CompareTree.GetEnumerator();
				}
				RCRBEnumerator.Reset();
			}
			
		}


		public RCTableRow Current
		{
			get
			{
				if (Index == null)
				{
					try
					{
						return Rows[position];
					}
					catch (IndexOutOfRangeException)
					{
						throw new InvalidOperationException();
					}
				}
				else
				{
					if (RCRBEnumerator == null)
					{
						RCRBEnumerator = (RCRBTreeEnumerator<RCTableRow>)Index.CompareTree.GetEnumerator();
					}
					return RCRBEnumerator.Current;
				}
			}
		}

		object System.Collections.IEnumerator.Current => throw new NotImplementedException();

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects)
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~RCTableEnum()
		// {
		//	 // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//	 Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

}