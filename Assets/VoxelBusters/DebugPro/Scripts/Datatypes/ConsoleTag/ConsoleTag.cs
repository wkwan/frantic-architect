using UnityEngine;
using System.Collections;

namespace VoxelBusters.DebugPRO.Internal
{
	[System.Serializable]
	public class ConsoleTag
	{
		#region Properties

		[SerializeField]
		private string 									m_name;
		public string 									Name
		{
			get
			{
				return m_name;
			}
			
			set
			{
				m_name	= value;
			}
		}
		
		[SerializeField]
		private bool									m_isActive;
		public bool 									IsActive
		{
			get
			{
				return m_isActive;
			}
			
			set
			{
				m_isActive	= value;
			}
		}
		
		[SerializeField]
		private bool									m_ignore;
		public bool 									Ignore
		{
			get
			{
				return m_ignore;
			}
			
			set
			{
				m_ignore	= value;
			}
		}

		#endregion

		#region Constructors

		private ConsoleTag ()
		{}

		public ConsoleTag (string _tagName, bool _isActive = true, bool _ignore = false)
		{
			Name		= _tagName;
			IsActive	= _isActive;
			Ignore		= _ignore;
		}

		#endregion
	}
}