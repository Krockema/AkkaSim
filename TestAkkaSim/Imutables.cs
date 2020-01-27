using System;
using System.Collections.Generic;
using System.Linq;
using AkkaSim;
using ImmutableObjectLib;
using Xunit;

namespace TestAkkaSim
{
    public class Imutables
    {
        [Fact]
        public void CopyImutableByFSharp()
        {
            var listof = new List<ImutableMessage>
            {
                MessageFactory.Create("t1", "s1", "abuse me")
            };

            var msg = listof.First();
            msg = msg.UpdatePriority(ImmutableObjectLib.Priority.High);

            Assert.Equal(expected: listof.First().Key, actual: msg.Key);
            Assert.NotEqual(expected: listof.First().Priority, actual: msg.Priority);
        }


        [Fact]
        public void TestMutableListInFSharp()
        {
            var listof = new List<ImutableMessage>
            {
                MessageFactory.Create("t1", "s1", "abuse me")
            };

            var beforeAdd = listof.First().List.Count;
            listof.First().List.Add(new Item(Guid.NewGuid(), "Blabla"));
            
            Assert.True(beforeAdd == 0);
            Assert.True(listof.First().List.Count == 1);
        }
    }
}
