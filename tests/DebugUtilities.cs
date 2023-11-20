using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Collections;

namespace DebugUtilities;

public static class DebugUtils
{
    public static bool IsTrue(this bool? nullable_bool)
    {
        return nullable_bool.HasValue && nullable_bool.Value;
    }

    public static (object?, object?) AssertAreEqualLazyMsgEval(object? o1, object? o2, Func<string> lazy_error_msg)
    {
        // Copy-paste of the condition that `Assert.AreEqual` checks before
        // throwing an error. The message is evaluated only if the error
        // must actually be thrown.
        if (!object.Equals(o1, o2)) Assert.AreEqual(o1, o2, lazy_error_msg());
        // We allow for use in fluent-like method chaining.
        return (o1, o2);
    }

    public static void AssertIsTrueLazyMsgEval(bool condition, Func<string> lazy_error_msg)
    {
        // Copy-paste of the condition that `Assert.IsTrue` checks before
        // throwing an error. The message is evaluated only if the error
        // must actually be thrown.
        if (!condition) Assert.IsTrue(condition, lazy_error_msg());
    }

    /// <summary>
    /// Usable to pretty print a finite sequence of objects as
    /// String.Format(
    ///     "[{0}, {1}, ..., {n}]",
    ///     item_0.ToDbgString(),
    ///     item_1.ToDbgString(),
    ///     ...
    ///     item_n.ToDbgString()
    /// )
    /// 
    /// ToDbgString equals ToString if the object is not IEnumerable
    /// </summary>
    public static String ToDbgString(this object s)
    {
        var r = new StringBuilder();
        ToDbgString(s, r);
        return r.ToString();
    }

    public static void ToDbgString(this object s, StringBuilder partial)
    {
        if (s is IEnumerable e)
        {
            var iter = e.GetEnumerator();
            partial.Append('[');
            if (iter.MoveNext())
            {
                // code repetition to avoid inserting a comma
                // before the first item.
                iter.Current.ToDbgString(partial);
                while (iter.MoveNext())
                {
                    partial.Append(", ");
                    iter.Current.ToDbgString(partial);
                }
            }
            partial.Append(']');
            if (iter is IDisposable trash) trash.Dispose();
        }
        else if (s != null) partial.Append(s.ToString());
        else partial.Append("null");
    }

    public static void AssertNestedSequenceEqual<L1, L2>(this L1 s1, L2 s2)
    where L1 : IEnumerable
    where L2 : IEnumerable
    {
        // In case the sequence equality comparison fails, we print an
        // error message representing the compared sequences so far, up to
        // the elements that were found to be different.
        var str_read_1 = new StringBuilder();
        var str_read_2 = new StringBuilder();
        AssertNestedSequenceEqual(s1, s2, str_read_1, str_read_2);
    }

    public static void AssertNestedSequenceEqual<L1, L2>(
        this L1 s1, L2 s2,
        StringBuilder str_read_1,
        StringBuilder str_read_2
    )
    where L1 : IEnumerable
    where L2 : IEnumerable
    {
        var iter1 = s1.GetEnumerator();
        var iter2 = s2.GetEnumerator();
        str_read_1.Append('[');
        str_read_2.Append('[');
        if (SynchroAdvance(iter1, iter2, str_read_1, str_read_2))
        {
            do
            {
                if (iter1.Current is IEnumerable e1 && iter2.Current is IEnumerable e2)
                    AssertNestedSequenceEqual(e1, e2, str_read_1, str_read_2);
                else
                {
                    str_read_1.AppendFormat("{0}, ", iter1.Current.ToDbgString());
                    str_read_2.AppendFormat("{0}, ", iter2.Current.ToDbgString());
                    AssertAreEqualLazyMsgEval(
                        iter1.Current,
                        iter2.Current,
                        () => string.Format(
                            "Sequences are not equal.\nCompared so far:\n\nSequence1:\n{0}\n\nSequence2:\n{1}\n\n",
                            str_read_1.ToDbgString(),
                            str_read_2.ToDbgString()
                        )
                    );
                }
            } while (SynchroAdvance(iter1, iter2, str_read_1, str_read_2));
            // The last two char here are ", ", if there was at least one item
            // in the sequences. We are sure that there is at least one value
            // on both sequences, because we are in an `if` block that checks
            // exactly that at least once it holds
            // `iter1.MoveNext() && iter2.MoveNext()`.
            str_read_1.Remove(str_read_1.Length - 2, 2);
            str_read_2.Remove(str_read_2.Length - 2, 2);
        }
        str_read_1.Append(']');
        str_read_2.Append(']');

        // Helper function to avoid excessive code duplication.
        // Attempts advancing the iterators checking that they either both
        // advance, or are both at the end of their sequence.
        // If that is not the case, an error is thrown,
        // otherwise, returns whether or not the iterators were advanced.
        static bool SynchroAdvance(
            IEnumerator iter1,
            IEnumerator iter2,
            StringBuilder str_read_1,
            StringBuilder str_read_2
        )
        {
            var move_next1 = iter1.MoveNext();
            var move_next2 = iter2.MoveNext();
            return (AssertAreEqualLazyMsgEval(
                move_next1,
                move_next2,
                () => string.Format(
                    "Sequences should be of same length.\nmove_next1 = {0} ; move_next2 = {1}\nCompared so far:\n\nSequence1:\n{2}\n\nSequence2:\n{3}\n\nNext item in Sequence1: {4}\nNext item in Sequence2: {5}",
                    move_next1,
                    move_next2,
                    str_read_1.ToDbgString(),
                    str_read_2.ToDbgString(),
                    iter1.Current.ToDbgString(),
                    iter2.Current.ToDbgString()
                )
            ).Item1 as bool?
            ).IsTrue();
        }
    }
}