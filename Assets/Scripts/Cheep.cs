using System.Collections.Generic;
using System.Linq;

namespace Cheep
{
	/// <summary>
	/// Declares the different type of syntax tokens you will find in an expression
	/// </summary>
	public enum SyntaxType
	{
		NumberToken,
		WhitespaceToken,
		PlusToken,
		MinusToken,
		StarToken,
		SlashToken,
		CloseParenthesisToken,
		OpenParenthesisToken,
		BadToken,
		EOFToken,
		BinaryExpression,
		NumberExpression
	}

	/// <summary>
	/// A token is a unity in an expression that defines its type and value.
	/// For example, "1" is a number of value 1, and "+" is a plus symbol with value null
	/// They are also Syntax Nodes, i.e. the leaves in our syntax tree used in parsing/evaluating the expression
	/// </summary>
	public class SyntaxToken : SyntaxNode
	{
		public override SyntaxType Type { get; }
		public int Position { get; }
		public string Text { get; }
		public object Value { get; }

		public SyntaxToken(SyntaxType inType, int inPosition, string inText, object inValue)
		{
			Type = inType;
			Position = inPosition;
			Text = inText;
			Value = inValue;
		}

		public override IEnumerable<SyntaxNode> GetChildren()
		{
			return Enumerable.Empty<SyntaxNode>();
		}
	}

	/// <summary>
	/// Analyse an expression and find its tokens, with type and value
	/// </summary>
	public class Lexer
	{
		private readonly string _text; // The text which we are analysing and extracting our tokens
		private int _position; // Current position of the lexer on the text
		private char _currentCharacter { get { return _position >= _text.Length ? '\0' : _text[_position]; } } // Gets the current character on the text, based on current _position
		public Lexer(string inText) => _text = inText; // Constructor takes in a string and sets _text private readonly property
		private void Next() => _position++; // This method updates our current position on the text
		
		public SyntaxToken NextToken() // Attempts to get the next token on the text
		{
			// Returns an end of file token when there are not more characters to read (end of the text string)
			if (_position >= _text.Length) return new SyntaxToken(SyntaxType.EOFToken, _position, "\0", null);

			// Attempting to get a number token
			if (char.IsDigit(_currentCharacter))
			{
				var start = _position; // get current start position
				while (char.IsDigit(_currentCharacter)) Next(); // keep going to the next character if it is a digit
				var length = _position - start; // after we get all the digits, we find the length of this token..
				var text = _text.Substring(start, length); // ..so we can get the text value of this token using substring
				int.TryParse(text, out var value); // we also try to get the value of the token
				return new SyntaxToken(SyntaxType.NumberToken, start, text, value); // we construct the new token and return it!
			}

			// Attempting to get whitespace token
			if (char.IsWhiteSpace(_currentCharacter))
			{
				var start = _position;
				while (char.IsWhiteSpace(_currentCharacter)) Next();
				var length = _position - start;
				var text = _text.Substring(start, length);
				return new SyntaxToken(SyntaxType.WhitespaceToken, start, text, null);
			}

			// Attempting to get random symbol tokens
			if (_currentCharacter == '+') return new SyntaxToken(SyntaxType.PlusToken, _position++, "+", null);
			if (_currentCharacter == '-') return new SyntaxToken(SyntaxType.MinusToken, _position++, "-", null);
			if (_currentCharacter == '*') return new SyntaxToken(SyntaxType.StarToken, _position++, "*", null);
			if (_currentCharacter == '/') return new SyntaxToken(SyntaxType.SlashToken, _position++, "/", null);
			if (_currentCharacter == '(') return new SyntaxToken(SyntaxType.OpenParenthesisToken, _position++, "(", null);
			if (_currentCharacter == ')') return new SyntaxToken(SyntaxType.CloseParenthesisToken, _position++, ")", null);

			// If nothing found, return a bad token
			return new SyntaxToken(SyntaxType.BadToken, _position++, _text.Substring(_position - 1, 1), null);
		}
	}

	/// <summary>
	/// Base class for syntax nodes of different types.
	/// Syntax nodes are items on a tree-structure showing the order which tokens on an expression will be evaluated. See example below:
	///	    +
	///    / \
	///   +   3
	///  / \
	/// 1   2
	/// </summary>
	public abstract class SyntaxNode
	{
		public abstract SyntaxType Type { get; }
		public abstract IEnumerable<SyntaxNode> GetChildren();
	}

	/// <summary>
	/// An abstract class for an expression, which itself is also a node in the expression tree
	/// </summary>
	public abstract class ExpressionSyntax : SyntaxNode {}

	/// <summary>
	/// An expression that holds a number token
	/// </summary>
	sealed class NumberExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.NumberExpression;
		public SyntaxToken NumberToken { get; }
		public NumberExpressionSyntax(SyntaxToken inNumberToken) => NumberToken = inNumberToken;

		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return NumberToken;
		}
	}

	/// <summary>
	/// An expression that looks like <left><operator><right>
	/// </summary>
	sealed class BinaryExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.BinaryExpression;
		public ExpressionSyntax Left { get; }
		public SyntaxToken OperatorToken { get; }
		public ExpressionSyntax Right { get; }

		public BinaryExpressionSyntax(ExpressionSyntax inLeft, SyntaxToken inOperatorToken, ExpressionSyntax inRight)
		{
			Left = inLeft;
			OperatorToken = inOperatorToken;
			Right = inRight;
		}

		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return Left;
			yield return OperatorToken;
			yield return Right;
		}
	}

	/// <summary>
	/// Parses the tokens in an expression (i.e. making sense of the series of tokens)
	/// </summary>
	public class Parser
	{
		private readonly SyntaxToken[] _tokens;
		private int _position;

		public Parser(string inText) // During construction, populate the _tokens array 
		{
			var lexer = new Lexer(inText);
			var tokens = new List<SyntaxToken>();
			SyntaxToken token;

			do
			{
				token = lexer.NextToken();
				if (token.Type != SyntaxType.WhitespaceToken && token.Type != SyntaxType.BadToken) tokens.Add(token);
			}
			while (token.Type != SyntaxType.EOFToken);

			_tokens = tokens.ToArray();
		}

		private SyntaxToken Peek(int inOffset) // Peeking ahead, relative to current _position 
		{
			var index = _position + inOffset;
			if (index >= _tokens.Length) return _tokens[_tokens.Length - 1];
			return _tokens[index];
		}

		private SyntaxToken CurrentToken => Peek(0); // Getting the current token

		private SyntaxToken NextToken() // Getting the next token in the expression
		{
			var current = CurrentToken;
			_position++;
			return current;
		}

		private SyntaxToken Match(SyntaxType inType) // Get the next token if it's of a certain type. Else return null
		{
			return CurrentToken.Type == inType ? NextToken() : new SyntaxToken(inType, CurrentToken.Position, null, null);
		}

		public ExpressionSyntax Parse()
		{
			var left = ParsePrimaryExpression();

			while (CurrentToken.Type == SyntaxType.PlusToken || CurrentToken.Type == SyntaxType.MinusToken)
			{
				var operatorToken = NextToken();
				var right = ParsePrimaryExpression();
				left = new BinaryExpressionSyntax(left, operatorToken, right);
			}

			return left;
		}

		private ExpressionSyntax ParsePrimaryExpression()
		{
			var numberToken = Match(SyntaxType.NumberToken);
			return new NumberExpressionSyntax(numberToken);
		}
	}
}