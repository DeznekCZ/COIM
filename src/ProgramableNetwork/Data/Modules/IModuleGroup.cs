using Mafi.Core.Mods;

namespace ProgramableNetwork.Data.Modules;

public interface IModuleGroup : IModData { }

public abstract class ModuleGroup : IModData {

	protected static readonly string[] NAMES = [
		"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q"
	];

	public abstract void RegisterData(ProtoRegistrator registrator);
}