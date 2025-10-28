﻿using System.Numerics;

using CommunityToolkit.Diagnostics;

using fin.schema;

using schema.text;
using schema.text.reader;


namespace xmod.schema;

public static class TextReaderUtils {
  public static char OPEN_BRACE = '{';
  public static char CLOSING_BRACE = '}';
  public static char COLON = ':';
  public static char QUOTE = '"';

  public static string ReadKeyValue(ITextReader tr, string expectedKey) {
    var (key, value) = ReadKeyValue(tr);
    Guard.IsEqualTo(key, expectedKey);
    return value;
  }

  public static (string key, string value) ReadKeyValue(ITextReader tr) {
    var key = tr.ReadWord();
    tr.Matches(':');
    tr.SkipWhitespace();
    var value = tr.ReadLine();
    return (key, value);
  }

  public static TNumber ReadKeyValueNumber<TNumber>(
      ITextReader tr,
      string prefix)
      where TNumber : INumber<TNumber>
    => TNumber.Parse(ReadKeyValue(tr, prefix), null);

  public static T ReadKeyValueInstance<T>(ITextReader tr, string prefix)
      where T : ITextDeserializable, new() {
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    tr.AssertString(prefix);
    tr.AssertChar(':');
    tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
    var instance = tr.ReadNew<T>();
    tr.SkipOnceIfPresent(TextReaderConstants.NEWLINE_STRINGS);
    return instance;
  }

  public static T[] ReadKeyValueInstances<T>(ITextReader tr,
                                             string prefix,
                                             int count)
      where T : ITextDeserializable, new() {
    var values = new T[count];
    for (var i = 0; i < count; ++i) {
      values[i] = ReadKeyValueInstance<T>(tr, prefix);
    }

    return values;
  }


  public static T[] ReadInstances<T>(ITextReader tr,
                                     string prefix,
                                     int count)
      where T : ITextDeserializable, new() {
    var values = new T[count];
    for (var i = 0; i < count; ++i) {
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      tr.AssertString(prefix);
      tr.SkipManyIfPresent(TextReaderConstants.WHITESPACE_CHARS);
      values[i] = tr.ReadNew<T>();
      tr.SkipOnceIfPresent(TextReaderConstants.NEWLINE_STRINGS);
    }

    return values;
  }
}