using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Declares the different type of syntax tokens you will find in a piece of text
	/// </summary>
	public enum SyntaxType
	{
		NumberToken, // 12345
		WhitespaceToken, //" "
		PlusToken, // +
		MinusToken, // -
		StarToken, // *
		SlashToken, // "/"
		CloseParenthesisToken, // ")"
		OpenParenthesisToken, // "("
		BadToken, // error/no such token type
		EOFToken, // end of file
		BinaryExpression, // <left expression><operator token><right expression>
		NumberExpression, // <number token>
		ParenthesisedExpression // ( <expression> )
	}

	/// <summary>
	/// Base class for syntax nodes of different types.
	/// Syntax nodes are tokens or expressions on a tree-structure (the syntax tree) showing the order which they will be evaluated
	/// </summary>
	public abstract class SyntaxNode
	{
		/// <summary>
		/// The syntax type of this node, determining what type of token or expression it is
		/// </summary>
		public abstract SyntaxType Type { get; }

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public abstract IEnumerable<SyntaxNode> GetChildren();
	}

	/// <summary>
	/// A token is a unit in an expression that defines its type and value.
	/// For example, "1" is a number of value 1, and "+" is a plus symbol with value null
	/// They are also Syntax Nodes, i.e. the leaves in our syntax tree used in parsing/evaluating the text
	/// </summary>
	public class SyntaxToken : SyntaxNode
	{
		public override SyntaxType Type { get; } // Token type
		public int Position { get; } // Its position on the expression
		public string Text { get; } // Its string representation on the expression
		public object Value { get; } // Its true value, whether it is a number, string, bool, or etc.

		public SyntaxToken(SyntaxType inType, int inPosition, string inText, object inValue)
		{
			Type = inType;
			Position = inPosition;
			Text = inText;
			Value = inValue;
		}

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
	}

	/// <summary>
	/// An abstract class for an expression, which itself is also a node in the expression tree
	/// </summary>
	public abstract class ExpressionSyntax : SyntaxNode {}

	/// <summary>
	/// An expression that holds a number token
	/// </summary>
	sealed class NumberExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.NumberExpression;
		public SyntaxToken NumberToken { get; }
		public NumberExpressionSyntax(SyntaxToken inNumberToken) => NumberToken = inNumberToken;

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return NumberToken;
		}
	}

	/// <summary>
	/// An expression that looks like <left-expression><operator-token><right-expression>
	/// </summary>
	sealed class BinaryExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.BinaryExpression;
		public ExpressionSyntax LeftExpression { get; }
		public SyntaxToken OperatorToken { get; }
		public ExpressionSyntax RightExpression { get; }

		public BinaryExpressionSyntax(ExpressionSyntax inLeftExpression, SyntaxToken inOperatorToken, ExpressionSyntax inRightExpression)
		{
			LeftExpression = inLeftExpression;
			OperatorToken = inOperatorToken;
			RightExpression = inRightExpression;
		}

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return LeftExpression;
			yield return OperatorToken;
			yield return RightExpression;
		}
	}

	/// <summary>
	/// An expression that looks like ( <expression> )
	/// </summary>
	sealed class ParenthesisedExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.ParenthesisedExpression;
		public SyntaxToken OpenParenthesisToken { get; }
		public ExpressionSyntax Expression { get; }
		public SyntaxToken CloseParenthesisToken { get; }

		public ParenthesisedExpressionSyntax(SyntaxToken inOpenParenthesisToken, ExpressionSyntax inExpression, SyntaxToken inCloseParenthesisToken)
		{
			OpenParenthesisToken = inOpenParenthesisToken;
			Expression = inExpression;
			CloseParenthesisToken = inCloseParenthesisToken;
		}

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return OpenParenthesisToken;
			yield return Expression;
			yield return CloseParenthesisToken;
		}
	}

	/// <summary>
	/// Identifies the starting syntax node (an expression syntax) and the ending node (an EOF Syntax Token) of the syntax tree
	/// It also acts as the final collection point of all the diagnostic logs from the lexing and parsing of tokens and syntax trees
	/// </summary>
	public sealed class SyntaxTree
	{
		public IReadOnlyList<string> Diagnostics { get; }
		public ExpressionSyntax Root { get; }
		public SyntaxToken EOFToken { get; }

		public SyntaxTree(IEnumerable<string> inDiagnostics, ExpressionSyntax inRoot, SyntaxToken inEOFToken)
		{
			Diagnostics = inDiagnostics.ToArray();
			Root = inRoot;
			EOFToken = inEOFToken;
		}

		/// <summary>
		/// Parse a syntax tree from a piece of text
		/// </summary>
		public static SyntaxTree Parse(string text) => new Parser(text).Parse();
	}
}