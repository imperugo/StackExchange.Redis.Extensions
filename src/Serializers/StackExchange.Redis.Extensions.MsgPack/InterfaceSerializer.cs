using System.Collections.Generic;
using System.Linq;
using MsgPack;
using MsgPack.Serialization;
using System;

namespace StackExchange.Redis.Extensions.MsgPack
{
	public class InterfaceSerializer<T> : MessagePackSerializer<T>
	{
		private readonly Dictionary<string, MessagePackSerializer> serializers;

		public InterfaceSerializer()
			: this(SerializationContext.Default)
		{
		}

		public InterfaceSerializer(SerializationContext context)
			: base(context)
		{
			serializers = new Dictionary<string, MessagePackSerializer>();

            // Get all types that implement T interface
            var implementingTypes = System.Reflection.Assembly
                .GetEntryAssembly()
                .DefinedTypes
                .Where(t => t.ImplementedInterfaces.Contains(typeof(T)))
                .Select(x => x.AsType());

			// Create serializer for each type and store it in dictionary
			foreach (var type in implementingTypes)
			{
				var key = type.Name;
				var value = Get(type, context);
				serializers.Add(key, value);
			}
		}

		protected override void PackToCore(Packer packer, T objectTree)
		{
            MessagePackSerializer serializer;
			string typeName = objectTree.GetType().Name;

			// Find matching serializer
			if (!serializers.TryGetValue(typeName, out serializer))
			{
				throw SerializationExceptions.NewTypeCannotSerialize(typeof(T));
			}

			packer.PackArrayHeader(2);             // Two-element array:
			packer.PackString(typeName);           //  0: Type name
			serializer.PackTo(packer, objectTree); //  1: Packed object
		}

		protected override T UnpackFromCore(Unpacker unpacker)
		{
            MessagePackSerializer serializer;
			string typeName;

			// Read type name and packed object
			if (!(unpacker.ReadString(out typeName) && unpacker.Read()))
			{
                // the previous exception was for internal use and now is obsolete meaning it will be removed soon and will break our code
				throw new Exception();
			}

			// Find matching serializer
			if (!serializers.TryGetValue(typeName, out serializer))
			{
				throw SerializationExceptions.NewTypeCannotDeserialize(typeof(T));
			}

			// Unpack and return
			return (T)serializer.UnpackFrom(unpacker);
		}
	}
}