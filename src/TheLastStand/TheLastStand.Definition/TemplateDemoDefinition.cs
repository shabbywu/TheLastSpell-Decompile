using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class TemplateDemoDefinition : Definition, ITopologicSortItem<TemplateDemoDefinition>
{
	private XElement demoElement;

	public string Id { get; private set; }

	public string TemplateId { get; private set; }

	public float TestFloat { get; private set; }

	public int TestInt { get; private set; }

	public string TestString { get; private set; }

	public TemplateDemoDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		DeserializeBeforeOrdering(container);
	}

	public void DeserializeBeforeOrdering(XContainer container)
	{
		demoElement = (XElement)(object)((container is XElement) ? container : null);
		Id = demoElement.Attribute(XName.op_Implicit("Id")).Value;
		XAttribute obj = demoElement.Attribute(XName.op_Implicit("TemplateId"));
		TemplateId = ((obj != null) ? obj.Value : null) ?? string.Empty;
	}

	public void DeserializeAfterTemplatesOrdering()
	{
		TemplateDemoDefinition templateDemoDefinition = ((!string.IsNullOrEmpty(TemplateId)) ? TemplateDemoDatabase.DemoDefinitions[TemplateId] : null);
		XElement val = ((XContainer)demoElement).Element(XName.op_Implicit("TestInt"));
		TestInt = ((val != null) ? int.Parse(val.Value) : templateDemoDefinition.TestInt);
		XElement obj = ((XContainer)demoElement).Element(XName.op_Implicit("TestString"));
		TestString = ((obj != null) ? obj.Value : null) ?? templateDemoDefinition.TestString;
		XElement obj2 = ((XContainer)demoElement).Element(XName.op_Implicit("TestSequence"));
		XElement val2 = ((obj2 != null) ? ((XContainer)obj2).Element(XName.op_Implicit("TestFloat")) : null);
		TestFloat = ((val2 != null) ? float.Parse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture) : templateDemoDefinition.TestFloat);
	}

	public IEnumerable<TemplateDemoDefinition> GetDependencies()
	{
		return TemplateDemoDatabase.DemoDefinitions.Values.Where((TemplateDemoDefinition o) => o.Id == TemplateId);
	}

	public override string ToString()
	{
		return $"{typeof(TemplateDemoDefinition).Name} Id {Id} : [{TestInt},{TestString},{TestFloat}]";
	}
}
