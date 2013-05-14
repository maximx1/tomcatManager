namespace tomcatmanager
{
	class MainClass
	{
		public static void Main(string[] args)
		{

			if(args.Length > 0 && args[0] == "-c")
			{
				FileManager.Configure();
			}
			else
			{
				FileManager man = new FileManager();
				man.Deploy();
			}
		}
	}
}
