using System.Collections.Generic;
using System.Xml.Serialization;

namespace TheLastStand.Serialization;

public class SerializedCity : ISerializedData
{
	[XmlAttribute]
	public string Id;

	public int MaxApoPassed;

	public int MaxNightReached;

	public int NumberOfRuns;

	public int NumberOfWins;

	public bool CustomModeEnabled;

	public List<string> SelectedGlyphs;
}
