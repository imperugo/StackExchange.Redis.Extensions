dotnet pack .\src\StackExchange.Redis.Extensions.Core\project.json -c Release -o .\nuget
dotnet pack .\src\Serializers\StackExchange.Redis.Extensions.Jil\project.json -c Release -o .\nuget 
dotnet pack .\src\Serializers\StackExchange.Redis.Extensions.Newtonsoft\project.json -c Release -o .\nuget 
dotnet pack .\src\Serializers\StackExchange.Redis.Extensions.MsgPack\project.json -c Release -o .\nuget 
dotnet pack .\src\Serializers\StackExchange.Redis.Extensions.Protobuf\project.json -c Release -o .\nuget 