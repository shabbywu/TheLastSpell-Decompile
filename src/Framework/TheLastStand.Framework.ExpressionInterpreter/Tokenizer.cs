using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class Tokenizer
{
	private readonly TextReader textReader;

	private char currentChar;

	public string Identifier { get; private set; }

	public double Number { get; private set; }

	public E_Token Token { get; private set; }

	public Tokenizer(string text)
	{
		textReader = new StringReader(text);
		NextChar();
		NextToken();
	}

	public Tokenizer(TextReader textReader)
	{
		this.textReader = textReader;
		NextChar();
		NextToken();
	}

	public void NextToken()
	{
		while (char.IsWhiteSpace(currentChar))
		{
			NextChar();
		}
		switch (currentChar)
		{
		case '\0':
			Token = E_Token.EOF;
			return;
		case '>':
			NextChar();
			Token = E_Token.Superior;
			return;
		case '<':
			NextChar();
			Token = E_Token.Inferior;
			return;
		case '=':
			NextChar();
			Token = E_Token.Equals;
			return;
		case '!':
			NextChar();
			Token = E_Token.Different;
			return;
		case '+':
			NextChar();
			Token = E_Token.Add;
			return;
		case '-':
			NextChar();
			Token = E_Token.Subtract;
			return;
		case '*':
			NextChar();
			Token = E_Token.Multiply;
			return;
		case '/':
			NextChar();
			Token = E_Token.Divide;
			return;
		case '%':
			NextChar();
			Token = E_Token.Modulo;
			return;
		case '(':
			NextChar();
			Token = E_Token.OpenParens;
			return;
		case ')':
			NextChar();
			Token = E_Token.CloseParens;
			return;
		case ',':
			NextChar();
			Token = E_Token.Comma;
			return;
		}
		if (char.IsDigit(currentChar) || currentChar == '.')
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			while (char.IsDigit(currentChar) || (!flag && currentChar == '.'))
			{
				stringBuilder.Append(currentChar);
				flag = currentChar == '.';
				NextChar();
			}
			Number = double.Parse(stringBuilder.ToString(), CultureInfo.InvariantCulture);
			Token = E_Token.Number;
		}
		else if (char.IsLetter(currentChar) || currentChar == '_')
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			while (char.IsLetterOrDigit(currentChar) || currentChar == '_' || currentChar == '.')
			{
				stringBuilder2.Append(currentChar);
				NextChar();
			}
			Identifier = stringBuilder2.ToString();
			Token = E_Token.Identifier;
		}
		else
		{
			Debug.LogError((object)$"Unexpected character: {currentChar}");
		}
	}

	private void NextChar()
	{
		int num = textReader.Read();
		currentChar = ((num >= 0) ? ((char)num) : '\0');
	}
}
