using Microsoft.VisualStudio.TestTools.UnitTesting;

using BinaryHeap;
using IterTools;
using DebugUtilities;
using System.Text;

namespace TestHeapManager;

[TestClass]
public class TestHeapManager
{
    /// <summary>
    /// A method that returns a comparison operator that encodes a preorder
    /// relationship, and an enumeration of all possible input configurations
    /// with respect to such relationship.
    /// 
    /// Two input values [i1, ..., i_size] and [j1, ..., j_size]
    /// are considered to "belong to the same configuration with respect to
    /// a preorder cmp" if
    /// for every couple of indices idx1, idx2, it holds
    /// either cmp(i_idx1, i_idx2) == 0 && cmp(j_idx1, j_idx2) == 0
    /// or cmp(i_idx1, i_idx2) * cmp(j_idx1, j_idx2) > 0
    /// 
    /// in essence, cmp induces the same order structure on i and on j.
    /// </summary>
    /// 
    /// <returns>
    /// A comparison operator that encodes a preorder
    /// relationship, and an enumeration of all possible input configurations
    /// with respect to such relationship.
    /// </returns>
    private static (Comparison<int>, IEnumerable<int[]>) PreorderStructuresEnumerator(int size)
    {
        Assert.IsTrue(size > 0);
        // Using a preorder relation instead of an order, so that we
        // can test what happens when some different items are considered equal
        Comparison<int> cmp = (a, b) => a / size - b / size;

        // This lists all possible cmp-induced preorders on `size` items.
        static IEnumerable<int[]> EnumeratorImplementation(int size)
        {
            var items = new int[size];
            // `cmp_eq_part` iterates over all possible ways of partitioning
            // `size` many items in cmp-equivalence classes.
            //
            // We care about the order in which the partitions
            // are listed, so for example, with size = 3,
            // the partition [2, 1] is not the same as
            // the partition [1, 2]
            //
            // Indeed, [2, 1] would yield the items I = [0, 1, 3]
            // while   [1, 2] would yield the items J = [0, 3, 4]
            // and those are not equivalent because
            // cmp(I[0], I[1]) == 0, cmp(I[0], I[2]) < 0, cmp(I[1], I[2]) < 0
            // while
            // cmp(J[1], J[2]) == 0, cmp(J[0], J[1]) < 0, cmp(J[0], J[2]) < 0
            // so, for I there is one item that is strictly bigger than the
            // others, and no item is strictly smaller than the others.
            // while for J the opposite is true.
            foreach (List<int> cmp_eq_part in new Partitions(size))
            {
                int i = 0;
                int next_item_value = 0;
                foreach (int eq_class_size in cmp_eq_part)
                {
                    for (int j = eq_class_size; j-- > 0; next_item_value++)
                    {
                        items[i++] = next_item_value;
                    }
                    // next_item_value = "the next multiple of LEN"   
                    next_item_value = next_item_value + size - eq_class_size;
                }
                yield return items;
            }
            yield break;
        }

        return (cmp, EnumeratorImplementation(size));
    }

    /// <summary>
    /// This test runs a single minimal example of what heapify does.
    /// This test is supposed to work with the current implementation details,
    /// and might need a rework if the details in the execution change.
    /// </summary>
    [TestMethod]
    public void TestHeapify()
    {
        string[] list = new string[10] {
            "z", "aha", "wonderful", "", "work", "zoo",
            "zebra", "ananas", "papaya", "paper"
        };
        Comparison<string> strcmp = (string a, string b) => b.CompareTo(a);

        Assert.IsFalse(HeapManager.IsHeap(ref list, strcmp));
        HeapManager.Heapify(ref list, strcmp);
        // How Heapify(ref list, strcmp) is supposed to act:
        // parent          | child1       | child2          | outcome
        // 3 - ""          | 7 - "ananas" | 8 - "papaya"    | no swap
        // 2 - "wonderful" | 5 - "zoo"    | 6 - "zebra"     | no swap
        // 1 - "aha"       | 3 - ""       | 4 - "work"      | swap 1 and 3
        // 3 - "aha"       | 7 - "ananas" | 8 - "papaya"    | no swap
        // 0 - "z"         | 1 - ""       | 2 - "wonderful" | swap 0 and 1
        // 1 - "z"         | 3 - "aha"    | 4 - "work"      | swap 1 and 3
        // 3 - "z"         | 7 - "ananas" | 8 - "papaya"    | swap 3 and 7
        // 
        // then SiftUp(ref list, strcmp, 9) is executed:
        // 
        // child       | parent     | outcome 
        // 9 - "paper" | 4 - "work" | swap
        // 4 - "paper" | 1 - "aha"  | no swap
        //
        // and the final output is:
        string[] heapify_expected_out = new string[10] {
            "", "aha", "wonderful", "ananas", "paper",
            "zoo", "zebra", "z", "papaya", "work"
        };
        Assert.IsTrue(HeapManager.IsHeap(ref list, strcmp));
        Assert.IsTrue(list.SequenceEqual(heapify_expected_out));
    }

    /// <summary>
    /// This test verifies that `HeapManager.Insert` and
    /// `HeapManager.Heapify` behave as expected.
    /// The test executes all possible configurations of a heap
    /// of a given length.
    /// 
    /// If `HeapManager.IsHeap` behaves as expected, and this test
    /// passes, it is a strong indicator that
    /// `Heapify` and `Insert` behave as expected too.
    /// </summary>
    [TestMethod]
    public void TestInsertAndHeapify()
    {
        const int LEN = 8;

        var test1 = new int[LEN];
        var test2 = new List<int>();

        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN);

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN - LEN + 1];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                // we fill `test1` with a permutation of `items`.
                permutation.CopyTo(test1, 0);

                // we use `HeapManager.Insert`, on every element of `test1`,
                // to fill `test1`.
                test2.Clear();
                foreach (int item in test1)
                {
                    // After every insert, we check that the heap property holds.
                    HeapManager.Insert(ref test2, cmp, item);
                    DebugUtils.AssertIsTrueLazyMsgEval(
                        HeapManager.IsHeap(ref test2, cmp),
                        () => string.Format(
                            "items = {0}\ntest2 = {1}",
                            items.ToDbgString(),
                            test2.ToDbgString()
                        )
                    );
                }
                // We heapify `test1` and we check that it satisfies
                // the heap property  with respect to `cmp`.
                HeapManager.Heapify(ref test1, cmp);
                Assert.IsTrue(HeapManager.IsHeap(ref test1, cmp));

                // The items in `test1` and `test2` should be the same
                // as the items contained in `items`. Let's check:

                // Assert that `test1` is a permutation of `items`
                foreach (int item in test1) item_count[item] += 1;
                foreach (int item in items) item_count[item] -= 1;
                Assert.IsTrue(item_count.All(c => c == 0));

                // Assert that `test2` is a permutation of `items`
                foreach (int item in test2) item_count[item] += 1;
                foreach (int item in items) item_count[item] -= 1;
                Assert.IsTrue(item_count.All(c => c == 0));
            }
    }

    /// <summary>
    /// This test verifies that `HeapManager.PopMax` behaves as expected.
    /// The test executes all possible configurations of a heap of a given length.
    /// 
    /// If `HeapManager.IsHeap` and `HeapManager.Heapify`
    /// behave as expected, and this test passes, it is a strong
    /// indicator that `PopMax` behaves as expected too.
    /// </summary>
    [TestMethod]
    public void TestPopMax()
    {
        const int LEN = 8;
        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN);

        var test = new List<int>();

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN - LEN + 1];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                test.AddRange(permutation);
                // We heapify the input to reach a valid heap initial configuration.
                HeapManager.Heapify(ref test, cmp);

                foreach (int item in items) item_count[item] += 1;
                for (int i = LEN; i-- > 0;)
                {
                    // We assert that, after every pop, test preserves the
                    // heap property.
                    item_count[HeapManager.PopMax(ref test, cmp)] -= 1;
                    DebugUtils.AssertIsTrueLazyMsgEval(
                        HeapManager.IsHeap(ref test, cmp),
                        () => string.Format(
                            "test = {0}",
                            test.ToDbgString()
                        )
                    );
                    Assert.AreEqual(test.Count, i);
                }

                // We assert that, after LEN many pops, test is empty, and
                // that the calls to PopMax returned exactly the items inside `items`.
                Assert.AreEqual(test.Count, 0);
                Assert.IsTrue(item_count.All(c => c == 0));
            }
    }

    /// <summary>
    /// This test verifies that `HeapManager.FindEqItem` behaves as expected.
    /// The test executes all possible configurations of a heap of a given length.
    /// 
    /// If `HeapManager.Heapify` behaves as expected, and this test passes,
    /// it is a strong indicator that `FindEqItem` behaves as expected too.
    /// </summary>
    [TestMethod]
    public void TestFindEqItem()
    {
        const int LEN = 6;

        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN + 1);

        var test = new int[LEN];

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN + LEN + 1];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                // We fill test with the input data, and we make it a heap.
                // We leave the last item out. That item will be used as search input.
                permutation[..LEN].CopyTo(test, 0);
                HeapManager.Heapify(ref test, cmp);

                var item_to_find = permutation[LEN];
                var search_result = HeapManager.FindEqItem(ref test, cmp, item_to_find);

                // We assert that whenever FindEqItem returns -1, nothing in the
                // test heap was found. Otherwise, we test that the item
                // at the index output by the search result is cmp-equivalent to
                // the item_to_find.
                if (search_result == -1)
                    Assert.IsTrue(test.All(item => cmp(item, item_to_find) != 0));
                else
                    Assert.IsTrue(cmp(test[search_result], item_to_find) == 0);
            }
    }

    /// <summary>
    /// This test verifies that `HeapManager.ReplaceAtIndex` behaves as expected.
    /// The test executes all possible configurations of a heap of a given length.
    /// 
    /// If `HeapManager.IsHeap` and `HeapManager.Heapify`
    /// behave as expected, and this test passes, it is a strong
    /// indicator that `ReplaceAtIndex` behaves as expected too.
    /// </summary>
    [TestMethod]
    public void TestReplaceAtIndex()
    {
        const int LEN = 6;
        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN + 1);

        // We will have LEN identical copies of each input test case,
        // and we apply ReplaceKey to each of them, with different index
        // being replaced in each of them.
        var tests = new List<int>[LEN];
        for (int i = 0; i < LEN; i++) tests[i] = new();

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN + LEN + 1];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                // we build the input for the test case
                tests[0].Clear();
                tests[0].AddRange(permutation[..LEN]);
                HeapManager.Heapify(ref tests[0], cmp);

                // and we make LEN - 1 copies of the same configuration.
                for (int j = 1; j < LEN; j++)
                {
                    tests[j].Clear();
                    tests[j].AddRange(tests[0]);
                }

                // we left out one item, which we will insert in each of the input
                // copies with ReplaceKey
                var item_to_insert = permutation[LEN];

                for (int replace_index = 0; replace_index < LEN; replace_index++)
                {
                    // And we count items to make sure that no items were duplicated
                    // or destroyed in the process. For this we count input and output.
                    foreach (int item in items) item_count[item] += 1;
                    item_count[
                        HeapManager.ReplaceAtIndex(ref tests[replace_index], cmp, replace_index, item_to_insert)
                    ] -= 1;
                    // Assert.IsFalse(true, string.Format("replace_index = {0}\ntest = {1}", replace_index, tests[replace_index].ToDbgString()));

                    // We make sure that ReplaceAtIndex leaves the heap property intact,
                    DebugUtils.AssertIsTrueLazyMsgEval(
                        HeapManager.IsHeap(ref tests[replace_index], cmp),
                        () => string.Format("items = {0}\ntest = {1}",
                        items[replace_index].ToDbgString(),
                        tests[replace_index].ToDbgString()
                        )
                    );

                    foreach (int item in tests[replace_index]) item_count[item] -= 1;

                    // We assert that no items were duplicated or destroyed.
                    DebugUtils.AssertIsTrueLazyMsgEval(
                        item_count.All(c => c == 0),
                        () => string.Format(
                            "permutation = {0}\nitem_count = {1}",
                            permutation.ToDbgString(),
                            item_count.ToDbgString()
                        )
                    );
                }
            }
    }

    /// <summary>
    /// This test verifies that `HeapManager.ReplaceItem` behaves as expected.
    /// The test executes all possible configurations of a heap of a given length.
    /// 
    /// If `HeapManager.IsHeap` and `HeapManager.Heapify`
    /// behave as expected, and this test passes, it is a strong
    /// indicator that `ReplaceItem` behaves as expected too.
    /// </summary>
    [TestMethod]
    public void TestReplaceItem()
    {
        const int LEN = 6;
        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN + 2);

        var test = new int[LEN];

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN + 3 * LEN + 3];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                permutation[..LEN].CopyTo(test, 0);
                HeapManager.Heapify(ref test, cmp);

                // We left two items out of test. One is to be used for comparison
                // and the other one is used to replace the item, if a suitable one
                // is found in test.
                var item_to_search = permutation[LEN];
                var replace_with = permutation[LEN + 1];

                (bool found, int output) = HeapManager.ReplaceItem(ref test, cmp, item_to_search, replace_with);

                // Whatever the outcome, test must still be a heap
                DebugUtils.AssertIsTrueLazyMsgEval(
                    HeapManager.IsHeap(ref test, cmp),
                    () => string.Format(
                        "item_to_search = {0}\nreplace_with = {1}\noutput = {2}, test = {3}",
                        item_to_search, replace_with, output, test.ToDbgString()
                    )
                );

                // Whatever the outcome, the item count must be consistent
                foreach (int item in items) item_count[item] += 1;
                foreach (int item in test) item_count[item] -= 1;
                // item_to_search should always stay out of test
                item_count[item_to_search] -= 1;
                // if found == true, then replace_with is in test, and was
                // already accounted for, while output wasn't.
                // if found == false, the opposite is true.
                if (found) item_count[output] -= 1;
                else item_count[replace_with] -= 1;

                Assert.IsTrue(item_count.All(c => c == 0));

                // If the item was replaced, check that it was actually
                // equivalent to item_to_search.
                if (found) Assert.IsTrue(cmp(output, item_to_search) == 0);
            }
    }

    [TestMethod]
    public void TestExtend()
    {
        const int LEN = 8;

        var (cmp, cmp_preorder_structures) = PreorderStructuresEnumerator(LEN);

        var test = new List<int>();

        // This array will be used for sanity checks.
        var item_count = new int[LEN * LEN - LEN + 1];
        // We make sure that is is `0` initialized. Maybe this step is
        // unnecessary, but it is only repeated once, so who cares.
        for (int i = 0; i < item_count.Length; i++) item_count[i] = 0;

        foreach (int[] items in cmp_preorder_structures)
            foreach (int[] permutation in new Knuth_L_Action<int[]>(items))
            {
                for (int j = 0; j < LEN; j++)
                {
                    test.Clear();
                    test.AddRange(new ArraySegment<int>(permutation, 0, j));
                    HeapManager.Heapify(ref test, cmp);

                    var perm_tail = new ArraySegment<int>(permutation, j, LEN - j);
                    HeapManager.Extend(ref test, cmp, ref perm_tail);

                    // We make sure that after the extension `test` is a heap,
                    // and that it contains the same items as `items`
                    DebugUtils.AssertIsTrueLazyMsgEval(
                        HeapManager.IsHeap(ref test, cmp),
                        () => string.Format(
                            "starting items = {0}\nextension items{1}\ntest result = {2}\nis heap = {3}",
                            permutation[..j].ToDbgString(),
                            permutation[j..].ToDbgString(),
                            test.ToDbgString(),
                            HeapManager.IsHeap(ref test, cmp)
                        )
                    );
                    Assert.IsTrue(HeapManager.IsHeap(ref test, cmp));
                    foreach (int item in items) item_count[item] += 1;
                    foreach (int item in test) item_count[item] -= 1;
                    Assert.IsTrue(item_count.All(c => c == 0));
                }
            }
    }
}