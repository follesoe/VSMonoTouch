# MonoTouch for Visual Studio 2010

## About
This Visual Studio 2010 extension will enable you to open and compile MonoTouch projects without 
having to modify the .cproj file. The extension makes Visual Studio 2010 recognize the project 
types created by MonoTouch. It will also make sure any XIB files are ignored by Visual Studio 
when compiling the project.
 
This extension will only let you to open and compile MonoTouch projects. 
You will not be able to run your MonoTouch projects from Visual Studio, to do this you 
still need to use the official MonoTouch tools on a Mac.
 
The purpose of this project is to make cross-platform mobile development easier.
The extension will enable MonoTouch developers to use familiar tools such as ReSharper and 
Visual Studio when editing code. You can also use the [Project Linker Synchronization Tool](http://msdn.microsoft.com/en-us/library/ff921108(v=pandp.20.aspx)
to automatically keep Windows Phone, Mono for Android and MonoTouch class libraries in sync.

## Installation
1. [Download and run the vsix-package](https://github.com/downloads/follesoe/VSMonoTouch/VSMonoTouch%201.2.vsix)
from the github page.

2. Copy the MonoTouch binaries from your Mac development environment to your Visual Studio 2010 development environment.
Copy all the files from `/Developer/MonoTouch/usr/lib/mono/2.1/` on your Mac to 
`C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v1.0` on your PC.

3. Add a `RedistList`-folder under your newly created `v1.0`-folder. 
Download the [FrameworkList.xml file](https://github.com/downloads/follesoe/VSMonoTouch/FrameworkList.xml) and add it to the `RedistList`-folder. 

## Why the v1.0 folder?
The reason for the ".NETFramework\v1.0" location on your PC is how MonoTouch specify the 
`<TargetFrameworkVersion>v1.0</TargetFrameworkVersion>` in the .cproj file. 
If the `<TargetFrameworkIdentifier>` isn't specified Visual Studio will default to `.NETFramework`, 
which will make Visual Studio look for reference assemblies in the `v1.0` folder. 
If you don't copy your MonoTouch files to this directory Visual Studio will complain that the 
Target Framework is not installed.

## A note on mscorlib.dll
The MonoTouch templates does not include an explicity reference to mscorlib.dll. The default
behavior of Visual Studio 2010 is that all projects contain an implied reference to mscorlib.dll.
The problem is that the implied version of mscorlib.dll is different from the MonoTouch mscorlib.dll,
which will give you warnings in the editor. The project will compile just fine without the reference. 

The other problem is how Visual Studio 2010 will not let you add a new reference to mscorlib.dll as it 
allready have that reference implied. The way to work around this is to add it manually by editing the .csproj file.

    <ItemGroup>
      <Reference Include="mscorlib" />
      <Reference Include="monotouch" />
      <Reference Include="System" />
      <Reference Include="System.Xml" />
      <Reference Include="System.Core" />    
    </ItemGroup> 

Visual Studio will now pick up the MonoTouch version of mscorlib.dll, and the project will still build
just fine in MonoDevelop. The next update of the extension will enable you to automatically add this
reference if missing.

## Credit
The MonoTouchFlavorProjectFactory is based on [Jamie Briant's implementation](https://github.com/jamiebriant/VsMono).