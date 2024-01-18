using UnityEngine;
using UnityEngine.Animations;

namespace WeldToolMod
{
	public class Mod
	{
		public static void Main()
		{
			ModAPI.RegisterTool<WeldTool>(
				"Weld", 
				"Indestructible fixed joint", 
				ModAPI.LoadSprite("weldTool_view.png")
			);
		}
	}
}