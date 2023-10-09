using System;

namespace TheLastStand.Framework.Extensions;

public static class IntExtensions
{
	public static int Mod(this int a, int m)
	{
		int num = a % m;
		if (num >= 0)
		{
			return num;
		}
		return num + m;
	}

	public static string ToRoman(this int number)
	{
		if (number < 0 || number > 3999)
		{
			throw new ArgumentOutOfRangeException("insert value between 1 and 3999");
		}
		if (number < 1)
		{
			return string.Empty;
		}
		if (number >= 1000)
		{
			return "M" + (number - 1000).ToRoman();
		}
		if (number >= 900)
		{
			return "CM" + (number - 900).ToRoman();
		}
		if (number >= 500)
		{
			return "D" + (number - 500).ToRoman();
		}
		if (number >= 400)
		{
			return "CD" + (number - 400).ToRoman();
		}
		if (number >= 100)
		{
			return "C" + (number - 100).ToRoman();
		}
		if (number >= 90)
		{
			return "XC" + (number - 90).ToRoman();
		}
		if (number >= 50)
		{
			return "L" + (number - 50).ToRoman();
		}
		if (number >= 40)
		{
			return "XL" + (number - 40).ToRoman();
		}
		if (number >= 10)
		{
			return "X" + (number - 10).ToRoman();
		}
		if (number >= 9)
		{
			return "IX" + (number - 9).ToRoman();
		}
		if (number >= 5)
		{
			return "V" + (number - 5).ToRoman();
		}
		if (number >= 4)
		{
			return "IV" + (number - 4).ToRoman();
		}
		if (number >= 1)
		{
			return "I" + (number - 1).ToRoman();
		}
		throw new ArgumentOutOfRangeException("something bad happened");
	}
}
