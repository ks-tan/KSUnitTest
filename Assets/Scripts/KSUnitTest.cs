using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KSUnitTest
{
	public class UnitTest
	{
		/// <summary>
		/// For keeping track number of failed test cases
		/// </summary>
		private static int _failCount = 0;

		/// <summary>
		/// For getting classes with the "KSTestClass" attribute
		/// </summary>
		private static IEnumerable<Type> GetTestClasses()
		{
			foreach (Type type in typeof(UnitTest).Assembly.GetTypes())
			{
				if (type.GetCustomAttributes(typeof(TestClass), false).Length > 0)
				{
					yield return type;
				}
			}
		}

		/// <summary>
		/// For getting a list of methods of a certain attribute in a certain class type
		/// </summary>
		private static void RunMethods(Type type, Type attribute)
		{
			foreach (MethodInfo method in type.GetMethods())
			{
				if (method.GetCustomAttribute(attribute, false) != null && method.IsStatic)
				{
					method.Invoke(null, null);
				}
			}
		}

		/// <summary>
		/// Exposing the method to run all unit tests in Unity's menu bar
		/// </summary>
		[MenuItem("KS Tools/Run Unit Tests")]
		private static void RunUnitTests()
		{
			IEnumerable<Type> testClasses = GetTestClasses();

			foreach (Type testClass in testClasses)
			{
				RunMethods(testClass, typeof(TestSetup));
				RunMethods(testClass, typeof(TestMethod));
				RunMethods(testClass, typeof(TestTeardown));
			}

			string result = Console.AddBold(_failCount == 0 ? "All unit tests passed!" : _failCount + " unit tests failed!");
			Console.Log(result, _failCount == 0 ? "green" : "red");

			_failCount = 0; // Clean test result when all tests are completed
		}

		/// <summary>
		///  Contains unit test assertion methods
		/// </summary>
		public class Assert
		{
			/// <summary>
			/// Simple comparison between objects and literal values
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="expected"></param>
			/// <param name="received"></param>
			/// <param name="shouldShowSuccessLog"></param>
			/// <returns></returns>
			public static bool AreEqual<T>(T expected, T received, bool shouldShowSuccessLog = false)
			{
				bool success = expected.Equals(received);

				if (!success || shouldShowSuccessLog)
				{
					string methodName = Console.AddBold("[" + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name + "]");
					string message = success ? "Passed!" : "Failed! Expected: " + expected + ". Received: " + received;
					Console.Log(methodName + " " + message, success ? "green" : "red");
				}

				_failCount += success ? 0 : 1;

				return success;
			}

			/// <summary>
			/// Comparison between 2 IEnumerable<T>
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="expected"></param>
			/// <param name="received"></param>
			/// <param name="shouldShowSuccessLog"></param>
			/// <returns></returns>
			public static bool AreSequencesEqual<T>(IEnumerable<T> expected, IEnumerable<T> received, bool shouldShowSuccessLog = false)
			{
				bool success = expected.SequenceEqual(received);

				if (!success || shouldShowSuccessLog)
				{
					string methodName = Console.AddBold("[" + new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name + "]");
					string expectedEnumerable = "", receivedEnumerable = "";
					foreach (T expect in expected) expectedEnumerable += expect + ", ";
					foreach (T receive in received) receivedEnumerable += receive + ", ";
					expectedEnumerable = "[" + expectedEnumerable + "]";
					receivedEnumerable = "[" + receivedEnumerable + "]";
					string message = success ? "Passed!" : "Failed! Expected: " + expectedEnumerable + ". Received: " + receivedEnumerable;
					Console.Log(methodName + " " + message, success ? "green" : "red");
				}

				_failCount += success ? 0 : 1;

				return success;
			}
		}
	}

	/// <summary>
	/// For formatting debug log messages conveniently
	/// </summary>
	public class Console
	{
		public static void Log(string logMessage, string color = null)
		{
			Debug.Log(AddColor(logMessage, color));
		}

		public static string AddBold(string message)
		{
			return "<b>" + message + "</b>";
		}

		public static string AddColor(string message, string color)
		{
			if (!string.IsNullOrEmpty(color))
			{
				return "<color=" + color + ">" + message + "</color>";
			}

			return message;
		}
	}

	/// <summary>
	/// To be assigned classes with test methods
	/// </summary>
	public class TestClass : Attribute { }

	/// <summary>
	/// To be assigned to STATIC methods that set up the state of the test class before unit tests commence
	/// </summary>
	public class TestSetup : Attribute { }

	/// <summary>
	/// To be assigned to STATIC methods that call any UnitTest.Assert methods
	/// </summary>
	public class TestMethod : Attribute { }

	/// <summary>
	/// To be assigned to STATIC methods that resets the state of the test class afetr unit tests finish
	/// </summary>
	public class TestTeardown : Attribute { }

}
