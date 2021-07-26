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
    public class RCRBTree<T> where T: IComparable
    {
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
				//    method without any and we haven't added or removed any
				//    black nodes
				// 3. If there is a red-violation, it is only between N and P
				// 4. N is a valid red-black tree, though P may not be due to
				//    a possible N-->P red-violation

				// N is root, and N is a valid red-black tree, we're done
				if(P == null) { return; }
				// G: grandparent of N, U: uncle of N
				RCRBTreeNode<T> G = P.parent, U = null;
				int grandDir;
				dir = P.childDir(N);

				// Only possible violation at present is a red-violation between
				// N and P. Without that, tree is a valid red-black tree.
				if(!P.isRed) { return; }

				// Identify which of the three actionable cases it is:
				// 1. Parent is root (G is null)
				if(G == null)
				{
					// Parent is root, N is red, just make sure root is
					// black. This won't introduce a black violation because all
					// paths go through the root, so the black-height is
					// increased equally in all of them.
					P.isRed = false;
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
						return;
					}
				}
				// This should never be reached
			}
		}

		public void Delete(T deleteVal)
		{

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
				nodeStr += ((RCTableRow)Convert.ChangeType(node.value, typeof(RCTableRow))).CurrentIndexValue.Replace("\"", "\\\"");
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
}
