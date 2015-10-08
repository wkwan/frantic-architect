using UnityEngine;
using System.Collections;

namespace VoxelBusters.DebugPRO.Internal
{
	public enum eConsoleLogType
	{
		ERROR		= 1 << 0,
		ASSERT		= 1 << 1,
		WARNING		= 1 << 2,
		INFO		= 1 << 3,
		EXCEPTION	= 1 << 4
	}
}
