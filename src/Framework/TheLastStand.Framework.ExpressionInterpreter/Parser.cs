using System;
using System.Collections.Generic;
using System.IO;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class Parser
{
	private readonly Tokenizer tokenizer;

	public Parser(Tokenizer tokenizer)
	{
		this.tokenizer = tokenizer;
	}

	public static Node Parse(string s, Dictionary<string, string> tokenVariables = null)
	{
		s = s.Replace(tokenVariables);
		return Parse(new Tokenizer(new StringReader(s)));
	}

	public static Node Parse(Tokenizer tokenizer)
	{
		return new Parser(tokenizer).ParseExpression();
	}

	public Node ParseExpression()
	{
		Node result = ParseComparer();
		if (tokenizer.Token != 0)
		{
			Debug.LogError((object)"Unexpected characters at end of expression");
		}
		return result;
	}

	private Node ParseComparer()
	{
		Node node = ParseAddSubtract();
		while (true)
		{
			Func<double, double, bool> func = null;
			if (tokenizer.Token == E_Token.Superior)
			{
				func = (double a, double b) => a > b;
			}
			else if (tokenizer.Token == E_Token.Inferior)
			{
				func = (double a, double b) => a < b;
			}
			else if (tokenizer.Token == E_Token.Equals)
			{
				func = (double a, double b) => a.Equals(b);
			}
			else if (tokenizer.Token == E_Token.Different)
			{
				func = (double a, double b) => !a.Equals(b);
			}
			if (func == null)
			{
				break;
			}
			tokenizer.NextToken();
			Node rightHandSize = ParseAddSubtract();
			node = new NodeBoolean(node, rightHandSize, func);
		}
		return node;
	}

	private Node ParseAddSubtract()
	{
		Node node = ParseMultiplyDivide();
		while (true)
		{
			Func<double, double, double> func = null;
			if (tokenizer.Token == E_Token.Add)
			{
				func = (double a, double b) => a + b;
			}
			else if (tokenizer.Token == E_Token.Subtract)
			{
				func = (double a, double b) => a - b;
			}
			if (func == null)
			{
				break;
			}
			tokenizer.NextToken();
			Node rightHandSize = ParseMultiplyDivide();
			node = new NodeBinary(node, rightHandSize, func);
		}
		return node;
	}

	private Node ParseLeaf()
	{
		if (tokenizer.Token == E_Token.Number)
		{
			NodeNumber result = new NodeNumber(tokenizer.Number);
			tokenizer.NextToken();
			return result;
		}
		if (tokenizer.Token == E_Token.OpenParens)
		{
			tokenizer.NextToken();
			Node result2 = ParseComparer();
			if (tokenizer.Token != E_Token.CloseParens)
			{
				Debug.LogError((object)"Missing close parenthesis!");
			}
			tokenizer.NextToken();
			return result2;
		}
		if (tokenizer.Token == E_Token.Identifier)
		{
			string identifier = tokenizer.Identifier;
			tokenizer.NextToken();
			if (tokenizer.Token != E_Token.OpenParens)
			{
				return new NodeVariable(identifier);
			}
			tokenizer.NextToken();
			List<Node> list = new List<Node>();
			if (tokenizer.Token != E_Token.CloseParens)
			{
				while (true)
				{
					list.Add(ParseComparer());
					if (tokenizer.Token != E_Token.Comma)
					{
						break;
					}
					tokenizer.NextToken();
				}
			}
			if (tokenizer.Token != E_Token.CloseParens)
			{
				Debug.LogError((object)"Missing close parenthesis!");
			}
			tokenizer.NextToken();
			return new NodeMethodCall(identifier, list.ToArray());
		}
		Debug.LogError((object)$"Unexpect token: {tokenizer.Token}");
		return null;
	}

	private Node ParseMultiplyDivide()
	{
		Node node = ParseUnary();
		while (true)
		{
			Func<double, double, double> func = null;
			if (tokenizer.Token == E_Token.Multiply)
			{
				func = (double a, double b) => a * b;
			}
			else if (tokenizer.Token == E_Token.Divide)
			{
				func = (double a, double b) => a / b;
			}
			else if (tokenizer.Token == E_Token.Modulo)
			{
				func = (double a, double b) => a % b;
			}
			if (func == null)
			{
				break;
			}
			tokenizer.NextToken();
			Node rightHandSize = ParseUnary();
			node = new NodeBinary(node, rightHandSize, func);
		}
		return node;
	}

	private Node ParseUnary()
	{
		while (true)
		{
			switch (tokenizer.Token)
			{
			case E_Token.Add:
				break;
			case E_Token.Subtract:
				tokenizer.NextToken();
				return new NodeUnary(ParseUnary(), (double a) => 0.0 - a);
			default:
				return ParseLeaf();
			}
			tokenizer.NextToken();
		}
	}
}
