using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KSCheep.CodeAnalysis
{
	/// <summary>
	/// Analyse a piece of text and find its tokens, with type and value.
	/// This is done by going through each character in the text and decide how to group them according to their token type
	/// </summary>
	public sealed class Lexer
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
			if (_position >= _text.Length) return new SyntaxToken(SyntaxType.EndOfFileToken, _position, "\0", null);

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
}