using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KSUnitTest
{
	public class Menu
	{
		[MenuItem("KS Tools/Run Unit Tests")]
		static void RunUnitTests()
		{
			Assert.AreEquals("asdasdsad", "asdasdsad", true);
			Assert.AreEquals("asd", "asdasdasd", true);
			Assert.AreEquals(2, 1 + 1, true);
			Assert.AreEquals(3, 1 + 1, true);
		}
	}

	public class Assert
	{
		public static bool AreEquals<T>(T expected, T received, bool shouldShowSuccessLog = false)
		{
			bool success = expected.Equals(received);

			if (!success || shouldShowSuccessLog)
			{
				string message = success ? "Passed!" : "Failed! Expected: " + expected + ". Received: " + received;
				KSDebug.Log(message, success ? "green" : "red");
			}

			return success;
		}
	}
}

public class KSTestClass : Attribute { }

public class KSTestSetup : Attribute { }

public class KSTestMethod : Attribute { }

public class KSTestTeardown : Attribute { }
