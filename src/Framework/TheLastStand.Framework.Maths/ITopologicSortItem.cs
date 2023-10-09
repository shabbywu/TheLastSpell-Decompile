using System.Collections.Generic;

namespace TheLastStand.Framework.Maths;

public interface ITopologicSortItem<T>
{
	IEnumerable<T> GetDependencies();
}
