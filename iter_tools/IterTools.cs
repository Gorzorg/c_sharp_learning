using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IterTools
{
    /// <summary>
    /// Iterates over all the partitions of a set of `set_cardinality`
    /// elements. Every partition is encoded as a list of positive numbers
    /// that add up to `set_cardinality`.
    /// </summary>
    public readonly struct Partitions : IEnumerable<List<int>>
    {
        readonly int set_cardinality;
        public Partitions(int set_cardinality)
        {
            this.set_cardinality = set_cardinality;
        }

        /// <summary>
        /// Inner implementation of the enumeration logic.
        /// Written in form of yield loop because it is easier to reason about.
        /// </summary>
        private IEnumerable<List<int>> Enumerator()
        {
            if (set_cardinality < 1) yield break;

            var part = new List<int> { set_cardinality };
            yield return part;
            var to_add_back = 1;
            while (part.Count > 0)
            {
                // We remove all trailing ones from `part`,
                // and we keep their count in `to_add_back`.
                if (part[^1] == 1)
                {
                    part.RemoveAt(part.Count - 1);
                    to_add_back++;
                    continue;
                }

                // When we have a non-1 item as `part.Last()`,
                // we decrement it, and we append `to_add_back` to `part`.
                part[^1]--;
                part.Add(to_add_back);
                to_add_back = 1;

                yield return part;
            }
            yield break;
        }

        IEnumerator<List<int>> IEnumerable<List<int>>.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }
    }

    /// <summary>
    /// Iterates over the partition types of a finite set.
    /// By partition type, we mean the equivalence class of all partitions
    /// up to applying a permutation on the set.
    /// 
    /// All such equivalence classes can be represented by a non-increasing
    /// sequence of positive numbers that add up to the cardinality of
    /// the set one is partitioning.
    /// </summary>
    public readonly struct PartitionTypes : IEnumerable<List<int>>
    {
        readonly int set_cardinality;
        public PartitionTypes(int set_cardinality)
        {
            this.set_cardinality = set_cardinality;
        }

        /// <summary>
        /// Inner implementation of the enumeration logic.
        /// Written in form of yield loop because it is easier to reason about.
        /// </summary>
        private IEnumerable<List<int>> Enumerator()
        {
            if (set_cardinality < 1) yield break;
            var part = new List<int> { set_cardinality };

            // `pivot` represents the biggest `i` such that `part[i] > 1`.
            int pivot = 0;

            int to_add_back;
            int pivot_value;

            // While `part` contains at least one item that is bigger than `1`:
            while (pivot >= 0)
            {
                // Return current partition type before proceeding to the next
                yield return part;

                // Loop body explanation:
                // We decrease `list[pivot]` by one, we discard the items after `pivot`
                // and we refill `part`, so that it is the lexicographically
                // biggest decreasing sequence that adds up to set_cardinality, and that
                // has `part[0..pivot + 1]` as a prefix.

                // We are going to discard the `1` items after part[pivot], so the
                // sum of `part` is going to be decreased by the cardinality of the set
                // {n integer such that pivot < n < list.Count}, which amounts to
                // `list.Count - pivot - 1`.
                // We are also going to decrease `list[pivot]` by 1, so the sum of 
                // part is decreased exactly by `part.Count - pivot`.
                to_add_back = part.Count - pivot;
                // since part[pivot] > 1, pivot_value > 0
                pivot_value = --part[pivot];
                // removing all the `1` items
                part.RemoveRange(pivot + 1, part.Count - pivot - 1);
                // The lexicographically biggest sequence that continues
                // part[0..pivot + 1] appends `pivot_value` to `part`, repeated
                // `to_add_back / pivot_value` many times. This is because,
                // for every new element, it must be less or equal than the previous one,
                // that is in turn less or equal than `pivot_value`, so the biggest we
                // can add is `pivot_value`. We can repeat this step
                // `to_add_back / pivot_value` many times before overshooting the
                // initial sum of `part`.
                part.AddRange(Enumerable.Repeat(pivot_value, to_add_back / pivot_value));
                to_add_back -= pivot_value * (to_add_back / pivot_value);
                // since the previous step cannot bring us all the way to matching
                // the initial sum of `part`, we add the remainder as last element.
                if (to_add_back > 0)
                {
                    part.Add(to_add_back);
                    // If the last element is bigger than 1, then pivot has to point it
                    if (to_add_back > 1) pivot = part.Count - 1;
                    // If the last element is not bigger than 1, then pivot must be
                    // less than part.Count - 1. If pivot_value > 1, then all elements
                    // up to the second-last one are bigger than 1, so pivot is
                    // exactly part.Count - 2.
                    else if (pivot_value > 1) pivot = part.Count - 2;
                    // In case pivot_value == 1, then we know that for all `i >= pivot`
                    // it holds `part[i] == 1`, so the new value for pivot should be less
                    // than the current value of pivot.
                    // On the other hand, by the definition of pivot, and by the
                    // non-increasing nature of `part`, we know that `part[pivot - 1] > 1`
                    // so the new value for pivot is `pivot - 1`.
                    //
                    // In case `pivot == 0` to start with, the above reasoning does not
                    // make sense, but it also implies that `part` now only contains
                    // `1` items. So, by setting `pivot = -1`, we signal to the
                    // while loop that the iteration must end.
                    else pivot--;
                }
                else
                {
                    // In this branch, we know that to_add_back == 0, so we can simplify
                    // the rule to update pivot. We do it with the same kind of reasoning
                    // applied to the `if (to_add_back > 0)` branch.
                    if (pivot_value > 1) pivot = part.Count - 1;
                    else pivot--;
                }
            }
            // we return the last partition type from here, because the process
            // of computing the type [1, ..., 1] assigns pivot to the -1 value
            yield return part;
        }

        public IEnumerator<List<int>> GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }
    }

    /// <summary>
    /// Iterates over the permutations of the numbers from 0 to `n_objects - 1`
    /// using the L algorithm by Knuth. For info, look it up on wikipedia.
    /// </summary>
    public readonly struct Knuth_L : IEnumerable<int[]>
    {
        readonly int n_objects;

        public Knuth_L(int n_objects)
        {
            this.n_objects = n_objects;
        }

        /// <summary>
        /// Inner implementation of the enumeration logic.
        /// Written in form of yield loop because it is easier to reason about.
        /// </summary>
        private IEnumerable<int[]> Enumerator()
        {
            // we can only permutate at least one object.
            if (n_objects < 1) yield break;

            var a = new int[n_objects];
            // We initialize the permutation to be the identity function.
            for (int idx = 0; idx < n_objects; idx++) a[idx] = idx;

            int j;
            int l;

            while (true)
            {
                // return the current iteration before proceeding to the next
                yield return a;

                // Find last j such that a[j] <= a[j+1]. Terminate if no such j exists.
                for (j = n_objects - 2; j >= 0; j--) if (a[j] <= a[j + 1]) break;
                // If j < 0, that means that we reached the last permutation,
                // i.e. List { n_objects - 1, n_objects - 2, ..., 1, 0 }        
                if (j < 0) yield break;
                // Find last l such that a[j] <= a[l], then exchange elements j and l.
                // l exists because by definition of j at least l=j+1 satisfies
                // a[l] = a[j + 1] >= a[j]
                for (l = a.Length - 1; l > j; l--) if (a[j] <= a[l]) break;
                (a[j], a[l]) = (a[l], a[j]);

                // Then reverse a[j + 1 .. a.Length]
                for (int k = a.Length; ++j < --k;) (a[j], a[k]) = (a[k], a[j]);
            }
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }
    }

    /// <summary>
    /// Iterates over the permutations of the object list given as input,
    /// using the L algorithm by Knuth. For info, look it up on wikipedia.
    /// </summary>
    public readonly struct Knuth_L_Action<L> : IEnumerable<L>
    where L : IList
    {
        readonly L list;
        readonly int n_objects;

        public Knuth_L_Action(L list)
        {
            this.list = list;
            n_objects = list.Count;
        }

        /// <summary>
        /// Inner implementation of the enumeration logic.
        /// Written in form of yield loop because it is easier to reason about.
        /// </summary>
        private IEnumerable<L> Enumerator()
        {
            // we can only permutate at least one object.
            if (n_objects < 1) yield break;

            // `a` keeps track of the current permutation of the input.
            // Every swap we do in `a`, we also mirror in `list`
            var a = new int[n_objects];
            // We initialize the permutation to be the identity function.
            for (int idx = 0; idx < n_objects; idx++) a[idx] = idx;

            int j;
            int l;

            var aux_buffer = new int[n_objects];

            while (true)
            {
                // return the current iteration before proceeding to the next
                yield return list;

                // Find last j such that a[j] <= a[j+1]. Terminate if no such j exists.
                for (j = n_objects - 2; j >= 0; j--) if (a[j] <= a[j + 1]) break;
                // If j < 0, that means that we reached the last permutation,
                // i.e. List { n_objects - 1, n_objects - 2, ..., 1, 0 }        
                if (j < 0) yield break;
                // Find last l such that a[j] <= a[l]
                // l exists because by definition of j at least l=j+1 satisfies
                // a[l] = a[j + 1] >= a[j]
                for (l = n_objects - 1; l > j; l--) if (a[j] <= a[l]) break;
                // Swap a[j] and a[l]. Also, swap list[j] and list[l]
                (a[j], a[l]) = (a[l], a[j]);
                (list[j], list[l]) = (list[l], list[j]);

                // Then reverse a[j + 1 .. n_objects], and list[j + 1 .. n_objects]
                for (int k = n_objects; ++j < --k;)
                {
                    (a[j], a[k]) = (a[k], a[j]);
                    (list[j], list[k]) = (list[k], list[j]);
                }
            }
        }

        public IEnumerator<L> GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }
    }


    /// <summary>
    /// Iterates the cartesian product of an array of lists.
    /// For performance reasons, the iteration happens in reverse order.
    /// </summary>
    /// 
    /// <remarks>
    /// If any of the lists contained in <c>collections</c> is empty,
    /// the function will try accessing invalid indices, and will
    /// raise an exception.
    /// </remarks>
    public readonly struct CartesianProduct<Item> : IEnumerable<Item[]>
    {
        readonly List<List<Item>> collections;

        public CartesianProduct(List<List<Item>> collections)
        {
            this.collections = collections;
        }

        /// <summary>
        /// Inner implementation of the enumeration logic.
        /// Written in form of yield loop because it is easier to reason about.
        /// </summary>
        private IEnumerable<Item[]> Enumerator()
        {
            // precomputation of collections.Length - 1.
            int n_collections = collections.Count - 1;

            // precomputation of the last valid index for every list in collections.
            var coll_counts = new int[collections.Count];
            // `indices` will be used as the mutable state to keep track of what
            // item to yield next.
            var indices = new int[collections.Count];

            // We compute `coll_counts`, and use it as initial state of `indices`.
            int i = 0;
            for (; i <= n_collections; i++)
            {
                coll_counts[i] = collections[i].Count - 1;
                indices[i] = coll_counts[i];
            }

            // the variable that we will yield.
            var item = new Item[collections.Count];
            // we build the first returnable value.
            for (int j = 0; j <= n_collections; j++) item[j] = collections[j][indices[j]];
            yield return item;

            i--;
            while (i >= 0)
            {
                if (indices[i] == 0)
                {
                    i--;
                    continue;
                }
                // here we know that indices[i] > 0.
                // We decrement indices[i], and we carry every following
                // indices[k] entry to coll_counts[k].
                // Think of it as similar to counting in reverse:
                // after 325000 one gets 324999.
                indices[i]--;
                for (; ++i <= n_collections;) indices[i] = coll_counts[i];
                i--;
                for (int j = 0; j <= n_collections; j++) item[j] = collections[j][indices[j]];
                yield return item;
            }
            yield break;
        }
        public IEnumerator<Item[]> GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Enumerator().GetEnumerator();
        }
    }
}