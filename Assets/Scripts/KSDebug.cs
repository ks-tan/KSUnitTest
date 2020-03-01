using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KSDebug
{
	public static void Log(string logMessage, string color = null)
	{
		if (!string.IsNullOrEmpty(color))
		{
			logMessage = "<color=" + color + ">" + logMessage + "</color>";
		}

		Debug.Log(logMessage);
	}
}