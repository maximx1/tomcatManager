using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace tomcatmanager
{
	public class FileManager
	{
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
			ShareDirectory = @"/usr/local/tomshare/";
			TomcatRoot = @"/var/lib/tomcat7/webapps/";
			TmpDir = @"/tmp/";
			UnwrapDir = TmpDir + @"War_Is_Unfolding/";

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
				Console.WriteLine("Directory not found.");
				Environment.Exit(0x1);
			}

			_DeployedWarNames = Directory.GetDirectories(TomcatRoot).Where(x => !x.ToUpper().EndsWith("ROOT")).ToList();
			_DeployedWarNames = _DeployedWarNames.Select(x => x.Substring(TomcatRoot.Length)).ToList();
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