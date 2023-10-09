using System;
using System.Collections.Generic;

namespace TheLastStand.Framework.Collections;

public class ReversedComparer<T> : IComparer<T> where T : IComparable<T>
{
	public int Compare(T a, T b)
	{
		return b.CompareTo(a);
	}
}
