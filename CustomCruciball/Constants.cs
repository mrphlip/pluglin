namespace CustomCruciball;

public class Constants {
	public const int NUM_LEVELS = 20;
	public const int NUM_COLS = 3;
	public const int NUM_ROWS = (NUM_LEVELS + NUM_COLS - 1) / NUM_COLS;

	public const float POPUP_XMIN = -147f;
	public const float POPUP_XMAX = 147f;
	public const float POPUP_YMIN = -75f;
	public const float POPUP_YMAX = 85f;
	public const float CELL_WIDTH = (POPUP_XMAX - POPUP_XMIN) / NUM_COLS;
	public const float CELL_HEIGHT = (POPUP_YMAX - POPUP_YMIN) / NUM_ROWS;

	// TODO: Proper string tables? Localisation???
	public static readonly string[] LABELS = new string[NUM_LEVELS]{
		"Add one Pebball\nPebballs have -0/-1",
		"Minibosses can\nappear in ?",
		"One less <sprite name=\"CRIT_PEG\">",
		"Misnavigation deals\nmore damage",
		"One less <sprite name=\"REFRESH_PEG\">",
		"Enemies have more HP",
		"Post-battle\nhealing reduced",
		"Receive less gold",
		"<sprite name=\"RIGGED_BOMB\"> hurts more",
		"Bosses have more HP",
		"Add Terriball",
		"Reduced <sprite name=\"BOMB\"> damage",
		"Heal less\nafter bosses",
		"Reduced Max HP",
		"Extra enemy turn\non reload",
		"Add Horriball",
		"Minibosses deal\nmore damage",
		"Shop is more\nexpensive",
		"Additional enemies",
		"Additional boss\nmechanics",
	};
}
