Tomcat Manager
=============

### Note
This was a bastard child hack for my school server..

### Description
Tomcat manager software controls opening war files and deploying them to the tomcat root.

### Author
[Justin Walrath](mailto:walrathjaw@gmail.com)

### Software to be installed prior:
1. Apache2/Tomcat7 (obviously)
2. Unzip
3. mono (c#/.net framework for linux)
4. mono-mcs
5. make (if you want to use the Makefile)

### Install
1. Set up the required software ^
2. If compiling from source there are 2 paths:
	a. (Using make) open the TomcatCLC (Tomcat Command Line Compile) directory and type "make".
      	b. Run the makefile commands manually. (See inner workings at the bottom)
3. Install the the tomcatmanager.exe somewhere out of reach from regular users (I used "/root/")
4. Configure the C# program
      	a. As of version 1.1 use the "-c" flag to specify the directories used.
5. Set up a crontab job as root.
	a. Using "sudo crontab -e" or being root and using "crontab -e"
	b. Add this line to the crontab file. -> "* * * * * /<locationOfExecutable>/tomcatmanager.exe"
		* Note this will run it every minute. You can change the timings. Google crontab

### Use
   Users will just pass their .war file into the the shared directory.
   The crontab schedule will then update it with tomcat everytime the update job fires.
   No further administration is needed.

### Limitations
   1. You should have users create unique names for their war files so as to avoid collision with other projects.
   2. This only adds new .war files or replaces previously deployed projects.
      It will not delete projects that are present in the tomcat webapps directory that aren't present in the share.

### The inner workings
"make" just runs the following commands:
1. `mcs *.cs -out:tomcatmanager.exe /reference:System.Xml.Linq`
2. `chmod 764 tomcatmanager.exe`

### changes
##### Version 1.0
As of version 1.0 the default install directories are as listed below. In subsequent versions will be updating it to use a config file

<pre>
   Tomcat:
	   webapps dir: "/var/lib/tomcat7/webapps/"
      This is where the c# program is currently pointed to.

	Share Folder:
	   location:    "/usr/local/tomshare/"
      permissions: "777" So all users can deploy to it.
		   *Might change later to handle groups.
</pre>

##### Version 1.1

<pre>
   Updated program to use a config file.
	To configure the application with custom directories,
	use the "-c" flag.
</pre>
