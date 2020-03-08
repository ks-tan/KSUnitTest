using System;
using KSCheep.CodeAnalysis.Syntax;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Evaluates a syntax tree
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

			// Evaluate a unary expression. Just returns the negative value of operand if operator is '-'
			if (inNode is UnaryExpressionSyntax unary)
			{
				var operandExpression = EvaluateExpression(unary.OperandExpression);
				if (unary.OperatorToken.Kind == SyntaxKind.MinusToken) return -operandExpression;
				else if (unary.OperatorToken.Kind == SyntaxKind.PlusToken) return operandExpression;
				else throw new Exception("Unexpected unary operator " + unary.OperatorToken.Kind);
			}

			// Evaluate the binary expression. Gets the left and right expression, identify the operator token between them, and perform operation
			if (inNode is BinaryExpressionSyntax binary)
			{
				var leftExpression = EvaluateExpression(binary.LeftExpression);
				var rightExpression = EvaluateExpression(binary.RightExpression);
				if (binary.OperatorToken.Kind == SyntaxKind.PlusToken) return leftExpression + rightExpression;
				else if (binary.OperatorToken.Kind == SyntaxKind.MinusToken) return leftExpression - rightExpression;
				else if (binary.OperatorToken.Kind == SyntaxKind.StarToken) return leftExpression * rightExpression;
				else if (binary.OperatorToken.Kind == SyntaxKind.SlashToken) return leftExpression / rightExpression;
				else throw new Exception("Unexpected binary operator " + binary.OperatorToken.Kind);
			}

			// Evaluate a parenthesised expression. We only evaluate the "expression" within the parenthesis
			if (inNode is ParenthesisedExpressionSyntax parenthesisedExpression)
			{
				return EvaluateExpression(parenthesisedExpression.Expression);
			}

			throw new Exception("Unexpected node " + inNode.Kind);
		}
	}
}