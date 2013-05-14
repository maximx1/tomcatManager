using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace TestingFields
{
	class MainClass
	{
		private const string SettingsFile = "tomcatman.config";
		private static string ShareDirectory;
		private static string TomcatRoot;
		private static string TmpDir;
		private static string UnwrapDir;

		public static void Main(string[] args)
		{
			LoadSettings();
		}

		/// <summary>
		/// Loads the settings from the config file.
		/// </summary>
		private static void LoadSettings()
		{
			if(!File.Exists(SettingsFile))
			{
				Console.WriteLine("Settings file hasn't been created.\n Try running with the -c to create settings.");
				Environment.Exit(1);
			}

			XElement settings = XElement.Load(SettingsFile);
			Console.WriteLine(settings);

			var serverProperties = settings.Descendants()
                           .Where(s => s.HasElements == false)
                           .ToDictionary(s => s.Name.ToString(), s => s.Value.ToString());

			//Check for the share directory.
			if(!serverProperties.TryGetValue("shareDir", out ShareDirectory))
			{
				Console.WriteLine("There was an issue with the file. \n Try running with the -c to re-create settings.");
				Environment.Exit(1);
			}
			if(ShareDirectory != "" && !ShareDirectory.EndsWith("/"))
			ShareDirectory += "/";

			//Check for the webapps Directory.
			if(!serverProperties.TryGetValue("tomcatDir", out TomcatRoot))
			{
				Console.WriteLine("There was an issue with the file. \n Try running with the -c to re-create settings.");
				Environment.Exit(1);
			}
			if(TomcatRoot != "" && !TomcatRoot.EndsWith("/"))
			TomcatRoot += "/";

			//Check for the tmp Directory.
			if(!serverProperties.TryGetValue("tmpDir", out TmpDir))
			{
				Console.WriteLine("There was an issue with the file. \n Try running with the -c to re-create settings.");
				Environment.Exit(1);
			}
			if(TmpDir != "" && !TmpDir.EndsWith("/"))
			TmpDir += "/";

			//Set up the Temporary unfolding directory.
			UnwrapDir = TmpDir + @"War_Is_Unfolding/";

			Console.WriteLine(ShareDirectory);
			Console.WriteLine(TomcatRoot);
			Console.WriteLine(TmpDir);
			Console.WriteLine(UnwrapDir);
		}
	}
}
