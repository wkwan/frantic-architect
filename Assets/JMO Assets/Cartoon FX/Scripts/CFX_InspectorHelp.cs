// Cartoon FX  - (c) 2015, Jean Moreno

// Help Component that can be added to any GameObject or Prefab
// Can be useful if you want to add comments to a particular prefab about
// its usage

using UnityEngine;
using System.Collections;

public class CFX_InspectorHelp : MonoBehaviour
{
	public bool Locked;
	public string Title;
	public string HelpText;
	public int MsgType;
	
	[ContextMenu("Unlock editing")]
	void Unlock()
	{
		this.Locked = false;
	}
}
