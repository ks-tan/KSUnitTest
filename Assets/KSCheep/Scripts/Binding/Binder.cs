using KSCheep.CodeAnalysis.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSCheep.CodeAnalysis.Binding
{
	internal enum BoundNodeKind
	{
		LiteralExpression,
		UnaryExpression,
		BinaryExpression
	}

	internal enum BoundUnaryOperatorKind
	{
		Identity,
		Negation
	}

	internal enum BoundBinaryOperatorKind
	{
		Addition,
		Subtraction,
		Multiplication,
		Division
	}

	/// <summary>
	/// A node in our bound tree
	/// </summary>
	internal abstract class BoundNode
	{
		public abstract BoundNodeKind Kind { get; }
	}

	/// <summary>
	/// A bound expression is a node in the bound tree. It also has a "Type".
	/// </summary>
	internal abstract class BoundExpression : BoundNode
	{
		public abstract Type Type { get; }
	}

	/// <summary>
	///  Defining a literal bound expression
	/// </summary>
	internal sealed class BoundLiteralExpression : BoundExpression
	{
		public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
		public override Type Type => Value.GetType();
		public object Value { get; }
		public BoundLiteralExpression(object inValue) => Value = inValue;
	}

	/// <summary>
	/// Defining a unary bound expression
	/// </summary>
	internal sealed class BoundUnaryExpression : BoundExpression
	{
		public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
		public override Type Type => Operand.Type;
		public BoundUnaryOperatorKind OperatorKind { get; }
		public BoundExpression Operand { get; }

		public BoundUnaryExpression(BoundUnaryOperatorKind inOperatorKind, BoundExpression inOperand)
		{
			OperatorKind = inOperatorKind;
			Operand = inOperand;
		}
	}

	/// <summary>
	/// Defining a binary bound express
	/// </summary>
	internal sealed class BoundBinaryExpression : BoundExpression
	{
		public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
		public override Type Type => Left.Type;
		public BoundExpression Left { get; }
		public BoundBinaryOperatorKind OperatorKind { get; }
		public BoundExpression Right { get; }

		public BoundBinaryExpression(BoundExpression inLeft, BoundBinaryOperatorKind inOperatorKind, BoundExpression inRight)
		{
			Left = inLeft;
			OperatorKind = inOperatorKind;
			Right = inRight;
		}
	}

	/// <summary>
	/// Apart from the syntax tree, we are creating a new tree known as the Bound Tree (intermediate representation, or annotated abstract syntax trees)
	/// This helps us resolve the "Type" (as in "int", "string", etc..) of node on the bound tree (i.e. expressions and etc)
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
