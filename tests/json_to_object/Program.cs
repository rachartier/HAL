using System;
using System.ComponentModel;
using Newtonsoft.Json.Linq;

namespace json_to_object
{
    class Program
    {

        static void Main(string[] args)
        {
            string jsonSample = @"{
					'attribut1': 10,
					'attribut2': 1.0654646848464648468,
					'attribut3': 'coucou',
					'obj': {
						'x': 10,
						'y': -5,
						'z': 12,
						'name': 'cube'
					},
					'array': [
						'a',
						'b',
						'c',
						10
					],
					'date': '1998-06-27T00:00:00'
				}";

            var obj = JObject.Parse(jsonSample);

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                string name = descriptor.Name;
                object val = descriptor.GetValue(obj);

                if (val is JValue)
                {
                    string type = (val as JValue).Value.GetType().Name;
                    Console.WriteLine($"{name} ({type}) -> {val}");
                }
                else
                {
                    Console.WriteLine($"{name} ({val.GetType()}) -> {val}");
                }
            }
        }
    }
}
