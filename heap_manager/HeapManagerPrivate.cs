using System;
using System.Collections.Generic;

namespace BinaryHeap
{
    /// <summary>
    /// Here we gather the private helper methods of the HeapManager class.
    /// </summary>
    public partial class HeapManager
    {
        /// <summary>
        /// given an index <c>idx</c>, returns its first child in the binary heap
        /// positional convention, i.e. <c>2 * idx + 1</c>
        /// </summary>
        /// 
        /// <remarks>
        /// See also <see cref="Child2">Child2</see>
        /// and <see cref="Parent">Parent</see>
        /// </remarks>
        private static int Child1(int idx)
        {
            return idx << 1 | 1;
        }
        /// <summary>
        /// given an index <c>idx</c>, returns its first child in the binary heap
        /// positional convention, i.e. <c>2 * idx + 2</c>
        /// </summary>
        /// 
        /// <remarks>
        /// See also <see cref="Child1">Child1</see>
        /// and <see cref="Parent">Parent</see>
        /// </remarks>
        private static int Child2(int idx)
        {
            return (idx + 1) << 1;
        }


        /// <summary>
        /// Given an index <c>idx</c>, returns its parent in the binary heap
        /// positional convention, i.e. <c>(idx - 1) / 2</c>
        /// <br/>
        /// For every <c>idx</c> it holds
        /// <list type = "bullet">
        ///   <item> <c>idx == Parent(Child1(idx))</c> </item>
        ///   <item> <c>idx == Parent(Child2(idx))</c> </item>
        /// </list>
        /// <br/>
        /// <font size="15"><b>Notice</b></font>
        /// <br/>
        /// <c>0</c> is the only <c>idx</c> value such that <c>idx == Parent(idx)</c>
        /// <br/>
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// See also <see cref="Child1">Child1</see>
        /// and <see cref="Child2">Child2</see>
        /// </remarks>
        private static int Parent(int idx)
        {
            return (idx - 1) / 2;
        }

        /// <summary>
        /// Given a <c>list</c>, returns the last index <c>i</c> for which
        /// it holds <c>Child1(i) &lt; list.Count</c> and <c>Child2(i) &lt; list.Count</c>.
        /// </summary>
        /// 
        /// <returns>
        /// <c>(list.Count - 1) / 2 - 1</c>
        /// </returns>
        private static int LastFullParentIndex(int length)
        {
            return (length - 1) / 2 - 1;
        }


        /// <summary>
        /// <b>Assumption:</b>
        /// <list type = "bullet">
        /// <item> <c>item1 == list[idx1]</c> </item>
        /// <item> <c>item2 == list[idx2]</c> </item>
        /// <item> <c>idx1 == Parent(idx2)</c> </item>
        /// </list>
        /// 
        /// Compares item1 and item2.
        /// If <c>item1 &lt; item2</c>, then it swaps <c>list[idx1]</c> and <c>list[idx2]</c>.
        /// </summary>
        /// 
        /// <returns>
        /// If <c>list[idx1]</c> and <c>list[idx2]</c> are swapped,
        /// the function returns <c>(idx2, idx1)</c>.
        /// <br/>
        /// Otherwise, it returns <c>(idx1, idx2)</c>
        /// </returns>
        private static (int, int) CompareSwap<L, Item>(
            ref L list,
            Comparison<Item> compare,
            int idx1,
            int idx2,
            Item item1,
            Item item2
        ) where L : IList<Item>
        {
            if (compare(item1, item2) >= 0) return (idx1, idx2);
            (list[idx1], list[idx2]) = (item2, item1);
            return (idx2, idx1);
        }


        /// <summary>
        /// Given
        /// <list type = "bullet">
        /// <item>
        ///     <c>L list</c>, with <c>L</c> implementing <c>IList&lt;Item&gt;</c>
        /// </item>
        /// <item>
        ///     <c>Comparison&lt;Item&gt; compare</c>, that encodes
        ///     a total preorder relationship on <c>Item</c>
        /// </item>
        /// <item>
        ///     <c>int idx</c>, a valid index for <c>list</c>
        /// </item>
        /// </list>
        /// 
        /// Applies <c>CompareSwap</c>  to <c>list[idx]</c> and the biggest one between
        /// <c>list[Child1(idx)]</c> and <c>list[Child2(idx)]</c>.
        /// </summary>
        /// 
        /// <remarks>
        /// This function serves as an optimization for when one needs to do
        /// <br/>
        /// <c>
        /// var c1 = Child1(idx);<br/>
        /// var c2 = Child2(idx);<br/>
        /// var item_idx = list[idx];<br/>
        /// CompareSwap(ref list, compare, idx, c1, item_idx, list[c1]);<br/>
        /// CompareSwap(ref list, compare, idx, c2, item_idx, list[c2]);<br/>
        /// </c>
        /// <br/>
        /// as it allows to spare two index assignments in the case in which
        /// <c>item_idx &lt; list[c1]</c> and <c>list[c1] &lt; list[c2]</c>.
        /// <br/> <br/>
        /// If either <c>Child1(idx)</c> or <c>Child2(idx)</c> are out of range,
        /// an exception is thrown.
        /// <br/>
        /// The caller must ensure this does not happen.
        /// </remarks>
        /// 
        /// <returns>
        /// The index in which <c>list[idx]</c> ends up in.
        /// </returns>
        private static int CompareSwapWithChildren<L, Item>(ref L list, Comparison<Item> compare, int idx)
        where L : IList<Item>
        {
            var c1_idx = Child1(idx);
            var c2_idx = Child2(idx);
            var c1_item = list[c1_idx];
            var c2_item = list[c2_idx];
            var idx_item = list[idx];
            if (compare(c1_item, c2_item) > 0)
                return CompareSwap(ref list, compare, idx, c1_idx, idx_item, c1_item).Item1;
            else
                return CompareSwap(ref list, compare, idx, c2_idx, idx_item, c2_item).Item1;
        }


        /// <summary>
        /// Moves an item up a list, until the heap property is locally satisfied.
        /// </summary>
        /// 
        /// <returns>
        /// <c>true</c> it the item was moved, <c>false</c> otherwise.
        /// </returns>
        private static bool SiftUp<L, Item>(ref L list, Comparison<Item> compare, int idx)
        where L : IList<Item>
        {
            if (idx <= 0) return false;
            int parent = Parent(idx);
            // What follows is equivalent to:
            // While `list[idx]` is bigger than `list[parent]`, we swap them,
            // and then we repeat with `idx = parent; parent = Parent(idx);`.
            //
            // The awkward code structure makes it possible to determine the value
            // returned by the function with just the code path taken.
            // It corresponds to unrolling the first iteration of the while loop,
            // so that the do {...} while (...) loop that follows knows that at
            // least one swap happened.
            if (compare(list[idx], list[parent]) > 0)
            {
                (list[idx], list[parent]) = (list[parent], list[idx]);
                idx = parent;
                do
                {
                    parent = Parent(idx);
                    if (idx == parent) return true;
                    idx = CompareSwap(ref list, compare, parent, idx, list[parent], list[idx]).Item2;
                } while (idx == parent);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Moves an item down a list, until the heap property is locally satisfied.
        /// </summary>
        /// 
        /// <returns>
        /// <c>true</c> it the item was moved, <c>false</c> otherwise.
        /// </returns>
        private static bool SiftDown<L, Item>(ref L list, Comparison<Item> compare, int idx)
        where L : IList<Item>
        {
            // The awkward code structure is such that it is possible to decide the
            // return value without having to update a boolean flag at every loop
            // iteration. The code path taken decides the return value.
            int last_full_parent_idx = LastFullParentIndex(list.Count);
            if (idx <= last_full_parent_idx)
            {
                int new_idx = CompareSwapWithChildren(ref list, compare, idx);
                // if `new_idx == idx`, that means that the heap property is still
                // valid, and that no swaps are needed, so we return false.
                if (new_idx == idx) return false;
                // from now on, we know that at least one swap happened,
                // so we know that we will return true.
                while (new_idx <= last_full_parent_idx)
                {
                    idx = new_idx;
                    new_idx = CompareSwapWithChildren(ref list, compare, idx);
                    // new_idx == idx means we are done because no swap was needed.
                    if (new_idx == idx) return true;
                }
                LastComparison(ref list, compare, new_idx);
                return true;
            }
            // if this code is executed, that means that idx was too big to be a
            // full parent, so we manage the edge case of it having only one child.
            // we return the last comparison output, because it is the only
            // place a swap could have occurred in this code path.
            return LastComparison(ref list, compare, idx);

            // Similar to what would be the "only child" version of CompareSwapWithChildren
            static bool LastComparison(ref L list, Comparison<Item> compare, int idx)
            {
                // if idx has only one child in range,
                // and if the child is bigger, swap them to restore the heap property.
                if (Child2(idx) > list.Count) return false;

                int c1 = list.Count - 1;
                return idx == CompareSwap(
                    ref list, compare, idx, c1, list[idx], list[c1]
                ).Item2;
            }
        }
    }
}