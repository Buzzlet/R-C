using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R_C.Data
{
	// JNR / CJR 2021-07-20
	// Red-black Tree Node
	public class RCRBTreeNode<T> where T: IComparable
	{
		public RCRBTreeNode<T>[] children;
		// It should in theory be possible to implement insert and delete
		// without needing parent references, by just keeping track of the path
		// we took to get to the node in question, but it seems like a hassle
		public RCRBTreeNode<T> parent;
		public T value;
		public bool isRed;
		public const int PREDECESSOR = 0, SUCCESSOR = 1;
		public RCRBTreeNode(T value)
		{
			children = new RCRBTreeNode<T>[2];
			parent = null;
			isRed = true;
			this.value = value;
		}

		// Only call this when you know child is actually a child of this.
		public int childDir(RCRBTreeNode<T> child)
		{
			return children[PREDECESSOR] == child ? PREDECESSOR : SUCCESSOR;
		}
	}
}
