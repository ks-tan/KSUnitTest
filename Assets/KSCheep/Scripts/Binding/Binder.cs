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
}
