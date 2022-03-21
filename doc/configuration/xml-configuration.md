# XML Configuration

If you are running the library outside of .NET Core, probably you have a `web.config` o `app.config` file. In this case here the code you have to add.

```markup
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="redisCacheClient" type="StackExchange.Redis.Extensions.LegacyConfiguration.RedisCachingSectionHandler, StackExchange.Redis.Extensions.LegacyConfiguration" />
	</configSections>

	<redisCacheClient allowAdmin="true" ssl="false" connectTimeout="3000" database="24">
		<serverEnumerationStrategy mode="Single" targetRole="PreferSlave" unreachableServerAction="IgnoreIfOtherAvailable" /> 
		<hosts>
			<add host="127.0.0.1" cachePort="6379" />
		</hosts>
	</redisCacheClient>

</configuration>
```

From now, in order to read the configuration is enough to run this:

```javascript
var redisConfiguration = RedisCachingSectionHandler.GetConfig();
```

