namespace Kerballoons
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class KBFilter : BaseFilter
	{
		protected override string Manufacturer
		{
			get
			{
				return "KerBalloons";
			}
			set
			{
			}
		}

		protected override string categoryTitle
		{
			get
			{
				return "KerBalloons";
			}
			set
			{
			}
		}
	}
}
