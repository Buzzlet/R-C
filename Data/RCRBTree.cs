using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

/*
  A red-black tree is a binary search tree with the following properties:
  1. Each node is either red or black.
  2. All NIL leaves are considered black.
  3. If a node is red, then both its children are black.
  4. Every path from a given node to any of its descendant NIL leaves goes
	 through the same number of black nodes.
	 - in other words, all subtrees must be "black-balanced".

  A violation of (3) is called a red-violation, and a violation of (4) is called
  a black-violation.
 */

// Implemented: Add(), Seek(), graphviz dot visualization
// TODO: Delete(), Enumerators

namespace R_C.Data
{
	// JNR / CJR 2021-07-20
	// Red-black Tree
	// Inserts, deletion, tree rearrangement, recoloring in O(logn)
	public class RCRBTree<T> : IEnumerable<T> where T: IComparable
	{
		public int Count = 0;
		public RCRBTreeNode<T> root;
		public RCRBTree()
		{

		}

		public void Add(T addVal)
		{
			// Find target parent node (if it doesn't exist, insert as root) and
			// direction
			RCRBTreeNode<T> N = root;
			int dir = 0;
			if(N == null)
			{
				Count += 1;
				root = new RCRBTreeNode<T>(addVal);
				return;
			}

			// cmp < 0 means addVal precedes N, 0 means same position, > 0 means
			// succeeds.
			while(true)
			{
				int cmp = addVal.CompareTo(N.value);
				if(cmp == 0) { return; }
				dir = (cmp < 0) ?
					RCRBTreeNode<T>.PREDECESSOR : RCRBTreeNode<T>.SUCCESSOR;
				if(N.children[dir] == null) { break; }
				N = N.children[dir];
			}

			// Insert node there
			RCRBTreeNode<T> addNode = new RCRBTreeNode<T>(addVal);
			addNode.parent = N;
			addNode.isRed = true;
			N.children[dir] = addNode;

			// Do insert-rebalance loop

			N = addNode;

			while(true)
			{
				// P: parent of N
				RCRBTreeNode<T> P = N.parent;
				// Conditions at this point:
				// 1. N is red
				// 2. There are no black-violations, since we entered this
				//	method without any and we haven't added or removed any
				//	black nodes
				// 3. If there is a red-violation, it is only between N and P
				// 4. N is a valid red-black tree, though P may not be due to
				//	a possible N-->P red-violation

				// N is root, and N is a valid red-black tree, we're done
				if(P == null) { Count += 1; return; }
				// G: grandparent of N, U: uncle of N
				RCRBTreeNode<T> G = P.parent, U = null;
				int grandDir;
				dir = P.childDir(N);

				// Only possible violation at present is a red-violation between
				// N and P. Without that, tree is a valid red-black tree.
				if(!P.isRed) { Count += 1; return; }

				// Identify which of the three actionable cases it is:
				// 1. Parent is root (G is null)
				if(G == null)
				{
					// Parent is root, N is red, just make sure root is
					// black. This won't introduce a black violation because all
					// paths go through the root, so the black-height is
					// increased equally in all of them.
					P.isRed = false;
					Count += 1;
					return;
				}
				// hereafter G != null and P is red

				grandDir = G.childDir(P);
				U = G.children[1 - grandDir];

				// 2. Parent and Uncle are red, grandparent is black
				if(U != null && U.isRed && !G.isRed)
				{
					// May introduce a red violation between G and P or between
					// G.parent and G
					G.isRed = true;
					// Removes G --> P red violation if it exists, and the P -->
					// N red violation we started out with. Doesn't introduce
					// any black violation because both sides are affected
					// equally, with one more black node in all paths.
					P.isRed = false;
					U.isRed = false;
					// Now the only possible remaining violation is a red
					// violation between G.parent and G
					N = G;
					// Which leaves loop condition (4) intact, ready for the
					// next iteration
					continue;
				}

				// 3. Parent is red, uncle is black or nonexistent, grandparent
				// is black
				if(!(U != null && U.isRed) && !G.isRed)
				{
					// Do tree rotation to make either N or P the new root of
					// G's current children (and G). Which one can be made the
					// new root depends on which one is "closer to" G in
					// comparison order (that is, its closer predecesssor or
					// successor). If N < P < G, then P is closer to G. If P < N
					// < G, then N is closer to G. Likewise with the opposite
					// comparison, >. Rather than doing these comparisons, we
					// can make use of the fact that we are working with a
					// binary search tree, and let the ancestry directions
					// (e.g. dir and grandDir) tell us this info. If grandDir ==
					// dir, then P is closer. Otherwise, N is closer.
					if(grandDir == dir)
					{
						// P should be made the new root of {N, G, U} and their
						// non-P descendants.

						// See
						// https://en.wikipedia.org/wiki/File:Red-black_tree_insert_case_D1rot.svg
						// for visual of what is happening

						// The key insight of tree rotations is that A < B is
						// the same as B > A. That is, if A is fit to be a
						// left-child of B, then B is also fit to be a
						// right-child of A.
						// Works like so:
						// - Let S be the sibling of N (may be null)
						// - Let GG be G.parent (may be null)
						// - Make P.children[1-dir] = G (replacing S)
						// - Make G.parent = P
						// - Make P.parent = GG
						// - Make GG.children[GG.childDir(G)] = P
						// - Make G.children[grandDir] = S
						// - S != null && S.parent = G
						RCRBTreeNode<T> S = P.children[1-dir];
						RCRBTreeNode<T> GG = G.parent;
						if(GG == null) { root = P; }
						else { GG.children[GG.childDir(G)] = P; }
						P.parent = GG;
						G.parent = P;
						P.children[1-dir] = G;
						G.children[grandDir] = S;
						if(S != null) { S.parent = G; }
						// To summarize what has changed:
						// - P is now the direct child of GG, and has as its
						//   children N and G. G has adopted S in place of P.
						// - Paths through N now have one less black node they
						//   go through than before, while paths through U and S
						//   have the same amount
						// - The red-violation between P and N remains, but now
						//   there is also a black-violation caused by an
						//   imbalance in black-height of 1 between N and U.
						// - A red-violation may exist between GG and P
						// - We can solve the black-violation and the first
						//   red-violation by coloring N black, but there could
						//   still be a red violation between GG and P, and we'd
						//   have to loop with N = P.
						// - Instead, we can color P black and G red. This works
						//   because we know G's children are black.
						P.isRed = false;
						G.isRed = true;
						// there are now no violations.
						Count += 1;
						return;
					}
					else
					{
						// N should be made the new root of {N, G, U} and their
						// non-N descendants

						// Great-grandparent of N
						RCRBTreeNode<T> GG = G.parent;
						// Inner child ("towards G") of N
						RCRBTreeNode<T> I = N.children[1-grandDir];
						// Outer child ("away from G") of N
						RCRBTreeNode<T> O = N.children[grandDir];
						if(GG == null) { root = N; }
						else { GG.children[GG.childDir(G)] = N; }
						N.parent = GG;
						N.children[1-grandDir] = G;
						G.parent = N;
						N.children[grandDir] = P;
						P.parent = N;
						P.children[dir] = O;
						if(O != null) { O.parent = P; }
						G.children[grandDir] = I;
						if(I != null) { I.parent = G; }
						// Now paths through P have one less black node than
						// paths through its new sibling, G, and the
						// red-violation between P and N still exists (though
						// now N is P's parent). The situation is now the same
						// as in the grandDir == dir case above, but with N and
						// P swapped around.
						N.isRed = false;
						G.isRed = true;
						// there are now no violations.
						Count += 1;
						return;
					}
				}
				// This should never be reached
			}
		}

		public void Delete(T deleteVal)
		{
			Count -= 1;
		}

		public bool Seek(T searchVal)
		{
			RCRBTreeNode<T> N = root;
			while(N != null)
			{
				int cmp = searchVal.CompareTo(N.value);
				if(cmp == 0) { return true; }
				int dir = (cmp < 0) ? RCRBTreeNode<T>.PREDECESSOR : RCRBTreeNode<T>.SUCCESSOR;
				N = N.children[dir];
			}
			return false;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new RCRBTreeEnumerator<T>(this);
		}

		System.Collections.IEnumerator
			System.Collections.IEnumerable.GetEnumerator()
		{
			return new RCRBTreeEnumerator<T>(this);
		}

		public void WriteGraphviz()
		{
			WriteGraphviz(System.Console.Out);
		}

		public void WriteGraphviz(TextWriter writer)
		{
			writer.WriteLine("digraph RBTree {");
			if(root != null) { WriteGraphviz(root, 0, writer); }
			writer.WriteLine("}");
		}

		public int WriteGraphviz(RCRBTreeNode<T> node,
								 int counter,
								 TextWriter writer)
		{
			string nodeStr = counter + "[label=\"";

			// JNR - A little nicer than just seeing the type on the graph...
			if (node.value.GetType() == typeof(RCTableRow))
			{
				RCTableRow row = ((RCTableRow)Convert.ChangeType(node.value, typeof(RCTableRow)));
				nodeStr += "{";
				foreach (string field in row.Table.Indices[row.Table.CurrentIndex].Fields)
				{
					nodeStr += row[field] + ",";
				}
				nodeStr += "}";
			}
			else
			{
				nodeStr += node.value;
			}
			nodeStr += "\"" + (node.isRed ? ", color=\"red\"" : "") + "];";

			writer.WriteLine(nodeStr);
			int nextCounter = counter + 1;

			if(node.children[RCRBTreeNode<T>.PREDECESSOR] != null)
			{
				int newNext =
					WriteGraphviz(node.children[RCRBTreeNode<T>.PREDECESSOR],
								  nextCounter,
								  writer);
				writer.WriteLine(counter + " -> " + nextCounter);
				nextCounter = newNext;
			}
			if(node.children[RCRBTreeNode<T>.SUCCESSOR] != null)
			{
				int newNext =
					WriteGraphviz(node.children[RCRBTreeNode<T>.SUCCESSOR],
								  nextCounter,
								  writer);
				writer.WriteLine(counter + " -> " + nextCounter);
				nextCounter = newNext;
			}
			return nextCounter;
		}

		public void ShowGraphviz()
		{
			Process xdot = new Process();
			xdot.StartInfo.UseShellExecute = false;
			xdot.StartInfo.RedirectStandardInput = true;
			xdot.StartInfo.FileName = "xdot";
			xdot.StartInfo.Arguments = "-";
			xdot.Start();
			WriteGraphviz(xdot.StandardInput);
			xdot.StandardInput.Close();
			xdot.StandardInput.Dispose();
			xdot.WaitForExit();
			xdot.Close();
			xdot.Dispose();
		}
	}

	public class RCRBTreeEnumerator<T> : IEnumerator<T> where T : IComparable
	{
		RCRBTree<T> tree;
		RCRBTreeNode<T> N;
		const int
			PREDECESSOR = RCRBTreeNode<T>.PREDECESSOR,
			SUCCESSOR = RCRBTreeNode<T>.SUCCESSOR;
		public RCRBTreeEnumerator(RCRBTree<T> tree)
		{
			this.tree = tree;
			this.N = null;
		}

		public T Current { get { return N.value; } }
		object System.Collections.IEnumerator.Current
		{ get { return N.value; } }

		// Only resources this enumerator uses is memory...
		public void Dispose() { }

		// root must be non-null
		static RCRBTreeNode<T> MinNode(RCRBTreeNode<T> root, int left)
		{
			while (root.children[left] != null)
			{
				root = root.children[left];
			}
			return root;
		}

		static RCRBTreeNode<T> MinNode(RCRBTreeNode<T> root)
		{
			return MinNode(root, PREDECESSOR);
		}

		static RCRBTreeNode<T> MaxNode(RCRBTreeNode<T> root)
		{
			return MinNode(root, SUCCESSOR);
		}

		public void Reset()
		{
			N = null;
		}

		// in normal usage, 'left' and 'right' are PREDECESSOR and SUCCESSOR,
		// respectively. They can be inverted to use this as a Predecessor
		// function.
		static RCRBTreeNode<T> Successor(RCRBTreeNode<T> N, int left, int right)
		{
			// The successor of N is the node M such that N < M and for all
			// nodes Q in the tree, Q ≤ N or Q ≥ M.

			// A binary tree is defined by the property that all
			// left-descendants of a node are less than it, and all
			// right-descendants of a node are greater than it.

			// Equivalently, the right-parent of a subtree is always greater
			// than every node in it, and the left-parent of a subtree is
			// always less than every node in it.

			// Important to note is that every subtree is a contiguous
			// range. That is, for any subtree N, minVal(N) ≤ M ≤ maxVal(N)
			// implies that M is in the subtree N. My proof of this isn't
			// very concise, so I'll leave it out.
			if (N.children[right] != null)
			{
				// down-right, then as far down-left as possible
				N = MinNode(N.children[right], left);
				return N;
			}
			else
			{
				// as far up-left as possible, then up-right
				RCRBTreeNode<T> newNode = N;
				while (newNode.parent != null)
				{
					// Invariant: At this point, N is the greatest
					// descendant of the subtree with root newNode
					if (newNode.parent.childDir(newNode) == left)
					{
						// 1. newNode's parent is greater than the entire
						//    subtree rooted at newNode
						// 2. N is the greatest descendant of the subtree
						//    rooted at newNode
						// 3. The subtree rooted at newNode.parent is a
						//    contiguous range, so any node whose value is
						//    within its bounds is a member of it. No node
						//    that is a member of it exists between N and
						//    newNode.parent, so no node between N and
						//    newNode.parent exists at all. Therefore
						//    newNode.parent is N's successor.
						N = newNode.parent;
						return N;
					}

					// newNode.parent is the left-parent, so N is also the
					// greatest descendant of the subtree with root
					// newNode.parent.
					newNode = newNode.parent;
				}
				// No successor! Traversal is done.
				return null;
			}

		}

		static RCRBTreeNode<T> Successor(RCRBTreeNode<T> N)
		{
			return Successor(N, PREDECESSOR, SUCCESSOR);
		}

		static RCRBTreeNode<T> Predecessor(RCRBTreeNode<T> N)
		{
			return Successor(N, SUCCESSOR, PREDECESSOR);
		}

		public bool MoveNext()
		{
			// Initial case
			if (N == null)
			{
				if (tree.root != null)
				{
					N = MinNode(tree.root);
					return true;
				}
				else
				{
					// Tree is empty
					return false;
				}
			}

			RCRBTreeNode<T> next = Successor(N);
			if (next != null)
			{
				N = next;
				return true;
			}
			return false;
		}

		// Seeks to the minimum element within the range, or the maximum element
		// if there is no element within range
		public void SeekMin(
			// A comparator that specifies a range of T's, giving > 0 if the
			// range is above the object, 0 if the range is around the object,
			// and < 0 if the range is below the object.
			IComparable<T> comparator
		)
		{
			RCRBTreeNode<T> min = null;
			RCRBTreeNode<T> N = tree.root;
			if(N == null)
			{
				this.N = null;
				return;
			}
			while(N != null)
			{
				int cmp = comparator.CompareTo(N.value);
				if(cmp < 0)
				{
					// Range is below N, go left
					N = N.children[PREDECESSOR];
				}
				else if(cmp > 0)
				{
					// Range is above N, go right
					N = N.children[SUCCESSOR];
				}
				else
				{
					// Range is around N, go left
					min = N;
					N = N.children[PREDECESSOR];
				}
			}
			if(min != null)
			{
				// In keeping with enumerator conventions, set this.N to
				// the value such that it becomes N after one initial
				// invocation of MoveNext(). Note that this may set
				// this.N to null, but it still works out.
				this.N = Predecessor(min);
			}
			else
			{
				// no elements are within range, seek to end (if we're here then
				// tree.root isn't null)
				this.N = MaxNode(tree.root);
			}
		}
	}
}
