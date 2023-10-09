using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class ExpressionInterpreterTester : MonoBehaviour
{
	private class MyClass
	{
		public float A { get; set; }

		public float B { get; set; }

		public float Add(float a, float b)
		{
			return a + b;
		}
	}

	private class MyClassContainingContext : MyClass
	{
		public InterpreterContext InterpreterContext { get; private set; }

		public MyClassContainingContext()
		{
			InterpreterContext = new InterpreterContext(this);
		}
	}

	public static void TestExpressionInterpreter()
	{
		Debug.Log((object)"Starts of tests");
		MyClass myClass = new MyClass();
		myClass.A = 10f;
		myClass.B = 20f;
		new InterpreterContext(myClass);
		new MyClassContainingContext
		{
			A = 45f,
			B = 15f
		};
		Debug.Log((object)"End of tests");
	}

	private void Start()
	{
		TestExpressionInterpreter();
	}
}
