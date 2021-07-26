using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
    // JNR 2021-07-19
    // Table (strings only)
	public class RCTable : IEnumerable<RCTableRow>
	{

		public List<string> FieldNames;
		public List<RCTableRow> Rows;
        public Dictionary<string, RCTableIndex> Indices;
        public string CurrentIndex;
        public RCTableEnum Enumerator
		{
            
			get 
            {
                RCTableIndex tmp;
                Indices.TryGetValue(CurrentIndex, out tmp);
                return new RCTableEnum(Rows, tmp); 
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
            CurrentIndex = indexName;
        }

        public void SetIndex(RCTableIndex index)
		{
            if (Indices.ContainsKey(index.Name))
			{
                CurrentIndex = index.Name;
			}
			else
			{
                Indices.Add(index.Name, index);
                CurrentIndex = index.Name;
            }
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
		int position = -1;
        private bool disposedValue;

        public RCTableEnum(List<RCTableRow> rows, RCTableIndex index)
        {
			Rows = rows;
            Index = index;
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
                // TODO: Go to the next result in the index...
                return false;
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
                // TODO: Set index back to beginning of IEnumerable result
			}
			
        }


		public RCTableRow Current
        {
            get
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
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}