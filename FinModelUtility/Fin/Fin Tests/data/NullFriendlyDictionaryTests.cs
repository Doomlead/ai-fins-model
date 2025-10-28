﻿using fin.data.dictionaries;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.data;

public sealed class NullFriendlyDictionaryTests {
  [Test]
  public void TestAdd() {
    var impl = new NullFriendlyDictionary<string?, string>();
    impl.Add("foo", "bar");
    impl.Add(null, "goo");

    Assert.AreEqual(2, impl.Count);
    Assert.AreEqual("bar", impl["foo"]);
    Assert.AreEqual("goo", impl[null]);
  }

  [Test]
  public void TestClear() {
    var impl = new SetDictionary<string, string>();
    impl.Add("foo", "bar");
    impl.Add(null, "goo");

    impl.Clear();

    Assert.AreEqual(0, impl.Count);
  }
}