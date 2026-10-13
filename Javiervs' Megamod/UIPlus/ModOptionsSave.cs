using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using Debug = ModUtils.Debug;

namespace UIPlus
{
	public class ModOptionsSave : ModUtils.ModOptionsSave
	{
		private UIPlusOptionsSave uiplusOptions = new UIPlusOptionsSave();
		private static readonly UIPlusOptionsSave fallbackOptions = new UIPlusOptionsSave();

		public override ModUtils.IOptionsSave OptionsSave
		{
			get { return uiplusOptions; }
			set { uiplusOptions = (UIPlusOptionsSave)value; }
		}

		public static string id
		{
			get
			{
				return typeof(ModOptionsSave).Assembly.GetName().Name;
			}
		}

		protected override string FileName
		{
			get { return "/UIPlusOptionsSave.xml"; }
		}

		public static UIPlusOptionsSave Options
		{
			get
			{
				try
				{
					var entry = ModUtils.ModOptionsSaveBank.gBank?.Get(id);
					if (entry != null && entry.OptionsSave is UIPlusOptionsSave opts)
					{
						return opts;
					}
				}
				catch { }
				return fallbackOptions;
			}
		}
	}
}
