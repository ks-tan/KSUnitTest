using System;

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
	/// Defining a binary bound expression
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
}
