# KSUnitTest

A lightweight, simple-to-use unit testing tool for Unity game engine. You can use this to test data-driven components of your game.

## Quick start

You will need to include the script `KSUnitTest.cs` in your Unity project. Then, create a new C# script in your project (e.g. `SampleTestClass.cs`).

Here is an example on how you can create a unit test class with KSUnitTest.

```c#
// You will want to wrap your unit test code in the #if UNITY_EDITOR preprocessor directive so that it will not get compiled into your game's production build
#if UNITY_EDITOR

using KSUnitTest; // include this namespace to use classes and methods in KSUnitTest

// Assign a 'TestClass' attribute to your test class
[TestClass]
public class SampleTestClass
{
  private static int _expected;

  // Assign a 'TestSetup' attribute for the set up method that will run before the unit test methods
  // Note that this must be a STATIC method
  [TestSetup]
  public static void Setup()
  {
    _expected = 100;
  }

  // Assign a 'TestMethod' attribute for all your unit test methods
  // Note that this must be a STATIC method
  [TestMethod]
  public static void AreEquals_100_Multiply()
  {
    // There is a list of comparison methods in the UnitTest class, which outputs the assertion result to console
    UnitTest.Assert.AreEqual(_expected, 10 * 90);
  }
  
  // It is highly recommended that you reset the state of your unit test class when the unit tests end. This can be done by assigning the "TestTeardown" attribute to your teardown method
  // Note that this must be a STATIC method
  [TestTeardown]
  public static void Teardown()
  {
    _expetced = 0;
  }  
}

#endif
```
To run the unit tests, go to Unity's menu and find `KS Tools > Unit Test Runner`. It will open up an editor window that allows you to run all unit tests together, or individually by each unit test class.

## Public methods

### UnitTest

A set of methods that allows you control of the unit testing process apart from using the Unit Test Runner editor window.

```c#
public static IEnumerable<Type> GetTestClasses() {...}

public static void RunUnitTests() {...}
```

### UnitTest.Assert

A set of assertion methods for comparing obejcts and literals of different types (more would be coming soon).

```c#
// For comparing simple obejcts and literal values (e.g. int, float, string, etc)
public static bool AreEqual<T>(T expected, T received, bool shouldShowSuccessLog = false) {...}

// For comparing any objects that implements the IEnumerable<T> interface, e.g. arrays, lists, etc.
public static bool AreSequencesEqual<T>(IEnumerable<T> expected, IEnumerable<T> received, bool shouldShowSuccessLog = false) {...}
```

### Console

A convenient set of methods for formatting and logging your debug messages to the console.

```c#
public static void Log(string logMessage, string color = null) {...}

public static string AddBold(string message) {...}

public static string AddColor(string message, string color) {...}
```

## Why would I need this?

You don't need this if you are comfortable with using Unity's own [Unity Test Runner](https://docs.unity3d.com/2019.1/Documentation/Manual/testing-editortestsrunner.html).

However there have been much complaints and confusion around using it, especially dealing with assembly definition files. The links below will provide some context to the problem.

https://answers.unity.com/questions/1540387/missing-reference-between-assembly-csharp-and-test.html
https://answers.unity.com/questions/1627039/unity-test-runner-unable-to-reference-my-code-from.html

They might not be huge problems. But for myself, rather than making big changes to the configurations of the project (which might already be considerably complex when it's already halfway through development), simply pasting a lightweight, standalone script to make unit testing work is definitely a very appealing option.

## Future work and collaboration

I will be including more assertion methods in the future.

You may submit issues or make pull requests to the repository. If you have further questions, you may contact me at tankangsoon@gmail.com

Thank you.
