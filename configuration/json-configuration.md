# Json Configuration

```javascript
{
	"Redis": {
		"Password": "my_super_secret_password",
		"AllowAdmin": true,
		"Ssl": false,
		"ConnectTimeout": 6000,
		"ConnectRetry": 2,
		"Database": 0,
		"Hosts": [
		{
			"Host": "192.168.0.10",
			"Port": "6379"
		},
		{
			"Host": "192.168.0.11",
			"Port": "6381"
		}]
	},
	"MaxValueLength" = 1024,
	"PoolSize" = 5,
	"KeyPrefix" = "_my_key_prefix_",
}
```



