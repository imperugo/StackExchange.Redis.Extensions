dotnet pack src\StackExchange.Redis.Extensions.Core\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.Binary\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.Jil\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.MsgPack\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.Newtonsoft\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.Protobuf\ -o ..\..\packages\ -c Release

dotnet pack src\StackExchange.Redis.Extensions.Utf8Json\ -o ..\..\packages\ -c Release

.\.nuget\NuGet.exe pack .\src\StackExchange.Redis.Extensions.LegacyConfiguration\StackExchange.Redis.Extensions.LegacyConfiguration.csproj -Prop Configuration=Release -OutputDirectory .\packages\