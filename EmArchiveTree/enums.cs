
namespace EmArchiveTree
{
	/// <summary>
	/// Types of the ArvhiveTreeView nodes
	/// </summary>
	public enum EmTreeNodeType
	{
		/// <summary>PostgreSQL server Node</summary>
		PgServer = 0,
		/// <summary>Folder Node</summary>
		Folder = 1,
		/// <summary>Object Node</summary>
		Object = 2,
		/// <summary>Measure Group Node: AVG, PQP, DNS</summary>
		MeasureGroup = 3,
		/// <summary>Measure Node: archive</summary>
		Measure = 4,
		/// <summary>EM32 Node</summary>
		EM32Device = 5,
		/// <summary>Folder Node for Device (root)</summary>
		DeviceFolder = 6,
		/// <summary>Folder Node for year</summary>
		YearFolder = 7,
		/// <summary>Folder Node for month</summary>
		MonthFolder = 8,
		/// <summary>AVG Group Node: 3 sec, 1 min, 30 min</summary>
		AvgGroup = 9,
		/// <summary>ETPQP Node</summary>
		ETPQPDevice = 10,
		/// <summary>Folder in Device</summary>
		FolderInDevice = 11,
		/// <summary>ETPQP-A Node</summary>
		ETPQP_A_Device = 12,
		/// <summary>ETPQP-A Registration</summary>
		Registration = 13
	}

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

	/// <summary>Types of measured data</summary>
	public enum MeasureType
	{
		/// <summary>PQP measures</summary>
		PQP = 0,
		/// <summary>Average measures</summary>
		AVG = 1,
		/// <summary>Dips and overvoltages measures</summary>
		DNS = 2,
	}

	/// <summary>Types of submeasures</summary>
	public enum SubMeasureType
	{
		/// <summary>Main parameters</summary>
		AVGMain = 0,
		/// <summary>Harmonics</summary>
		AVGHarmonics = 1,
		/// <summary>Harmonics Angles </summary>
		AVGAngles = 2
	}
}