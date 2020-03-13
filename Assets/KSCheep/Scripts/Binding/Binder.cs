using KSCheep.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;

namespace KSCheep.CodeAnalysis.Binding
{
	/// <summary>
	/// Apart from the syntax tree, we are creating a new tree known as the Bound Tree to do what is known as "binding".
	/// In an expression like a + b, we know we are adding a and b, but we don’t know what those names refer to. Are they local variables? Global? Where are they defined?
	/// The first bit of analysis that most languages do is called binding or resolution. For each identifier we find out where that name is defined and wire the two together.
	/// </summary>
	internal sealed class Binder
	{
		private readonly List<string> _diagnostics = new List<string>();
		public IEnumerable<string> Diagnostitcs => _diagnostics;

		/// <summary>
		/// Binding an expression based on the expression's syntax kind
		/// </summary>
		public BoundExpression BindExpression(ExpressionSyntax inSyntax)
		{
			switch(inSyntax.Kind)
			{
				case SyntaxKind.LiteralExpression: return BindLiteralExpression((LiteralExpressionSyntax)inSyntax); // Just assingns a value
				case SyntaxKind.UnaryExpression: return BindUnaryExpression((UnaryExpressionSyntax)inSyntax); // binds operand recursively while evaluating the unary operator kind of each bound expression
				case SyntaxKind.BinaryExpression: return BindBinaryExpression((BinaryExpressionSyntax)inSyntax); // binds operand recursively while evaluating the binary operator kind of each bound expression
				default: throw new Exception("Unexpected syntax " + inSyntax.Kind);
			}
		}

		/// <summary>
		/// Binding a literal expression. Just assigns a value
		/// </summary>
		private BoundExpression BindLiteralExpression(LiteralExpressionSyntax inSyntax)
		{
			var value = inSyntax.LiteralToken.Value as int? ?? 0;
			return new BoundLiteralExpression(value);
		}

		/// <summary>
		/// Binding a unary expression. Binds operand recursively while evaluating the unary operator kind of each bound expression
		/// </summary>
		private BoundExpression BindUnaryExpression(UnaryExpressionSyntax inSyntax)
		{
			var boundOperand = BindExpression(inSyntax.OperandExpression);
			var boundOperatorKind = BindUnaryOperatorKind(inSyntax.OperatorToken.Kind, boundOperand.Type);

			if (boundOperatorKind == null)
			{
				_diagnostics.Add("Unary operator " + inSyntax.OperatorToken.Text + " is not defined for type " + boundOperand.Type);
				return boundOperand;
			}

			return new BoundUnaryExpression(boundOperatorKind.Value, boundOperand);
		}

		/// <summary>
		/// Binding a binary expression. Binds operand recursively while evaluating the binary operator kind of each bound expression
		/// </summary>
		private BoundExpression BindBinaryExpression(BinaryExpressionSyntax inSyntax)
		{
			var boundLeft = BindExpression(inSyntax.LeftExpression);
			var boundRight = BindExpression(inSyntax.RightExpression);
			var boundOperatorKind = BindBinaryOperatorKind(inSyntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

			if (boundOperatorKind == null)
			{
				_diagnostics.Add("Binary operator " + inSyntax.OperatorToken.Text + " is not defined for type " + boundLeft.Type + " and " + boundRight.Type);
				return boundLeft;
			}

			return new BoundBinaryExpression(boundLeft, boundOperatorKind.Value, boundRight);
		}

		/// <summary>
		/// Determining the unary operator kind of the bound expression by checking the syntax kind of the unary operator
		/// </summary>
		private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind inKind, Type inOperandType)
		{
			if (inOperandType != typeof(int))
				return null;

			switch (inKind)
			{
				case SyntaxKind.PlusToken: return BoundUnaryOperatorKind.Identity;
				case SyntaxKind.MinusToken: return BoundUnaryOperatorKind.Negation;
				default:  throw new Exception("Unexpected unary operator " + inKind);
			}
		}

		/// <summary>
		/// Determining the binary operator kind of the bound expression by checking if left and right are of the right type, and the syntax kind of the binary operator
		/// </summary>
		private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind inKind, Type inLeftType, Type inRightType)
		{
			if (inLeftType != typeof(int) || inRightType != typeof(int))
				return null;

			switch (inKind)
			{
				case SyntaxKind.PlusToken: return BoundBinaryOperatorKind.Addition;
				case SyntaxKind.MinusToken: return BoundBinaryOperatorKind.Subtraction;
				case SyntaxKind.StarToken: return BoundBinaryOperatorKind.Multiplication;
				case SyntaxKind.SlashToken: return BoundBinaryOperatorKind.Division;
				default: throw new Exception("Unexpected unary operator " + inKind);
			}
		}
	}
}
