using System.Collections.Generic;

namespace TheLastStand.Framework.Extensions;

public static class StringExtensions
{
	public static string TrimOnEachLine(this string s, params char[] trimChars)
	{
		string text = string.Empty;
		string[] array = s.Split(new char[1] { '\n' });
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			text += array[i].TrimStart(trimChars).TrimEnd(trimChars);
			if (i < array.Length - 1)
			{
				text += "\n";
			}
		}
		return text;
	}

	public static string Replace(this string s, Dictionary<string, string> replacements)
	{
		if (replacements == null)
		{
			return s;
		}
		string text = s;
		foreach (KeyValuePair<string, string> replacement in replacements)
		{
			text = text.Replace(replacement.Key, replacement.Value);
		}
		return text;
	}

	public static string ReplaceFirst(this string text, string search, string replace)
	{
		int num = text.IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
	}

	public static string ReplaceFirst(this string text, string search, string replace, int beginPosSearch = 0)
	{
		string text2 = text.Substring(0, beginPosSearch);
		string text3 = text.Substring(beginPosSearch);
		int num = text3.IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text2 + text.Substring(beginPosSearch, num) + replace + text3.Substring(num + search.Length);
	}

	public static string InsertBefore(this string text, string search, string insert, int beginPosSearch = 0)
	{
		string text2 = text.Substring(0, beginPosSearch);
		int num = text.Substring(beginPosSearch).IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text2 + text.Substring(beginPosSearch, num) + insert + text.Substring(beginPosSearch + num);
	}
}
