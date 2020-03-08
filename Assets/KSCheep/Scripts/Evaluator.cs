using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Evaluates an expression syntax
	/// </summary>
	public sealed class Evaluator
	{
		private readonly ExpressionSyntax _root;

		/// <summary>
		/// To construct the evaluator, we pass in the root syntax node of the syntax tree we wish to evaluate
		/// </summary>
		public Evaluator(ExpressionSyntax inRoot) => _root = inRoot;

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		/// <returns></returns>
		public int Evaluate() => EvaluateExpression(_root);

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		private int EvaluateExpression(ExpressionSyntax inNode)
		{
			// Evaluate a number expression. Just returns the number token
			if (inNode is LiteralExpressionSyntax number)
			{
				return (int)number.LiteralToken.Value;
			}

			// Evaluate the binary expression. Gets the left and right expression, identify the operator token between them, and perform operation
			if (inNode is BinaryExpressionSyntax binary)
			{
				var leftExpression = EvaluateExpression(binary.LeftExpression);
				var rightExpression = EvaluateExpression(binary.RightExpression);
				if (binary.OperatorToken.Type == SyntaxType.PlusToken) return leftExpression + rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.MinusToken) return leftExpression - rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.StarToken) return leftExpression * rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.SlashToken) return leftExpression / rightExpression;
				else throw new Exception("Unexpected binary operator " + binary.OperatorToken.Type);
			}

			// Evaluate a parenthesised expression. We only evaluate the "expression" within the parenthesis
			if (inNode is ParenthesisedExpressionSyntax parenthesisedExpression)
			{
				return EvaluateExpression(parenthesisedExpression.Expression);
			}

			throw new Exception("Unexpected node " + inNode.Type);
		}
	}
}