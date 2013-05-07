using System;
using System.IO;

namespace tomcatmanager
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			FileManager man = new FileManager();
			man.Deploy();
		}
	}
}
