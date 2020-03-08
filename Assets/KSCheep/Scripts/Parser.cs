using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Parses the tokens in a text (i.e. making sense of a series of tokens)
	/// This means going through each token in the piece of text and placing them in a syntax tree based on their syntax type.
	/// Their position on the syntax tree will determine the order which they will be evaluated (e.g. parenthesised expressions first, and etc)
	/// </summary>
	public sealed class Parser
	{
		private readonly SyntaxToken[] _tokens;
		private int _position;
		private List<string> _diagnostics = new List<string>();

		/// <summary>
		/// During construction, we will split the text into an array of tokens
		/// </summary>
		public Parser(string inText)
		{
			var lexer = new Lexer(inText);
			var tokens = new List<SyntaxToken>();
			SyntaxToken token;

			do
			{
				token = lexer.NextToken();
				if (token.Type != SyntaxType.WhitespaceToken && token.Type != SyntaxType.BadToken) tokens.Add(token);

			}  while (token.Type != SyntaxType.EndOfFileToken);

			_tokens = tokens.ToArray();

			// Adding diagnostics from lexer to parser's diagnostics list
			_diagnostics.AddRange(lexer.Diagnostics);
		}

		/// <summary>
		/// Returns a collection of diagnostics messages collected so far during parsing of text
		/// </summary>
		public IEnumerable<string> Diagnostics => _diagnostics;

		/// <summary>
		/// Peeking ahead, relative to current _position in the tokens array
		/// </summary>
		private SyntaxToken Peek(int inOffset)
		{
			var index = _position + inOffset;
			if (index >= _tokens.Length) return _tokens[_tokens.Length - 1];
			return _tokens[index];
		}

		/// <summary>
		/// Getting the current token in the tokens array
		/// </summary>
		private SyntaxToken CurrentToken => Peek(0);

		/// <summary>
		/// Getting the next token in the tokens array
		/// </summary>
		private SyntaxToken NextToken()
		{
			var current = CurrentToken;
			_position++;
			return current;
		}

		/// <summary>
		/// Get the next token in the tokens array if it's of a certain type. Else return error
		/// </summary>
		private SyntaxToken MatchToken(SyntaxType inType)
		{
			if (CurrentToken.Type == inType) return NextToken();
			_diagnostics.Add("ERROR: Unexpected token " + CurrentToken.Type + ", expected " + inType);
			return new SyntaxToken(inType, CurrentToken.Position, null, null);
		}

		/// <summary>
		/// Parse the entire syntax tree, starting from the first syntax node
		/// </summary>
		public SyntaxTree Parse()
		{
			var expression = ParseExpression();
			var eofToken = MatchToken(SyntaxType.EndOfFileToken);
			return new SyntaxTree(_diagnostics, expression, eofToken);
		}

		/// <summary>
		/// Building the expression tree based on precedence of operators
		/// "High precedence" roughly means "should be calculated first", e.g. * operator VS + operator
		/// Syntax nodes with a high precedence will be placed first (lower) in the syntax tree, as syntax trees are evaluated from bottom up
		/// </summary>
		private ExpressionSyntax ParseExpression(int inParentPrecedence = 0)
		{
			ExpressionSyntax left;
			var unaryOperatorPrecedence = CurrentToken.Type.GetUnaryOperatorPrecedence();

			// If current token is a unary operator (i.e. UnaryOperatorPrecedenc != 0) and it is more than parent precedence, we put it first (i.e. lower) in the syntax tree
			if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= inParentPrecedence)
			{
				var operatorToken = NextToken();
				var operandExpression = ParseExpression(unaryOperatorPrecedence);
				left = new UnaryExpressionSyntax(operatorToken, operandExpression);
			}
			else // The left expression must be a primary expression
			{
				left = ParsePrimaryExpression();
			}

			// After settling the unary expression, if there exists more tokens after it (i.e. an operator and right expression), we shall parse this "binary expression"
			while (true)
			{
				var precedence = CurrentToken.Type.GetBinaryOperatorPrecedence(); // Getting the precedence of the current token
				if (precedence == 0 || precedence < inParentPrecedence) break; // If precedence = 0 (i.e. no tokens), or if it is less than the parent (not part of current expression, but the next one), we shall break
				var operatorToken = NextToken(); // Get current token and proceed to the next one
				var right = ParseExpression(precedence); // Recursively parse the "right-side" of the binary expression
				left = new BinaryExpressionSyntax(left, operatorToken, right); // Now with the left, operator, and right sides of the expression, we shall group (or "collapse") them all together as the same "node" (i.e. left) as a BinaryExpressionSyntax
			}

			return left;
		}

		/// <summary>
		/// Parses a primary expression.
		/// Primary expressions are building blocks of more complex expressions. They are literals, name and names qualified by a "scope-resolution" oeprator (i.e. parenthesises)
		/// </summary>
		private ExpressionSyntax ParsePrimaryExpression()
		{
			if (CurrentToken.Type == SyntaxType.OpenParenthesisToken)
			{
				var openParenthesisToken = NextToken();
				var expression = ParseExpression();
				var closeParenthesisToken = MatchToken(SyntaxType.CloseParenthesisToken);
				return new ParenthesisedExpressionSyntax(openParenthesisToken, expression, closeParenthesisToken);
			}

			var numberToken = MatchToken(SyntaxType.NumberToken);
			return new LiteralExpressionSyntax(numberToken);
		}

		/// <summary>
		/// For debugging purpose, you can always pass in a syntax node to view its syntax tree
		/// </summary>
		public static string PrettifySyntaxTree(SyntaxNode inNode, StringBuilder inStringBuilder = null, string inIndent = "", bool inIsLast = true)
		{
			var indentMarker = inIsLast ? "└──" : "├──";
			inStringBuilder = inStringBuilder == null ? new StringBuilder() : inStringBuilder;
			inStringBuilder.Append(inIndent + indentMarker + inNode.Type);
			if (inNode is SyntaxToken t && t.Value != null) inStringBuilder.Append(", " + t.Value);
			inStringBuilder.AppendLine();
			inIndent += inIsLast ? "    " : "│   ";
			var last = inNode.GetChildren().LastOrDefault();
			foreach (var child in inNode.GetChildren()) PrettifySyntaxTree(child, inStringBuilder, inIndent, child == last);
			return inStringBuilder.ToString();
		}
	}
}