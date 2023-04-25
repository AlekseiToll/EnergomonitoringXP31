// (c) Mars-Energo Ltd.
// author : Andrew A. Golyakov 
// 
// Enumerations using in the project

namespace EmDataSaver.SavingInterface
{
	/// <summary>Types of folders in treelike structure</summary>
	public enum FolderType : short
	{
		/// <summary>The folder is empty</summary>
		Empty = 0,
		/// <summary>Folder contents other folders (subfolders)</summary>
		HasSubfolders = 1,
		/// <summary>Folder contents database archives</summary>
		HasDatabases = 2,
	}

	///// <summary>Types of measured data</summary>
	//public enum MeasureType
	//{
	//    /// <summary>PQP measures</summary>
	//    PQP = 0,
	//    /// <summary>Average measures</summary>
	//    AVG = 1,
	//    /// <summary>Dips and overvoltages measures</summary>
	//    DNS = 2,
	//}

	///// <summary>Types of submeasures</summary>
	//public enum SubMeasureType
	//{
	//    /// <summary>Main parameters</summary>
	//    AVGMain = 0,
	//    /// <summary>Harmonics</summary>
	//    AVGHarmonics = 1,
	//    /// <summary>Harmonics Angles </summary>
	//    AVGAngles = 2
	//}
}
