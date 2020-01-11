# Gamemaster emulator
This is a bundled version of the Visual Basic .NET based components of the gamemaster server emulator.

## Compiling the code

**If you do not want to compile the source files yourself, ready built binaries are available in the "Releases" section.**

- Download the latest source files (or git clone this repository)
- Open gamemaster.sln in Visual Studio (2017 or newer)
- Select the "Release" profile
- Run "Build" -> "Build Solution"
- After compilation, all files are automatically copied to the "build" folder in the solution directory.

## Running the code

Once you aquired a working build, everything you need should be contained in the "build" folder.
Going forward this folder will be referred to as "./".

### Prerequisites
The following software is required to successfully run this project
- .NET core 2.1\<
- MySQL database server

### Setting up the MySQL database
- create a new (empty) MySQL database
- import "./gamemaster.sql"

### Configuring the servers
- run all 3 start_\<service\>.bat files in the build folder
- stop the servers again - a "./cfg" folder containing 3 XML files should have been generated
- open "./cfg/gamemaster.natneg.xml" and "./cfg/gamemaster.serverlist.xml" and adjust the MySQL configuration for your database server
```xml
  <MySQLHostname>localhost</MySQLHostname>
  <MySQLPort>3306</MySQLPort>
  <MySQLDatabase>gamemaster</MySQLDatabase>
  <MySQLUsername>root</MySQLUsername>
  <MySQLPwd></MySQLPwd>
```
- save both files

### Starting the servers
After configuration, the servers can be run by starting the matching "./run_gamemaster.\<service\>.bat" scripts.

### Linux
For use on linux systems, change to the build directory. Afterwards the services can be run using the following command line:
```
dotnet "bin/gamemaster.serverlist.dll"
```
```
dotnet "bin/gamemaster.natneg.dll"
```
```
dotnet "bin/gamemaster.keyauth.dll"
```
