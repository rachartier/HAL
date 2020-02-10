using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HAL.Connection.Client
{
    public class FuncManager
    {
        private readonly IDictionary<string, Func<Task>> functions = new Dictionary<string, Func<Task>>();

        public bool AddFunc(string name, Func<Task> Func)
        {
            return functions.TryAdd(name, Func);
        }

        public Func<Task> GetFunc(string name)
        {
            if (functions.TryGetValue(name, out var func)) return func;

            return null;
        }
    }
}