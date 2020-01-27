using System;
using System.Collections.Generic;
using ImmutableObjectLib;

namespace TestAkkaSim
{
    public class MessageFactory
    {
        public static ImutableMessage Create(string target, string source, object obj)
        {
            return new ImutableMessage(key: Guid.NewGuid(),
                target: target,
                @object: obj,
                list: new List<Item>(),
                priority: Priority.Medium);
        }
        public static ImutableMessage Create(string target, string source, object obj, long due)
        {
            return new ImutableMessage(key: Guid.NewGuid(),
                target: target,
                @object: obj,
                list: new List<Item>(),
                priority: Priority.Medium);
        }
    }
}