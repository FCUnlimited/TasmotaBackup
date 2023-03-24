# Tasmota Backup Tool
This tool helps you to make a backup of all Tasmota devices connected to your wifi.
For this the basic steps are:
1. Scan for tasmota divices
2. Save the devices found
3. Download the config files of the devices
4. Save them local

# How to use
There are two possibilities how to get this running.
You can download the exe file and just run it or you can use dotnet-script.

# Use dotnet-script
This variant has the benefit that you can edit the code according to your needs.
It's not much code so should be easy to understand. To get started follow this steps:
1. Install dotnet script: https://github.com/dotnet-script/dotnet-script#windows
2. clone the project
3. open the project folder with vscode
4. open main.csx press F5

# Why another one
I tried to find a simple backup solution. For this I found the following tools.
- https://github.com/sigmdel/tasmotasbacker
  - could not find how to install and I dont know pascal but looks good
- https://github.com/tasmota/decode-config
  - as far as I understand this tool is only for decoding the config files
- https://github.com/TasmoAdmin/TasmoAdmin
  - Is only for administration of Tasmotas. Has no backup function and runs on a server.

# Short code description
- main.csx
  - Entry point of the tool
- tasmota.csx
  - Class which saves the device informations
- tasmohttp.csx
  - handles the communication with the devices
- state.csx
  - saves the informations for next program start
- command.csx
  - class to build the menu
- mqttscan.csx
  - handles the mqtt communication which is not used at the moment.

