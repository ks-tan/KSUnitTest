using TMPro;
using UnityEngine;

public class KSCheepController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _inputTextUGUI = null;
	[SerializeField] private TextMeshProUGUI _consoleTextUGUI = null;

	private string _inputText = "";
	private int _currentInputPosition = 0;

	private const char BACKSPACE = '\b';
	private const char RETURN = '\r';

	void OnGUI()
	{
		Event e = Event.current;

		if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
		{
			string left = _inputText.Substring(0, _currentInputPosition);
			string right = _inputText.Substring(Mathf.Min(_currentInputPosition, _inputText.Length), Mathf.Max(0, _inputText.Length - _currentInputPosition));

			if (e.keyCode == KeyCode.RightArrow && _currentInputPosition < _inputText.Length)
			{
				_currentInputPosition++;
			}
			else if (e.keyCode == KeyCode.LeftArrow && _currentInputPosition > 0)
			{
				_currentInputPosition--;
			}
			else if (e.keyCode == KeyCode.Backspace)
			{
				if (_currentInputPosition > 0)
				{
					left = left.Remove(left.Length - 1, 1);
					_inputText = left + right;
					_currentInputPosition--;
				}
			}
			else if (e.keyCode == KeyCode.Return)
			{
				_inputText = left + '\n' + right;
				_currentInputPosition++;
			}
			else if (Input.inputString.Length >= 0)
			{
				_inputText = left + Input.inputString + right;
				_currentInputPosition += Input.inputString.Length;
			}

			_inputTextUGUI.SetText(_inputText);
		}
	}
}
