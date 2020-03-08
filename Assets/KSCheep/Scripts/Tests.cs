#if UNITY_EDITOR

using KSUnitTest;
using KSCheep.CodeAnalysis;
using System.Text;
using System.Linq;

[TestClass]
public class CheepLexerParserTests
{
	[TestMethod]
	public static void Lexer_CreateTokens()
	{
		string text = "1 + 2 + 3";
		var lexer = new Lexer(text);
		int tokenIndex = 0;

		while (true)
		{
			SyntaxToken token = lexer.NextToken();

			if (token.Type == SyntaxType.EndOfFileToken)
			{
				break;
			}

			if (tokenIndex == 4)
			{
				UnitTest.Assert.AreEqual(SyntaxType.NumberToken, token.Type);
				UnitTest.Assert.AreEqual(2, token.Value);
				break;
			}

			tokenIndex++;
		}
	}

	[TestMethod]
	public static void Parser_CreateSyntaxTree()
	{
		string text = "1 + 2 + 3";
		var parser = new Parser(text);
		var syntaxTree = parser.Parse();

		StringBuilder expectedSyntaxTree = new StringBuilder()
			.AppendLine("└──BinaryExpression")
			.AppendLine("    ├──BinaryExpression")
			.AppendLine("    │   ├──NumberExpression")
			.AppendLine("    │   │   └──NumberToken, 1")
			.AppendLine("    │   ├──PlusToken")
			.AppendLine("    │   └──NumberExpression")
			.AppendLine("    │       └──NumberToken, 2")
			.AppendLine("    ├──PlusToken")
			.AppendLine("    └──NumberExpression")
			.AppendLine("        └──NumberToken, 3");

		UnitTest.Assert.AreEqual(expectedSyntaxTree.ToString(), Parser.PrettifySyntaxTree(syntaxTree.Root));
	}

	[TestMethod]
	public static void Parser_ReportDiagnostics()
	{
		string text = "1 + ";
		var syntaxTree = new Parser(text).Parse();
		StringBuilder expectedDiagnostics = new StringBuilder().AppendLine("ERROR: Unexpected token EOFToken, expected NumberToken");
		StringBuilder receivedDiagnostics = new StringBuilder();

		if (syntaxTree.Diagnostics.Any())
		{
			foreach (var diagnostic in syntaxTree.Diagnostics)
			{
				receivedDiagnostics.AppendLine(diagnostic);
			}
		}

		UnitTest.Assert.AreEqual(expectedDiagnostics.ToString(), receivedDiagnostics.ToString());
	}

	[TestMethod]
	public static void Evaluator_EvaluateExpression()
	{
		string text = "4 + (1 + 2) * 3";
		var syntaxTree = SyntaxTree.Parse(text);
		StringBuilder expectedResult = new StringBuilder().AppendLine("13");
		StringBuilder receivedResult = new StringBuilder();

		if (syntaxTree.Diagnostics.Any())
		{
			foreach(var diagnostic in syntaxTree.Diagnostics)
			{
				receivedResult.AppendLine(diagnostic);
			}
		}
		else
		{
			var evaluator = new Evaluator(syntaxTree.Root);
			var result = evaluator.Evaluate();
			receivedResult.AppendLine(result.ToString());
		}

		UnitTest.Assert.AreEqual(expectedResult.ToString(), receivedResult.ToString());
	}
}

#endif