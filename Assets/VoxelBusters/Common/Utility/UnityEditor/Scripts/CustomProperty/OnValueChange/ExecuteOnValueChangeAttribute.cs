using UnityEngine;
using System.Collections;

namespace VoxelBusters.Utility
{
	public class ExecuteOnValueChangeAttribute : PropertyAttribute 
	{
		#region Properties
		
		public		string			CallbackMethod 
		{ 
			get; 
			private set; 
		}
		
		#endregion

		#region Constructor

		private ExecuteOnValueChangeAttribute ()
		{}

		public ExecuteOnValueChangeAttribute (string _method)
		{
			CallbackMethod	= _method;
		}

		#endregion
	}
}
