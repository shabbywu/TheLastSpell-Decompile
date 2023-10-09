namespace TheLastStand.Framework.ExpressionInterpreter;

public enum E_Token
{
	EOF,
	Superior,
	Inferior,
	Equals,
	Different,
	Add,
	Subtract,
	Multiply,
	Divide,
	Modulo,
	OpenParens,
	CloseParens,
	Number,
	Identifier,
	Comma
}
