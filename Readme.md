# MonoTouch for Visual Studio 2010 #
This Visual Studio 2010 extension enables you to open and compile MonoTouch projects without 
having to modify the .cproj file. The extension makes Visual Studio 2010 recognize the project 
types created by MonoTouch. It will also make sure any XIB files are ignored by Visual Studio 
when compiling the project.
 
This extension will only let you to open and compile MonoTouch projects. 
You will not be able to run your MonoTouch projects from Visual Studio, to do this you 
still need to use the official MonoTouch tools on a Mac.
 
The purpose of this project is to make cross-platform mobile development smoother now 
that you can open MonoTouch, Mono for Android and Windows Phone 7 projects in the same 
Visual Studio 2010 solution. The extension will also enable MonoTouch developers to use 
familiar tools such as ReSharper and Visual Studio when editing code.

## Installation ##
To install the extension simply [download and run the vsix-package](https://github.com/downloads/follesoe/VSMonoTouch/VSMonoTouch.vsix)
from the download  section on the github page. In addition you need to copy the MonoTouch 
binaries from your Mac development environment to your Visual Studio 2010 development environment.
Copy all the files from "/Developer/MonoTouch/usr/lib/mono/2.1/" on your Mac to 
"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v1.0" on your PC.

The reason for the ".NETFramework\v1.0" location on your PC is how MonoTouch specify the 
"<TargetFrameworkVersion>v1.0</TargetFrameworkVersion>" in the .cproj file. 
If the "TargetFrameworkIdentifier" isn’t specified Visual Studio will default to ".NETFramework", 
which will make Visual Studio look for reference assemblies in the "v1.0" folder. 
If you don’t copy your MonoTouch files to this directory Visual Studio will complain that the 
Target Framework is not installed.

The final step is to add a "RedistList" folder under your newly created "v1.0"-folder. 
Inside this folder you need to download and add a ["FrameworkList.xml" file](https://github.com/downloads/follesoe/VSMonoTouch/FrameworkList.xml). 