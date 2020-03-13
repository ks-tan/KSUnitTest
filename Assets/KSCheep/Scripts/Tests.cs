#if UNITY_EDITOR

using KSUnitTest;
using KSCheep.CodeAnalysis;
using KSCheep.CodeAnalysis.Syntax;
using System.Text;
using System.Linq;
using KSCheep.CodeAnalysis.Binding;

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

			if (token.Kind == SyntaxKind.EndOfFileToken)
			{
				break;
			}

			if (tokenIndex == 4)
			{
				UnitTest.Assert.AreEqual(SyntaxKind.NumberToken, token.Kind);
				UnitTest.Assert.AreEqual(2, token.Value);
				break;
			}

			tokenIndex++;
		}
	}

	[TestMethod]
	public static void Parser_ReportDiagnostics()
	{
		string text = "1 + ";
		var syntaxTree = new Parser(text).Parse();
		StringBuilder expectedDiagnostics = new StringBuilder().AppendLine("ERROR: Unexpected token EndOfFileToken, expected NumberToken");
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
		var binder = new Binder();
		var boundExpression = binder.BindExpression(syntaxTree.Root);
		var diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostitcs).ToArray();
		StringBuilder expectedResult = new StringBuilder().AppendLine("13");
		StringBuilder receivedResult = new StringBuilder();

		if (diagnostics.Any())
		{
			foreach(var diagnostic in diagnostics)
			{
				receivedResult.AppendLine(diagnostic);
			}
		}
		else
		{
			var evaluator = new Evaluator(boundExpression);
			var result = evaluator.Evaluate();
			receivedResult.AppendLine(result.ToString());
		}

		UnitTest.Assert.AreEqual(expectedResult.ToString(), receivedResult.ToString());
	}
}

#endif