
# HLTV-CLI

HLTV CLI client written in C#, built with .NET Core 3.1. I have no clue how to structure a C# project so files may be all over the place. It works mostly well on the Windows command line but on UNIX systems the colours are off and the scoreboard is broken.

## Prerequisites

Obviously .NET Core 3.1 is needed and all the libraries below to build the project.

## Libraries used

- [CloudflareSolverRe](https://github.com/RyuzakiH/CloudflareSolverRe) .NET lib for bypassing cf.
- [Colorful](http://colorfulconsole.com/) for RGB conversion into console
- [HtmlAgilityPack](https://html-agility-pack.net/)
- [Json.NET](https://www.newtonsoft.com/json)
- [SocketIOClient](https://github.com/doghappy/socket.io-client-csharp), a very friendly and capable C# socketio client library
