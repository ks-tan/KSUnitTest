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

	internal abstract class BoundNode
	{
		public abstract BoundNodeKind Kind { get; }
	}

	internal abstract class BoundExpression : BoundNode
	{
		public abstract Type Type { get; }
	}

	internal sealed class BoundLiteralExpression : BoundExpression
	{
		public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
		public override Type Type => Value.GetType();
		public object Value { get; }
		public BoundLiteralExpression(object inValue) => Value = inValue;
	}

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

	internal sealed class Binder
	{
		private readonly List<string> _diagnostics = new List<string>();
		public IEnumerable<string> Diagnostitcs => _diagnostics;

		public BoundExpression BindExpression(ExpressionSyntax inSyntax)
		{
			switch(inSyntax.Kind)
			{
				case SyntaxKind.LiteralExpression: return BindLiteralExpression((LiteralExpressionSyntax)inSyntax);
				case SyntaxKind.UnaryExpression: return BindUnaryExpression((UnaryExpressionSyntax)inSyntax);
				case SyntaxKind.BinaryExpression: return BindBinaryExpression((BinaryExpressionSyntax)inSyntax);
				default: throw new Exception("Unexpected syntax " + inSyntax.Kind);
			}
		}

		private BoundExpression BindLiteralExpression(LiteralExpressionSyntax inSyntax)
		{
			var value = inSyntax.LiteralToken.Value as int? ?? 0;
			return new BoundLiteralExpression(value);
		}

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
