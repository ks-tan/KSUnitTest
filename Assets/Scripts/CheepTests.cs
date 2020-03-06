﻿#if UNITY_EDITOR

using KSUnitTest;
using Cheep;
using UnityEngine;
using System.Text;

[TestClass]
public class CheepLexerTests
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

			if (token.Type == SyntaxType.EOFToken)
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
	public static void Parser_CreateExpressionTree()
	{
		string text = "1 + 2 + 3";
		var parser = new Parser(text);
		var expression = parser.Parse();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder
			.AppendLine("BinaryExpression")
			.AppendLine("....BinaryExpression")
			.AppendLine("........NumberExpression")
			.AppendLine("............NumberToken, 1")
			.AppendLine("........PlusToken")
			.AppendLine("........NumberExpression")
			.AppendLine("............NumberToken, 2")
			.AppendLine("....PlusToken")
			.AppendLine("....NumberExpression")
			.AppendLine("........NumberToken, 3");
		UnitTest.Assert.AreEqual(stringBuilder.ToString(), PrettifyExpression(expression));
	}

	private static string PrettifyExpression(SyntaxNode node, string indent = "")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(indent + node.Type);
		if (node is SyntaxToken t && t.Value != null) stringBuilder.Append(", " + t.Value);
		stringBuilder.AppendLine();
		indent += "....";
		foreach (var child in node.GetChildren()) stringBuilder.Append(PrettifyExpression(child, indent));
		return stringBuilder.ToString();
	}
}

#endif