using System;

namespace Cityline
{
    public static class ProducerExtensions 
    {
        public static string Name(this ICitylineProducer producer) 
        {
            var name = producer.GetType().Name.Replace("Producer", "");
            return Char.ToLowerInvariant(name[0]) + name.Substring(1);
        }
    }
}