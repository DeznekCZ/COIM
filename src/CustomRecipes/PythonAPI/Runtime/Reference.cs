using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonAPI.Runtime;

public class Reference<T>
{
	public T Value
	{
		get => getter();
		set => setter(value);
	}

	private readonly Action<T> setter;
	private readonly Func<T> getter;

	public Reference(Action<T> setter, Func<T> getter)
	{
		this.setter = setter;
		this.getter = getter;
	}
}
