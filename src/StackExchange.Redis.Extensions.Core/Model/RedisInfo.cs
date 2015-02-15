using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis.Extensions.Core.Model
{
	public class RedisInfo
	{
		/// <summary>
		/// Version of the Redis serve
		/// </summary>
		public string Version { get; private set; }

		/// <summary>
		/// Git dirty flag
		/// </summary>
		public string GitSha1 { get; private set; }

		/// <summary>
		/// Git dirty flag
		/// </summary>
		public string GitDirty { get; private set; }

		/// <summary>
		/// Operating system hosting the Redis server
		/// </summary>
		public string Os { get; private set; }

		public static RedisInfo ParseRedisInformation(string info)
		{
			RedisInfo response = new RedisInfo();

			string[] lines = info.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			var data = new Dictionary<string, string>();
			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				
				if (string.IsNullOrEmpty(line) || line[0] == '#')
				{
					// 2.6+ can have empty lines, and comment lines
					continue; 
				}

				int idx = line.IndexOf(':');
				if (idx > 0) // double check this line looks about right
				{
					var key = line.Substring(0, idx);
					var infoValue = line.Substring(idx + 1);

					data.Add(key, infoValue);
				}
			}

			return response;
		}
	}
}
