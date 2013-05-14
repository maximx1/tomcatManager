using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;

namespace tomcatmanager
{
	public class FileManager
	{
		/// <summary>
		/// The Settings file.
		/// </summary>
		private const string SettingsFile = "tomcatman.config";

		/// <summary>
		/// The share directory.
		/// </summary>
		private string ShareDirectory;

		/// <summary>
		/// The tomcat webapps root.
		/// </summary>
		private string TomcatRoot;

		/// <summary>
		/// The system tmp dir.
		/// </summary>
		private string TmpDir;

		/// <summary>
		/// The directory to unwrap the war files.
		/// </summary>
		private string UnwrapDir;

		/// <summary>
		/// The war names.
		/// </summary>
		private List<String> _WarNames;

		/// <summary>
		/// Gets the shared drive war names.
		/// </summary>
		/// <value>
		/// The war names.
		/// </value>
		public List<String> WarNames
		{
			get
			{
				return _WarNames;
			}
		}

		/// <summary>
		/// The Deployed war project names.
		/// </summary>
		private List<String> _DeployedWarNames;

		/// <summary>
		/// Gets the deployed war project names.
		/// </summary>
		/// <value>
		/// The deployed war names.
		/// </value>
		public List<String> DeployedWarNames
		{
			get
			{
				return _DeployedWarNames;
			}
		}

		/// <summary>
		/// The project names found between the 2 folders.
		/// </summary>
		private List<String> IntersectingProjectNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="tomcatmanager.FileManager"/> class.
		/// </summary>
		public FileManager()
		{
			LoadSettings();

			_WarNames = new List<string>();

			LoadWarFiles();
			LoadDeployed();
			IntersectingProjectNames = _WarNames.Select(x => x.Substring(0, x.Length - 4)).Intersect(_DeployedWarNames).ToList();
		}

		/// <summary>
		/// Loads the war files from the shares directory.
		/// </summary>
		private void LoadWarFiles()
		{
			if(!Directory.Exists(ShareDirectory))
			{
				Console.WriteLine("Directory not found.");
				Environment.Exit(0x1);
			}

			_WarNames = Directory.GetFiles(ShareDirectory, "*.war").ToList();
			_WarNames = _WarNames.Select(x => x.Substring(ShareDirectory.Length)).ToList();
		}

		/// <summary>
		/// Loads the deployed sites from the tomcat root.
		/// </summary>
		private void LoadDeployed()
		{
			if(!Directory.Exists(TomcatRoot))
			{
				Console.WriteLine("Directory not found.\n Try running with the -c to create settings.");
				Environment.Exit(0x1);
			}

			_DeployedWarNames = Directory.GetDirectories(TomcatRoot).Where(x => !x.ToUpper().EndsWith("ROOT")).ToList();
			_DeployedWarNames = _DeployedWarNames.Select(x => x.Substring(TomcatRoot.Length)).ToList();
		}

		/// <summary>
		/// Loads the settings from the config file.
		/// </summary>
		private void LoadSettings()
		{
			if(!File.Exists(SettingsFile))
			{
				Console.WriteLine("Settings file hasn't been created.\n Try running with the -c to create settings.");
				Environment.Exit(1);
			}

			XElement settings = XElement.Load(SettingsFile);

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

		/// <summary>
		/// Configure this instance.
		/// </summary>
		public static void Configure()
		{
			String defaultInput = @"/usr/local/tomshare/";
			String input = "";
			Console.Write("Enter location of the share directory(Default:" + defaultInput + "): ");
			input = Console.ReadLine();
			if(input != "" && !input.EndsWith("/"))
				input += "/";
			XElement exelm = new XElement("settings", new XElement("shareDir", (input == "" ? defaultInput : input)));

			defaultInput = @"/var/lib/tomcat7/webapps/";
			input = "";
			Console.Write("Enter location of the Tomcat webapps(Default:" + defaultInput + "): ");
			input = Console.ReadLine();
			if(input != "" && !input.EndsWith("/"))
				input += "/";
			exelm.Add(new XElement("tomcatDir", (input == "" ? defaultInput : input)));

			defaultInput = @"/tmp/";
			input = "";
			Console.Write("Enter location tmp directory(Default:" + defaultInput + "): ");
			input = Console.ReadLine();
			if(input != "" && !input.EndsWith("/"))
				input += "/";
			exelm.Add(new XElement("tmpDir", (input == "" ? defaultInput : input)));

			exelm.Save(SettingsFile);
		}

		/// <summary>
		/// Deploys the war files.
		/// </summary>
		public void Deploy()
		{
			this.CleanUp();

			Directory.CreateDirectory(UnwrapDir);

			//Deploys the previously deployed files.
			foreach(string projectName in IntersectingProjectNames)
			{
				//If the previously deployed object is older then redeploy
				if(Directory.GetCreationTime(TomcatRoot + projectName)
					< Directory.GetCreationTime(ShareDirectory + projectName + ".war"))
				{
					Process job = new Process();
					job.StartInfo = new ProcessStartInfo("unzip", ShareDirectory + projectName + ".war -d " + UnwrapDir + projectName);
					job.Start();
					job.WaitForExit();

					//Swap 'em out
					Directory.Delete(TomcatRoot + projectName, true);
					Directory.Move(UnwrapDir + projectName, TomcatRoot + projectName);

					//Alert that things are deployed.
					Console.WriteLine("Deployed: " + projectName);
				}
			}

			//Deploys all the others
			foreach(string projectName in _WarNames
			        .Where(x => !IntersectingProjectNames
			        .Contains(x.Substring(0, x.Length - 4)))
			        .Select(x => x.Substring(0, x.Length - 4)))
			{
				Process job = new Process();
				job.StartInfo = new ProcessStartInfo("unzip", ShareDirectory + projectName + ".war -d " + UnwrapDir + projectName);
				job.Start();
				job.WaitForExit();

				//Swap 'em out
				Directory.Move(UnwrapDir + projectName, TomcatRoot + projectName);

				//Alert that things are deployed.
				Console.WriteLine("Deployed: " + projectName);
			}

			this.CleanUp();
		}

		/// <summary>
		/// Cleans up the used directories.
		/// </summary>
		public void CleanUp()
		{
			if(Directory.Exists(UnwrapDir))
			{
				Directory.Delete(UnwrapDir, true);
			}
		}
	}
}