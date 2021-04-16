dotnet pack src\core\StackExchange.Redis.Extensions.Core\StackExchange.Redis.Extensions.Core.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.Jil\StackExchange.Redis.Extensions.Jil.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.MsgPack\StackExchange.Redis.Extensions.MsgPack.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.Newtonsoft\StackExchange.Redis.Extensions.Newtonsoft.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.Protobuf\StackExchange.Redis.Extensions.Protobuf.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.Utf8Json\StackExchange.Redis.Extensions.Utf8Json.csproj -o .\packages\ -c Release
dotnet pack src\serializers\StackExchange.Redis.Extensions.System.Text.Json\StackExchange.Redis.Extensions.System.Text.Json.csproj -o .\packages\ -c Release
dotnet pack src\aspnet\StackExchange.Redis.Extensions.AspNetCore\StackExchange.Redis.Extensions.AspNetCore.csproj -o .\packages\ -c Release