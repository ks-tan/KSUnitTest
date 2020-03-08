using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Parses the tokens in a text (i.e. making sense of the series of tokens)
	/// This is done by arranging the tokens into a syntax tree, and then iterating through the syntax nodes and evaluate them
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
		/// </summary>
		private ExpressionSyntax ParseExpression(int inParentPrecedence = 0)
		{
			var left = ParsePrimaryExpression();

			while (true)
			{
				var precedence = CurrentToken.Type.GetBinaryOperatorPrecedence();
				if (precedence == 0 || precedence < inParentPrecedence) break;
				var operatorToken = NextToken();
				var right = ParseExpression(precedence);
				left = new BinaryExpressionSyntax(left, operatorToken, right);
			}

			return left;
		}

		/// <summary>
		/// A primary expression is an atomic unit that only contains one token. Here, we simply return that token
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