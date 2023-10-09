using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Sirenix.OdinInspector;
using TPLib;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Framework.Database;

public abstract class Database<T> : TPSingleton<T>, IDatabase, ILegacyDeserializable where T : SerializedMonoBehaviour
{
	public class MissingAssetException : Exception
	{
		public MissingAssetException(string assetName)
			: base($"An asset is missing: {assetName}")
		{
		}
	}

	public struct TopologicSortedDefinition
	{
		public XElement Element;

		public string Id;

		public string TemplateId;

		public TopologicSortedDefinition(XElement element, string id, string templateId)
		{
			Element = element;
			Id = id;
			TemplateId = templateId;
		}

		public override string ToString()
		{
			return "[" + Element.Name.LocalName + " Id=\"" + Id + "\"" + (string.IsNullOrEmpty(TemplateId) ? "" : (" TemplateId =\"" + TemplateId + "\"")) + "]";
		}
	}

	protected static bool HasBeenDeserialized { get; private set; }

	public abstract void Deserialize(XContainer container = null);

	public bool GetHasBeenDeserialized()
	{
		return HasBeenDeserialized;
	}

	public void SetHasBeenDeserialized(bool hasBeenDeserialized)
	{
		HasBeenDeserialized = hasBeenDeserialized;
	}

	protected override void Awake()
	{
		base.Awake();
		if (base._IsValid && !HasBeenDeserialized)
		{
			Deserialize();
			SetHasBeenDeserialized(hasBeenDeserialized: true);
		}
	}

	protected Queue<XElement> GatherElements(IEnumerable<TextAsset> groupElements, IEnumerable<TextAsset> individualElements, string singleElementName, string customGroupElementName = null)
	{
		Queue<XElement> gatheredElements = new Queue<XElement>();
		if (groupElements != null)
		{
			string text = customGroupElementName ?? (singleElementName + "s");
			foreach (TextAsset groupElement in groupElements)
			{
				((XContainer)((XContainer)XDocument.Parse(groupElement.text, (LoadOptions)2)).Element(XName.op_Implicit(text))).Elements(XName.op_Implicit(singleElementName)).All(delegate(XElement o)
				{
					gatheredElements.Enqueue(o);
					return true;
				});
			}
		}
		if (individualElements != null)
		{
			foreach (TextAsset individualElement in individualElements)
			{
				XElement item = ((XContainer)XDocument.Parse(individualElement.text, (LoadOptions)2)).Element(XName.op_Implicit(singleElementName));
				gatheredElements.Enqueue(item);
			}
		}
		return gatheredElements;
	}

	protected IEnumerable<XElement> SortElementsByDependencies(IEnumerable<XElement> elements)
	{
		List<TopologicSortedDefinition> unsortedElements = new List<TopologicSortedDefinition>();
		foreach (XElement element in elements)
		{
			List<TopologicSortedDefinition> list = unsortedElements;
			string value = element.Attribute(XName.op_Implicit("Id")).Value;
			XAttribute obj = element.Attribute(XName.op_Implicit("TemplateId"));
			list.Add(new TopologicSortedDefinition(element, value, ((obj != null) ? obj.Value : null) ?? string.Empty));
		}
		return from o in TopologicSorter.Sort(unsortedElements, (TopologicSortedDefinition o) => unsortedElements.Where((TopologicSortedDefinition p) => p.Id == o.TemplateId))
			select o.Element;
	}
}
