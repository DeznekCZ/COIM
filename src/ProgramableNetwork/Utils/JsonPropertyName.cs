using System;

namespace ProgramableNetwork.Utils;

public class JsonPropertyNameAttribute(string name) : Attribute {
	public string Name { get; } = name;
}
