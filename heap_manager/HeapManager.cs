using System;
using System.Collections.Generic;

namespace BinaryHeap
{

    /// <summary>
    /// A utility class that allows one to implement a binary heap on top of any
    /// class that implements the `IList` interface.
    ///
    /// The usefulness of this derives from the fact that no user implementation
    /// is needed: as soon as the `IList` interface is provided, heap operations
    /// can be used out of the box.
    /// </summary>
    public partial class HeapManager
    {

        /// <summary>
        /// Given
        /// <list type = "bullet">
        /// <item>
        ///     <c>L list</c>, where <c>L</c> implements <c>IList&lt;Item&gt;</c>
        /// </item>
        /// <item>
        ///     <c>Comparison&lt;Item&gt; compare</c>,
        ///     a total preorder relationship on <c>Item</c>
        /// </item>
        /// </list>
        /// it rearranges the items of <c>list</c>
        /// so that the heap property with respect to <c>compare</c>
        /// is satisfied for <c>list</c> after <c>Heapify</c> terminates its execution.
        /// </summary>
        /// 
        /// <remarks>
        /// By heap property with respect to <c>compare</c>, we mean
        /// that, for every valid index <c>idx</c>, it holds
        /// <br/>
        /// <c>compare(list[(idx - 1) / 2], list[idx]) &gt;= 0</c>
        /// </remarks>
        public static void Heapify<L, Item>(ref L list, Comparison<Item> compare)
        where L : IList<Item>
        {
            int last_full_parent_idx = LastFullParentIndex(list.Count);
            for (int pivot = last_full_parent_idx + 1; pivot-- > 0;)
            {
                int idx = pivot;
                // While one of the children of `idx` in `list` contains
                // an item that is bigger than the item in `idx`,
                // we swap them, and store the index we swapped with in
                // `new_idx`. If no swap occurred, `new_idx == idx`.
                // Otherwise, we repeat with `idx = new_idx`.
                var new_idx = CompareSwapWithChildren(ref list, compare, idx);
                while (new_idx != idx && new_idx <= last_full_parent_idx)
                {
                    idx = new_idx;
                    new_idx = CompareSwapWithChildren(ref list, compare, idx);
                }
            }
            // The previous for loop does not touch list[list.Count - 1]
            // in case it is an only child. Here we fix that.
            if (list.Count % 2 == 0) SiftUp(ref list, compare, list.Count - 1);
        }

        /// <summary>
        /// Given
        /// <list type = "bullet">
        /// <item>
        ///     <c>L list</c>, where <c>L</c> implements <c>IList&lt;Item&gt;</c>
        /// </item>
        /// <item>
        ///     <c>Comparison&lt;Item&gt; compare</c>,
        ///     a total preorder relationship on <c>Item</c>
        /// </item>
        /// </list>
        /// computes if <c>list</c> satisfies the heap property with
        /// respect to <c>compare</c>
        /// </summary>
        /// 
        /// <remarks>
        /// By heap property with respect to <c>compare</c>, we mean
        /// that, for every valid index <c>idx</c>, it holds
        /// <br/>
        /// <c>compare(list[(idx - 1) / 2], list[idx]) &gt;= 0</c>
        /// </remarks>
        public static bool IsHeap<L, Item>(ref L list, Comparison<Item> compare)
        where L : IList<Item>
        {
            for (int idx = list.Count - 1; idx > 0; idx--)
                if (compare(list[Parent(idx)], list[idx]) < 0) return false;
            return true;
        }

        /// <summary>
        /// Given a list, a comparison operator, and an item, and assuming that
        /// - the comparison operator encodes a total preorder on Item
        /// - the list satisfies the heap property with respect to the operator
        /// the function returns an index such that
        /// `compare(list[index], item) == 0`,
        /// if such an index exists. Otherwise, the value -1 is returned.
        ///
        /// The performance of this method is linear in `list.Count`.
        /// The only slight optimization exploits the heap property to rougly
        /// halve the number of comparisons needed.
        /// However, keep in mind that this will have the same computational
        /// complexity as a normal linear search in a generic list.
        /// </summary>
        public static int FindEqItem<L, Item>(ref L list, Comparison<Item> compare, Item item)
        where L : IList<Item>
        {
            if (list.Count == 0) return -1;

            var stack = new Stack<int>();
            stack.Push(0);
            int last_full_parent_idx = LastFullParentIndex(list.Count);
            while (stack.Count != 0)
            {
                int parent = stack.Pop();
                int cmp = compare(list[parent], item);
                if (cmp > 0)
                {
                    if (parent <= last_full_parent_idx)
                    {
                        stack.Push(Child1(parent));
                        stack.Push(Child2(parent));
                    }
                    else if (Child1(parent) < list.Count)
                        stack.Push(Child1(parent));
                }
                else if (cmp == 0) return parent;
            }
            return -1;
        }

        /// <summary>
        /// Given
        /// <list type = "bullet">
        /// <item>
        ///     <c>Comparison&lt;Item&gt; compare</c>,
        ///     a total preorder relationship on <c>Item</c>
        /// </item>
        /// <item>
        ///     <c>L list</c>, where <c>L</c> implements <c>IList&lt;Item&gt;</c>,
        ///     and where we assume that <c>list</c> satisfies the heap property
        ///     with respect to <c>compare</c>
        /// </item>
        /// <item>
        ///     any valid <c>Item item</c> value
        /// </item>
        /// </list>
        /// 
        /// the function adds <c>item</c> to <c>list</c>, and swaps the items
        /// in <c>list</c> in order to restore the heap property.
        /// </summary>
        /// 
        /// <remarks>
        /// By heap property with respect to <c>compare</c>, we mean
        /// that, for every valid index <c>idx</c>, it holds
        /// <br/>
        /// <c>compare(list[(idx - 1) / 2], list[idx]) &gt;= 0</c>
        /// </remarks>
        public static void Insert<L, Item>(ref L list, Comparison<Item> compare, Item item)
        where L : IList<Item>
        {
            list.Insert(list.Count, item);
            SiftUp(ref list, compare, list.Count - 1);
        }

        // 

        /// <summary>
        /// Given
        /// <list type = "bullet">
        /// <item>
        ///     <c>Comparison&lt;Item&gt; compare</c>,
        ///     a total preorder relationship on <c>Item</c>
        /// </item>
        /// <item>
        ///     <c>L list</c>, where <c>L</c> implements <c>IList&lt;Item&gt;</c>,
        ///     and where we assume that <c>list</c> satisfies the heap property
        ///     with respect to <c>compare</c>
        /// </item>
        /// </list>
        /// 
        /// this function removes `list[0]` from the list,
        /// rearranges the remaining items so that the heap property is restored
        /// </summary>
        /// 
        /// <remarks>
        /// If the list is empty, the function panics.
        /// the caller must ensure this does not happen.
        /// </remarks>
        /// 
        /// <returns>
        /// The item that was removed from the list, i.e. the item that
        /// was stored in <c>list[0]</c> when the function was called.
        /// </returns>
        public static Item PopMax<L, Item>(ref L list, Comparison<Item> compare)
        where L : IList<Item>
        {
            if (list.Count == 0) throw new ArgumentOutOfRangeException("Cannot call HeapManager.PopMax on an empty collection.");
            var ret = list[0];
            // We replace the base with the last element,
            list[0] = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            // We move down the head until the heap property is restored.
            if (list.Count != 0) SiftDown(ref list, compare, 0);
            return ret;
        }

        // Given a list, a comparison operator,an index, and an item, and assuming
        // - the comparison operator encodes a total preorder
        // - the list satisfies the heap property with respect to the comparison
        //   operator
        // this function replaces the `index`-th item of the list,
        // and then swaps items in order to restore the heap property.
        // The item that was replaced is then returned.
        //
        // If the index is out of range, the function panics.
        // It is up to the caller to ensure this does not happen.
        public static Item ReplaceAtIndex<L, Item>(
            ref L list,
            Comparison<Item> compare,
            int index,
            Item new_item
        ) where L : IList<Item>
        {
            var ret = list[index];
            list[index] = new_item;
            // Attempt to move the item upwards to restore heap property.
            // If SiftUp didn't move the item upwards, try moving it down.
            if (!SiftUp(ref list, compare, index))
                SiftDown(ref list, compare, index);
            return ret;
        }

        // Given a list, a comparison operator,an item to be found,
        // an item to replace it with, and assuming
        // - the comparison operator encodes a total preorder
        // - the list satisfies the heap property with respect to the comparison
        //   operator
        // this function finds an `index` such that
        // `compare(item_equivalent_to, list[index]) == 0`,
        // replaces it with `new_item`,
        // and then swaps items in order to restore the heap property.
        //
        // If such index exists, (true, "the item that was replaced") is returned,
        // otherwise, (false, default) is returned.
        //
        // This variant of ReplaceKey is inherently slower than the one in which
        // the index is explicitly provided, because it finds the appropriate
        // index with the FindItem method, which takes linear time in list.Count
        // to execute.
        public static (bool, Item?) ReplaceItem<L, Item>(
            ref L list,
            Comparison<Item> compare,
            Item item_equivalent_to,
            Item new_item
        ) where L : IList<Item>
        {
            int index = FindEqItem(ref list, compare, item_equivalent_to);
            if (index == -1) return (false, default);
            return (true, ReplaceAtIndex(ref list, compare, index, new_item));
        }

        // Given a list, a sequence of items, and a comparison operator, assuming
        // - the comparison operator is a total preorder on Item
        // - the list satisfies the heap property with respect to the operator
        // the function appends the elements of the sequence to the list,
        // and swaps the elements around in order to restore the heap property.
        //
        // The complexity of this operation is linear in the length of the
        // sequence of added elements, plus the logarithm of the initial length of the list.
        public static void Extend<L, En, Item>(ref L list, Comparison<Item> compare, ref En extension)
        where L : IList<Item>
        where En : IEnumerable<Item>
        {
            int initial_length = list.Count;
            foreach (Item item in extension) list.Insert(list.Count, item);

            // Here we could solve the situation with Heapify, but we can optimize a bit.
            // We already know that list[0..initial_length] satisfies the heap property.
            // We can exploit this, and do as follows:
            //
            // - for the indices bigger than or equal to initial_lenght, we can
            //   do as we do with Heapify, while also keeping track of what items
            //   with index smaller than initial_length are swapped in the process.
            // - for the indices smaller than initial_length, we can use the list of
            //   touched items that we built while "heapifying" the new part of the list
            //   and only control if those need to be swapped to restore the heap property.

            if (list.Count == initial_length) return;
            if (initial_length == 0)
            {
                Heapify(ref list, compare);
                return;
            }

            // now we know that initial_length > 0, and that list.Count > initial_length.
            // useful to avoid some non-0 checks.

            var last_full_parent_idx = LastFullParentIndex(list.Count);

            int pivot = last_full_parent_idx;
            // We perform everything as in Heapify for sub-heaps that have
            // a root placed after the start of the initial sequence.
            for (; pivot >= initial_length; pivot--)
            {
                int idx = pivot;
                // While one of the children of `idx` in `list` contains
                // an item that is bigger than the item in `idx`,
                // we swap them, and store the index we swapped with in
                // `new_idx`. If no swap occurred, `new_idx == idx`.
                // Otherwise, we repeat with `idx = new_idx`.
                var new_idx = CompareSwapWithChildren(ref list, compare, idx);
                while (new_idx != idx && new_idx <= last_full_parent_idx)
                {
                    idx = new_idx;
                    new_idx = CompareSwapWithChildren(ref list, compare, idx);
                }
            }

            var batch = new List<int> { };
            var parent_il = Parent(initial_length - 1);

            // For the sub-heaps that have a root bigger than parent_il, and
            // smaller than min(last_full_parent_idx, initial_length),
            // we start heapifying while also keeping track of what items we move.
            for (; pivot >= parent_il; pivot--)
                HeapifyStepAndTag(
                    ref list, compare, last_full_parent_idx,
                    pivot, batch
                );
            pivot = parent_il;

            // the elements in batch are ordered in decreasing order.
            // We know that e1 = batch[0] < initial_length
            // and e2 = batch[batch.Count - 1] > parent_il
            // satisfy
            // Parent(e1) <= Parent(initial_length - 1)
            //            == parent_il < e2
            //
            // Resursively, if batch is such that,
            // for every e1, and e2 in batch, it holds
            // Parent(e1) <= e2, then, defining
            // ```
            // new_batch = List{Parent(item) for item in batch}
            // ```
            // for every i1 and i2 in new_batch,
            // there is some e1 and e2 such that
            // i1 = Parent(e1) and i2 = Parent(e2).
            // Then,
            //
            // Parent(i1) == Parent(Parent(e1))
            //            == (Parent(e1) - 1) / 2
            //            <= (e2 - 1) / 2
            //            == Parent(e2) == i2
            //
            // so that the property is preserved
            // if we apply Parent to each element of batch.
            //
            // This way, the concatenation of batch and new_batch is
            // still a decreasing sequence.

            // This will contain (some of) the parents of the items in batch.
            var new_batch = new List<int> { };
            while (batch.Count > 0)
            {
                // we know that batch[0] > 0,
                // and the explanation comment above makes us sure that
                // ```
                // foreach(int idx in batch) {
                //     Assert(Parent(batch[0]) <= idx);
                // }
                // ```
                // And if batch[0] > 0, then Parent(batch[0]) >= 0.
                //
                // This means that we are sure we will not have any
                // out of bounds access to `list` if we use the items in
                // batch as indices.
                foreach (int idx in batch)
                {
                    // skip if we visited the index before
                    // (keep in mind that batch is a decreasing list)
                    // (also, keep in mind that different indices can
                    // have the same parent)
                    if (pivot <= idx) continue;
                    // then we update the pivot, and heapify it.
                    // if heapifying the pivot swaps things, then
                    // HeapifyStepAndTag adds Parent(pivot)
                    // to new_batch
                    pivot = idx;
                    HeapifyStepAndTag(
                        ref list, compare, last_full_parent_idx,
                        pivot, new_batch
                    );
                }
                // Now we visited and heapified all the indices in batch,
                // and new_batch contains the indices that need checking
                // to ensure they satisfy the heap property.
                // Therefore, we swap the two lists, and then
                // we empty new_batch, to restore the initial invariants
                // of this while loop
                (batch, new_batch) = (new_batch, batch);
                new_batch.Clear();
            }

            // In case the last item is an only child, sift it up.
            // Indeed, it was never compared against anything so far.
            if (list.Count % 2 == 0) SiftUp(ref list, compare, list.Count - 1);
        }

        static void HeapifyStepAndTag<L, Item>(
            ref L list, Comparison<Item> compare, int last_full_parent_idx,
            int pivot, List<int> next_batch
        ) where L : IList<Item>
        {
            var new_idx = CompareSwapWithChildren(ref list, compare, pivot);
            if (new_idx == pivot) return;
            // The next batch of items to process contains Parent(pivot)
            // because list[pivot] was swapped with something else, and we
            // must check if the heap property needs fixing for the parent.
            next_batch.Add(Parent(pivot));
            while (new_idx != pivot && new_idx <= last_full_parent_idx)
            {
                pivot = new_idx;
                new_idx = CompareSwapWithChildren(ref list, compare, pivot);
            }
        }
    }
}
