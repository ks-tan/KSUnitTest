using KSCheep.CodeAnalysis;
using KSCheep.CodeAnalysis.Syntax;
using System.Collections;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class CheepTerminal : MonoBehaviour
{
	[SerializeField] private TMP_InputField _inputField = null;
	[SerializeField] private TMP_InputField _consoleField = null;

	private StringBuilder _stringBuilder = new StringBuilder();
	private bool _shouldShowTree = false;

	IEnumerator InvokeOnNextFrame(System.Action onComplete)
	{
		yield return new WaitForEndOfFrame();
		onComplete();
	}

	void OnGUI()
	{
		Event e = Event.current;

		if (e.type == EventType.KeyDown)
		{
			if (e.keyCode == KeyCode.Return)
			{
				string[] inputLines = _inputField.text.Split('\n');
				_stringBuilder.Clear();

				if (inputLines[inputLines.Length - 1].ToLower() == "clear")
				{
					StartCoroutine(InvokeOnNextFrame(() => { _inputField.text = ""; _consoleField.text = ""; }));
				}
				else if (inputLines[inputLines.Length - 1].ToLower() == "showtree")
				{
					_shouldShowTree = !_shouldShowTree;
				}
				else
				{
					foreach (var input in inputLines)
					{
						var syntaxTree = SyntaxTree.Parse(input);

						if (syntaxTree.Diagnostics.Any())
						{
							foreach (var diagnostic in syntaxTree.Diagnostics)
							{
								_stringBuilder.AppendLine(diagnostic);
							}
						}
						else
						{
							var evaluator = new Evaluator(syntaxTree.Root);
							var result = evaluator.Evaluate();
							_stringBuilder.AppendLine(result.ToString());

							if (_shouldShowTree)
							{
								_stringBuilder.AppendLine(Parser.PrettifySyntaxTree(syntaxTree.Root));
							}
						}
					}

					_consoleField.text = _stringBuilder.ToString();
				}
			}
		}
	}
}
