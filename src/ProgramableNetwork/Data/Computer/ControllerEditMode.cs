namespace ProgramableNetwork
{
	public enum ControllerEditMode
	{
		// Only the field floater is reachable; no add, no move.
		Edit,
		// Click a placed module to set it as the "last created" template;
		// '+' slots open the picker / shift-add the last created module.
		Add,
		// Click a placed module to pick it up, then click an empty slot to drop it there.
		Move,
	}
}
