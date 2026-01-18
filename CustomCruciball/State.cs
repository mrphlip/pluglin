namespace CustomCruciball;

public class State {
	public static readonly State inst = new State();

	public bool isCustom = false;
	public bool[] levels = new bool[20]{false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false};
}
