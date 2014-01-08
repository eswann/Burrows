// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using Burrows.NewIds.NewIdProviders;

namespace Burrows.Tests.NewId_
{
    using System;
    using System.Diagnostics;
    using NUnit.Framework;

    [TestFixture]
    public class Using_the_newid_generator
    {
        [Test, Explicit]
        public void Should_be_able_to_extract_timestamp()
        {
            DateTime now = DateTime.UtcNow;
            NewIds.NewId id = NewIds.NewId.Next();

            DateTime timestamp = id.Timestamp;

            Console.WriteLine("Now: {0}, Timestamp: {1}", now, timestamp);

            TimeSpan difference = (timestamp - now);
            if (difference < TimeSpan.Zero)
                difference = difference.Negate();

            Assert.LessOrEqual(difference, TimeSpan.FromMinutes(1));
        }

        [Test]
        public void Should_generate_sequential_ids_quickly()
        {
            NewIds.NewId.SetTickProvider(new StopwatchTickProvider());
            NewIds.NewId.Next();

            int limit = 10;

            var ids = new NewIds.NewId[limit];
            for (int i = 0; i < limit; i++)
                ids[i] = NewIds.NewId.Next();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                Console.WriteLine(ids[i]);
            }
        }

        [Test, Explicit]
        public void Should_generate_unique_identifiers_with_each_invocation()
        {
            NewIds.NewId.Next();

            Stopwatch timer = Stopwatch.StartNew();

            int limit = 1024 * 1024;

            var ids = new NewIds.NewId[limit];
            for (int i = 0; i < limit; i++)
                ids[i] = NewIds.NewId.Next();

            timer.Stop();

            for (int i = 0; i < limit - 1; i++)
            {
                Assert.AreNotEqual(ids[i], ids[i + 1]);
                string end = ids[i].ToString().Substring(32, 4);
                if (end == "0000")
                    Console.WriteLine("{0}", ids[i].ToString());
            }

            Console.WriteLine("Generated {0} ids in {1}ms ({2}/ms)", limit, timer.ElapsedMilliseconds,
                limit / timer.ElapsedMilliseconds);
        }
    }
}