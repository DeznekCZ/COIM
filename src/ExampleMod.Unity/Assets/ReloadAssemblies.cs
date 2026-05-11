using UnityEngine;

public class ReloadAssemblies
{
	// Add menu item to reload assemblies
	[UnityEditor.MenuItem("Tools/Reload Assemblies %#r")]
	private static void Reload() {
		UnityEditor.EditorApplication.ExecuteMenuItem("Assets/Refresh");
	}
}
