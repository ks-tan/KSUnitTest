using System.Collections.Generic;
using System.Linq;

namespace Cheep
{
	/// <summary>
	/// Declares the different type of syntax tokens you will find in a piece of text
	/// </summary>
	public enum SyntaxType
	{
		NumberToken, // 12345
		WhitespaceToken, //" "
		PlusToken, // +
		MinusToken, // -
		StarToken, // *
		SlashToken, // "/"
		CloseParenthesisToken, // ")"
		OpenParenthesisToken, // "("
		BadToken, // error/no such token type
		EOFToken, // end of file
		BinaryExpression, // <left expression><operator token><right expression>
		NumberExpression // <number token>
	}

	/// <summary>
	/// A token is a unit in an expression that defines its type and value.
	/// For example, "1" is a number of value 1, and "+" is a plus symbol with value null
	/// They are also Syntax Nodes, i.e. the leaves in our syntax tree used in parsing/evaluating the text
	/// </summary>
	public class SyntaxToken : SyntaxNode
	{
		public override SyntaxType Type { get; } // Token type
		public int Position { get; } // Its position on the expression
		public string Text { get; } // Its string representation on the expression
		public object Value { get; } // Its true value, whether it is a number, string, bool, or etc.

		public SyntaxToken(SyntaxType inType, int inPosition, string inText, object inValue)
		{
			Type = inType;
			Position = inPosition;
			Text = inText;
			Value = inValue;
		}

		/// <summary>
		/// Gets child syntax nodes in the parse tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
	}

	/// <summary>
	/// Analyse a piece of text and find its tokens, with type and value.
	/// This is done by going through each character in the text and decide how to group them according to their token type
	/// </summary>
	public class Lexer
	{
		private readonly string _text; // The text which we are analysing and extracting our tokens
		private int _position; // Current position of the lexer on the text
		private char _currentCharacter { get { return _position >= _text.Length ? '\0' : _text[_position]; } } // Gets the current character on the text, based on current _position

		/// <summary>
		/// Constructor takes in a string and sets _text private readonly property
		/// </summary>
		public Lexer(string inText) => _text = inText;

		/// <summary>
		/// Updates our current position on the text
		/// </summary>
		private void Next() => _position++;

		/// <summary>
		/// Attempts to get the next token on the text
		/// </summary>
		public SyntaxToken NextToken()
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
	/// Syntax nodes are tokens or expressions on a tree-structure (the parse tree) showing the order which they will be evaluated
	/// </summary>
	public abstract class SyntaxNode
	{
		/// <summary>
		/// The syntax type of this node, determining what type of token or expression it is
		/// </summary>
		public abstract SyntaxType Type { get; }

		/// <summary>
		/// Gets child syntax nodes in the parse tree
		/// </summary>
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

		/// <summary>
		/// Gets child syntax nodes in the parse tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return NumberToken;
		}
	}

	/// <summary>
	/// An expression that looks like <left-expression><operator-token><right-expression>
	/// </summary>
	sealed class BinaryExpressionSyntax : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.BinaryExpression;
		public ExpressionSyntax LeftExpression { get; }
		public SyntaxToken OperatorToken { get; }
		public ExpressionSyntax RightExpression { get; }

		public BinaryExpressionSyntax(ExpressionSyntax inLeftExpression, SyntaxToken inOperatorToken, ExpressionSyntax inRightExpression)
		{
			LeftExpression = inLeftExpression;
			OperatorToken = inOperatorToken;
			RightExpression = inRightExpression;
		}

		/// <summary>
		/// Gets child syntax nodes in the parse tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return LeftExpression;
			yield return OperatorToken;
			yield return RightExpression;
		}
	}

	/// <summary>
	/// Parses the tokens in a text (i.e. making sense of the series of tokens)
	/// This is done by arranging the tokens into a parse tree, and then iterating through the syntax nodes and evaluate them
	/// </summary>
	public class Parser
	{
		private readonly SyntaxToken[] _tokens;
		private int _position;

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

			}  while (token.Type != SyntaxType.EOFToken);

			_tokens = tokens.ToArray();
		}

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
		/// Get the next token in the tokens array if it's of a certain type. Else return null
		/// </summary>
		private SyntaxToken Match(SyntaxType inType)
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