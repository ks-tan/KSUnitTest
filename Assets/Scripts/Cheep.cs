using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		NumberExpression, // <number token>
		ParenthesisedExpression // ( <expression> )
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
		/// Gets child syntax nodes in the syntax tree
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
		private List<string> _diagnostics = new List<string>(); // For keeping track fo errors and reporting them
		private char _currentCharacter { get { return _position >= _text.Length ? '\0' : _text[_position]; } } // Gets the current character on the text, based on current _position

		/// <summary>
		/// Constructor takes in a string and sets _text private readonly property
		/// </summary>
		public Lexer(string inText) => _text = inText;

		/// <summary>
		/// Returns a collection of diagnostics messages collected so far during lexing of tokens
		/// </summary>
		public IEnumerable<string> Diagnostics => _diagnostics;

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
				if (!int.TryParse(text, out var value)) // we also try to get the value of the token
					_diagnostics.Add("ERROR: The number " + _text + " is not a valid int32."); // report an error if number is not a valid int32
				return new SyntaxToken(SyntaxType.NumberToken, start, text, value); // we construct the new token and return it!
			}

			// Attempting to get whitespace token
			if (char.IsWhiteSpace(_currentCharacter))
			{
				var start = _position; // get current start position
				while (char.IsWhiteSpace(_currentCharacter)) Next(); // keep going to the next character if it is a whitespace
				var length = _position - start; // after we get all the whitespaces, we find the length of this whitespace token..
				var text = _text.Substring(start, length); // .. so we can get the substring of this whitespace token
				return new SyntaxToken(SyntaxType.WhitespaceToken, start, text, null); // we construct a new whitespace token and return it!
			}

			// Attempting to get random symbol tokens
			if (_currentCharacter == '+') return new SyntaxToken(SyntaxType.PlusToken, _position++, "+", null);
			if (_currentCharacter == '-') return new SyntaxToken(SyntaxType.MinusToken, _position++, "-", null);
			if (_currentCharacter == '*') return new SyntaxToken(SyntaxType.StarToken, _position++, "*", null);
			if (_currentCharacter == '/') return new SyntaxToken(SyntaxType.SlashToken, _position++, "/", null);
			if (_currentCharacter == '(') return new SyntaxToken(SyntaxType.OpenParenthesisToken, _position++, "(", null);
			if (_currentCharacter == ')') return new SyntaxToken(SyntaxType.CloseParenthesisToken, _position++, ")", null);

			// If nothing found, return a bad token and add an error log into the diagnostics list
			_diagnostics.Add("ERROR: Bad character input: " + _currentCharacter);
			return new SyntaxToken(SyntaxType.BadToken, _position++, _text.Substring(_position - 1, 1), null);
		}
	}

	/// <summary>
	/// Base class for syntax nodes of different types.
	/// Syntax nodes are tokens or expressions on a tree-structure (the syntax tree) showing the order which they will be evaluated
	/// </summary>
	public abstract class SyntaxNode
	{
		/// <summary>
		/// The syntax type of this node, determining what type of token or expression it is
		/// </summary>
		public abstract SyntaxType Type { get; }

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
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
		/// Gets child syntax nodes in the syntax tree
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
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return LeftExpression;
			yield return OperatorToken;
			yield return RightExpression;
		}
	}

	/// <summary>
	/// An expression that looks like ( <expression> )
	/// </summary>
	sealed class ParenthesisedExpression : ExpressionSyntax
	{
		public override SyntaxType Type => SyntaxType.ParenthesisedExpression;
		public SyntaxToken OpenParenthesisToken { get; }
		public ExpressionSyntax Expression { get; }
		public SyntaxToken CloseParenthesisToken { get; }

		public ParenthesisedExpression(SyntaxToken inOpenParenthesisToken, ExpressionSyntax inExpression, SyntaxToken inCloseParenthesisToken)
		{
			OpenParenthesisToken = inOpenParenthesisToken;
			Expression = inExpression;
			CloseParenthesisToken = inCloseParenthesisToken;
		}

		/// <summary>
		/// Gets child syntax nodes in the syntax tree
		/// </summary>
		public override IEnumerable<SyntaxNode> GetChildren()
		{
			yield return OpenParenthesisToken;
			yield return Expression;
			yield return CloseParenthesisToken;
		}
	}

	/// <summary>
	/// Identifies the starting syntax node (an expression syntax) and the ending node (an EOF Syntax Token) of the syntax tree
	/// It also acts as the final collection point of all the diagnostic logs from the lexing and parsing of tokens and syntax trees
	/// </summary>
	public sealed class SyntaxTree
	{
		public IReadOnlyList<string> Diagnostics { get; }
		public ExpressionSyntax Root { get; }
		public SyntaxToken EOFToken { get; }

		public SyntaxTree(IEnumerable<string> inDiagnostics, ExpressionSyntax inRoot, SyntaxToken inEOFToken)
		{
			Diagnostics = inDiagnostics.ToArray();
			Root = inRoot;
			EOFToken = inEOFToken;
		}

		/// <summary>
		/// Parse a syntax tree from a piece of text
		/// </summary>
		public static SyntaxTree Parse(string text) => new Parser(text).Parse();
	}

	/// <summary>
	/// Parses the tokens in a text (i.e. making sense of the series of tokens)
	/// This is done by arranging the tokens into a syntax tree, and then iterating through the syntax nodes and evaluate them
	/// </summary>
	public class Parser
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

			}  while (token.Type != SyntaxType.EOFToken);

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
		private SyntaxToken Match(SyntaxType inType)
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
			var expression = ParseTerm();
			var eofToken = Match(SyntaxType.EOFToken);
			return new SyntaxTree(_diagnostics, expression, eofToken);
		}

		/// <summary>
		/// A helper method to parse an expression
		/// </summary>
		private ExpressionSyntax ParseExpression() => ParseTerm();

		/// <summary>
		/// Parsing an expression that uses +- operators
		/// </summary>
		public ExpressionSyntax ParseTerm()
		{
			// We parse the left factor expression. We build the syntax tree with factor expressions first because +- operations always come last
			var left = ParseFactor();

			while (CurrentToken.Type == SyntaxType.PlusToken || CurrentToken.Type == SyntaxType.MinusToken)
			{
				var operatorToken = NextToken(); // Get the operator token and move to the pointer to the next one
				var right = ParseFactor(); // With the pointer at the right token, we parse the right factor expression
				left = new BinaryExpressionSyntax(left, operatorToken, right); // Collapse everything as one binary expression
			}

			return left;
		}

		/// <summary>
		/// Parsing expression that uses */ operators
		/// </summary>
		public ExpressionSyntax ParseFactor()
		{
			// Similar to how ParseTerm() works, but this time for times and divide

			var left = ParsePrimaryExpression();

			while (CurrentToken.Type == SyntaxType.StarToken || CurrentToken.Type == SyntaxType.SlashToken)
			{
				var operatorToken = NextToken();
				var right = ParsePrimaryExpression();
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
				var closeParenthesisToken = Match(SyntaxType.CloseParenthesisToken);
				return new ParenthesisedExpression(openParenthesisToken, expression, closeParenthesisToken);
			}

			var numberToken = Match(SyntaxType.NumberToken);
			return new NumberExpressionSyntax(numberToken);
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

	/// <summary>
	/// Evaluates an expression syntax
	/// </summary>
	public class Evaluator
	{
		private readonly ExpressionSyntax _root;

		/// <summary>
		/// To construct the evaluator, we pass in the root syntax node of the syntax tree we wish to evaluate
		/// </summary>
		public Evaluator(ExpressionSyntax inRoot) => _root = inRoot;

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		/// <returns></returns>
		public int Evaluate() => EvaluateExpression(_root);

		/// <summary>
		/// Evaluate an expression starting from the root node of its syntax tree
		/// </summary>
		private int EvaluateExpression(ExpressionSyntax inNode)
		{
			// Evaluate a number expression. Just returns the number token
			if (inNode is NumberExpressionSyntax number)
			{
				return (int)number.NumberToken.Value;
			}

			// Evaluate the binary expression. Gets the left and right expression, identify the operator token between them, and perform operation
			if (inNode is BinaryExpressionSyntax binary)
			{
				var leftExpression = EvaluateExpression(binary.LeftExpression);
				var rightExpression = EvaluateExpression(binary.RightExpression);
				if (binary.OperatorToken.Type == SyntaxType.PlusToken) return leftExpression + rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.MinusToken) return leftExpression - rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.StarToken) return leftExpression * rightExpression;
				else if (binary.OperatorToken.Type == SyntaxType.SlashToken) return leftExpression / rightExpression;
				else throw new Exception("Unexpected binary operator " + binary.OperatorToken.Type);
			}

			// Evaluate a parenthesised expression. We only evaluate the "expression" within the parenthesis
			if (inNode is ParenthesisedExpression parenthesisedExpression)
			{
				return EvaluateExpression(parenthesisedExpression.Expression);
			}

			throw new Exception("Unexpected node " + inNode.Type);
		}
	}
}