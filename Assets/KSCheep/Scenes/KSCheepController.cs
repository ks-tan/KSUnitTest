using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KSCheepController : MonoBehaviour
{
	[SerializeField] private Text _inputTextUI = null; // the input text field
	[SerializeField] private Text _inputCursorUI = null; // the text field that holds the cursor

	private string _inputText = ""; // the entire input field's text
	private int _currentCursorIndex = 0; // the index on the input string our cursor should be at

	private const char BACKSPACE = '\b'; // backspace character
	private const char RETURN = '\r'; // return character

	void OnGUI()
	{
		Event e = Event.current;

		// if user presses a key
		if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
		{
			// split current line to left and right tokens, based on _currentCursorIndex
			string left = _inputText.Substring(0, _currentCursorIndex);
			string right = _inputText.Substring(Mathf.Min(_currentCursorIndex, _inputText.Length), Mathf.Max(0, _inputText.Length - _currentCursorIndex));

			// right arrow
			if (e.keyCode == KeyCode.RightArrow && _currentCursorIndex < _inputText.Length)
			{
				_currentCursorIndex++;
			}
			// left arrow
			else if (e.keyCode == KeyCode.LeftArrow && _currentCursorIndex > 0)
			{
				_currentCursorIndex--;
			}
			else if (e.keyCode == KeyCode.UpArrow)
			{
				// find last new line from current position
				// find offset of current position from last new line
				// find the last, last new line
				// if no last, last new line, break..
				// .. else, apply that calculated offset on last, last newline
			}
			else if (e.keyCode == KeyCode.DownArrow)
			{
				// find last new line from current position
				// find offset of current position from last new line
				// find the next new line
				// if no next new line, break..
				// .. else, apply that calculated offset on next newline
			}
			// backspace
			else if (e.keyCode == KeyCode.Backspace)
			{
				if (_currentCursorIndex > 0)
				{
					left = left.Remove(left.Length - 1, 1);
					_inputText = left + right;
					_currentCursorIndex--;
				}
			}
			// return/enter/newline
			else if (e.keyCode == KeyCode.Return)
			{
				_inputText = left + '\n' + right;
				_currentCursorIndex++;
			}
			// other characters and digits
			else if (Input.inputString.Length >= 0)
			{
				_inputText = left + Input.inputString + right;
				_currentCursorIndex += Input.inputString.Length;
			}

			_inputTextUI.text = _inputText;

			string cursorText = _inputText.Substring(0, Mathf.Max(0, _currentCursorIndex));
			_inputCursorUI.text = cursorText + '_';
		}
	}
}
