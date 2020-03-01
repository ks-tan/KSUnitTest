#if UNITY_EDITOR

using KSUnitTest;

/// <summary>
/// A sample of a unit test class.
/// 1. The TestClass attribute must be assigned to the test class.
/// 2. The TestSetup attribute must be assigned to the setup method if setup is required.
/// 3. The TestMethod attribute must be assigned to the unit testing methods.
/// 4. The TestTeardown attribute must be assigned to the teardown method. It is highly recommended that the state of the test class is clean, which can be done through this method.
/// </summary>
[TestClass]
public class SampleTestClass
{
	private static int _expected;
	private static int[] _intArray;

	[TestSetup]
	public static void Setup()
	{
		_expected = 100;
		_intArray = new int[] { 0, 1, 2, 3, 4, 5};
	}

	[TestMethod]
	public static void AreEquals_100_Multiply()
	{
		UnitTest.Assert.AreEqual(_expected, 10 * 90);
	}

	[TestMethod]
	public static void AreEquals_100_Addition()
	{
		UnitTest.Assert.AreEqual(_expected, 10 + 90, shouldShowSuccessLog: true);
	}

	[TestMethod]
	public static void AreEquals_IntArray()
	{
		UnitTest.Assert.AreSequencesEqual(_intArray, new int[] { 0, 1, 2, 3, 4, 5 }, shouldShowSuccessLog: true);
	}

	[TestMethod]
	public static void AreEquals_IntArray_Reversed()
	{
		UnitTest.Assert.AreSequencesEqual(_intArray, new int[] { 5, 4, 3, 2, 1, 0 }, shouldShowSuccessLog: true);
	}

	[TestTeardown]
	public static void Teardown()
	{
		_expected = 0;
		_intArray = null;
	}
}

#endif