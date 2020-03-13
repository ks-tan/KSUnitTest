using System;
using KSCheep.CodeAnalysis.Binding;
using KSCheep.CodeAnalysis.Syntax;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Evaluates a syntax tree
	/// </summary>
	internal sealed class Evaluator
	{
		private readonly BoundExpression _root;

		/// <summary>
		/// To construct the evaluator, we pass in the root syntax node of the syntax tree we wish to evaluate
		/// </summary>
		public Evaluator(BoundExpression inRoot) => _root = inRoot;

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		public int Evaluate() => EvaluateExpression(_root);

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		private int EvaluateExpression(BoundExpression inNode)
		{
			// Evaluate a number expression. Just returns the number token
			if (inNode is BoundLiteralExpression number)
			{
				return (int)number.Value;
			}

			// Evaluate a unary expression. Just returns the negative value of operand if operator is '-'
			if (inNode is BoundUnaryExpression unary)
			{
				var operandExpression = EvaluateExpression(unary.Operand);

				switch (unary.OperatorKind)
				{
					case BoundUnaryOperatorKind.Identity: return operandExpression;
					case BoundUnaryOperatorKind.Negation: return -operandExpression;
					default: throw new Exception("Unexpected unary operator " + unary.OperatorKind);
				}
			}

			// Evaluate the binary expression. Gets the left and right expression, identify the operator token between them, and perform operation
			if (inNode is BoundBinaryExpression binary)
			{
				var leftExpression = EvaluateExpression(binary.Left);
				var rightExpression = EvaluateExpression(binary.Right);

				switch(binary.OperatorKind)
				{
					case BoundBinaryOperatorKind.Addition:			return leftExpression + rightExpression;
					case BoundBinaryOperatorKind.Subtraction:		return leftExpression - rightExpression;
					case BoundBinaryOperatorKind.Multiplication:	return leftExpression * rightExpression;
					case BoundBinaryOperatorKind.Division:			return leftExpression / rightExpression;
					default: throw new Exception("Unexpected binary operator " + binary.OperatorKind);
				}
			}

			// Evaluate a parenthesised expression. We only evaluate the "expression" within the parenthesis
			//if (inNode is ParenthesisedExpressionSyntax parenthesisedExpression)
			//{
			//	return EvaluateExpression(parenthesisedExpression.Expression);
			//}

			throw new Exception("Unexpected node " + inNode.Kind);
		}
	}
}