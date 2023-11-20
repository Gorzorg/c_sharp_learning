using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Collections;

using IterTools;
using DebugUtilities;

namespace TestIterTools;

[TestClass]
public class UnitTests
{
    /// <summary>
    /// Minimal test for IterTools.Partitions.
    /// </summary>
    [TestMethod]
    public void TestPartitions()
    {
        // Writing the output explicitly makes the test less customizable
        // but it gives better certainty on the exact nature of what is
        // being tested.
        // In this elementary function, I believe it is worth it.
        var expected_out = new List<List<int>>
        {
            new() {4},
            new() {3, 1},
            new() {2, 2},
            new() {2, 1, 1},
            new() {1, 3},
            new() {1, 2, 1},
            new() {1, 1, 2},
            new() {1, 1, 1, 1}
        };

        new Partitions(4).AssertNestedSequenceEqual(expected_out);

    }

    /// <summary>
    /// Minimal test for IterTools.PartitionTypes.
    /// </summary>
    [TestMethod]
    public void TestPartitionTypes()
    {
        // Writing the output explicitly makes the test less customizable
        // but it gives better certainty on the exact nature of what is
        // being tested.
        // In this elementary function, I believe it is worth it.
        var expected_out = new List<List<int>> {
            new() {7},
            new() {6, 1},
            new() {5, 2},
            new() {5, 1, 1},
            new() {4, 3},
            new() {4, 2, 1},
            new() {4, 1, 1, 1},
            new() {3, 3, 1},
            new() {3, 2, 2},
            new() {3, 2, 1, 1},
            new() {3, 1, 1, 1, 1},
            new() {2, 2, 2, 1},
            new() {2, 2, 1, 1, 1},
            new() {2, 1, 1, 1, 1, 1},
            new() {1, 1, 1, 1, 1, 1, 1},
        };

        new PartitionTypes(7).AssertNestedSequenceEqual(expected_out);
    }

    /// <summary>
    /// Minimal test for IterTools.Knuth_L.
    /// </summary>
    [TestMethod]
    public void TestKnuth_L()
    {
        // Writing the output explicitly makes the test less customizable
        // but it gives better certainty on the exact nature of what is
        // being tested.
        // In this elementary function, I believe it is worth it.
        var expected_out = new List<int[]> {
            new int[] {0, 1, 2, 3},
            new int[] {0, 1, 3, 2},
            new int[] {0, 2, 1, 3},
            new int[] {0, 2, 3, 1},
            new int[] {0, 3, 1, 2},
            new int[] {0, 3, 2, 1},
            new int[] {1, 0, 2, 3},
            new int[] {1, 0, 3, 2},
            new int[] {1, 2, 0, 3},
            new int[] {1, 2, 3, 0},
            new int[] {1, 3, 0, 2},
            new int[] {1, 3, 2, 0},
            new int[] {2, 0, 1, 3},
            new int[] {2, 0, 3, 1},
            new int[] {2, 1, 0, 3},
            new int[] {2, 1, 3, 0},
            new int[] {2, 3, 0, 1},
            new int[] {2, 3, 1, 0},
            new int[] {3, 0, 1, 2},
            new int[] {3, 0, 2, 1},
            new int[] {3, 1, 0, 2},
            new int[] {3, 1, 2, 0},
            new int[] {3, 2, 0, 1},
            new int[] {3, 2, 1, 0}
        };

        new Knuth_L(4).AssertNestedSequenceEqual(expected_out);
    }

    /// <summary>
    /// Minimal test for IterTools.Knuth_L_Action.
    /// </summary>
    [TestMethod]
    public void TestKnuth_L_Action()
    {
        var test_list = new string[4] { "hi", "hello", "howdy", "goodbye" };
        // Writing the output explicitly makes the test less customizable
        // but it gives better certainty on the exact nature of what is
        // being tested.
        // In this elementary function, I believe it is worth it.

        var expected_out = new List<string[]> {
            new string[] {"hi", "hello", "howdy", "goodbye"},
            new string[] {"hi", "hello", "goodbye", "howdy"},
            new string[] {"hi", "howdy", "hello", "goodbye"},
            new string[] {"hi", "howdy", "goodbye", "hello"},
            new string[] {"hi", "goodbye", "hello", "howdy"},
            new string[] {"hi", "goodbye", "howdy", "hello"},
            new string[] {"hello", "hi", "howdy", "goodbye"},
            new string[] {"hello", "hi", "goodbye", "howdy"},
            new string[] {"hello", "howdy", "hi", "goodbye"},
            new string[] {"hello", "howdy", "goodbye", "hi"},
            new string[] {"hello", "goodbye", "hi", "howdy"},
            new string[] {"hello", "goodbye", "howdy", "hi"},
            new string[] {"howdy", "hi", "hello", "goodbye"},
            new string[] {"howdy", "hi", "goodbye", "hello"},
            new string[] {"howdy", "hello", "hi", "goodbye"},
            new string[] {"howdy", "hello", "goodbye", "hi"},
            new string[] {"howdy", "goodbye", "hi", "hello"},
            new string[] {"howdy", "goodbye", "hello", "hi"},
            new string[] {"goodbye", "hi", "hello", "howdy"},
            new string[] {"goodbye", "hi", "howdy", "hello"},
            new string[] {"goodbye", "hello", "hi", "howdy"},
            new string[] {"goodbye", "hello", "howdy", "hi"},
            new string[] {"goodbye", "howdy", "hi", "hello"},
            new string[] {"goodbye", "howdy", "hello", "hi"}
        };

        new Knuth_L_Action<string[]>(test_list).AssertNestedSequenceEqual(expected_out);
    }

    /// <summary>
    /// Minimal test for IterTools.Knuth_L.
    /// </summary>
    [TestMethod]
    public void TestCartesianProduct()
    {
        var dims = new int[] { 6, 2, 3 };
        var collections = new List<List<int>> { };
        foreach (int len in dims)
        {
            var aux = new List<int> { };
            for (int c = 0; c < len; c++) aux.Add(c);
            collections.Add(aux);
        }

        // Writing the output explicitly makes the test less customizable
        // but it gives better certainty on the exact nature of what is
        // being tested.
        // In this elementary function, I believe it is worth it.
        var expected_out = new List<List<int>> {
            new() {5, 1, 2},
            new() {5, 1, 1},
            new() {5, 1, 0},
            new() {5, 0, 2},
            new() {5, 0, 1},
            new() {5, 0, 0},
            new() {4, 1, 2},
            new() {4, 1, 1},
            new() {4, 1, 0},
            new() {4, 0, 2},
            new() {4, 0, 1},
            new() {4, 0, 0},
            new() {3, 1, 2},
            new() {3, 1, 1},
            new() {3, 1, 0},
            new() {3, 0, 2},
            new() {3, 0, 1},
            new() {3, 0, 0},
            new() {2, 1, 2},
            new() {2, 1, 1},
            new() {2, 1, 0},
            new() {2, 0, 2},
            new() {2, 0, 1},
            new() {2, 0, 0},
            new() {1, 1, 2},
            new() {1, 1, 1},
            new() {1, 1, 0},
            new() {1, 0, 2},
            new() {1, 0, 1},
            new() {1, 0, 0},
            new() {0, 1, 2},
            new() {0, 1, 1},
            new() {0, 1, 0},
            new() {0, 0, 2},
            new() {0, 0, 1},
            new() {0, 0, 0},
        };

        new CartesianProduct<int>(collections).AssertNestedSequenceEqual(expected_out);
    }
}