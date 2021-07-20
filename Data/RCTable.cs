using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	public class RCTable : IEnumerable<RCTableRow>
	{

		public static List<string> FieldNames;
		public static List<RCTableRow> Rows;

		public RCTable()
		{
			System.Console.WriteLine("RCTable");

			FieldNames = new List<string>();
		}

		public RCTableRow AddNewRow()
        {
			RCTableRow newRow;
			// Only add rows after a table structure has been defined
            if (FieldNames.Count > 0)
            {

				newRow = new RCTableRow(FieldNames);
            }
            else
            {
				newRow = null;
            }

			return newRow;

        }

		public void AddField(string name)
        {
			FieldNames.Add(name);
        }

		IEnumerator<RCTableRow> IEnumerable<RCTableRow>.GetEnumerator()
        {
            return (IEnumerator<RCTableRow>)GetEnumerator();
        }

        public RCTableEnum GetEnumerator()
        {
            return new RCTableEnum(Rows);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Rows).GetEnumerator();
        }
    }

	public class RCTableEnum : IEnumerator<RCTableRow>
    {
		public List<RCTableRow> Rows;
		int position = -1;
        private bool disposedValue;

        public RCTableEnum(List<RCTableRow> rows)
        {
			Rows = rows;
        }

		public bool MoveNext()
        {
			position++;
			return (position < Rows.Count);
        }

		public void Reset()
        {
			position = -1;
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