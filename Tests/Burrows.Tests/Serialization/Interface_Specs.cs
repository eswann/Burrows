// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Burrows.Tests.Serialization
{
    using System;
    using Burrows.Pipeline;
    using Burrows.Pipeline.Configuration;
    using Burrows.Serialization;
    using NUnit.Framework;
    using TestConsumers;

    public abstract class Deserializing_an_interface<TSerializer> :
		SerializationSpecificationBase<TSerializer> where TSerializer : IMessageSerializer, new()
	{
		[Test]
		public void Should_create_a_proxy_for_the_interface()
		{
			var user = new User("Chris", "noone@nowhere.com");
			IComplaintAdded complaint = new ComplaintAdded(user, "No toilet paper", BusinessArea.Appearance)
				{
					Body = "There was no toilet paper in the stall, forcing me to use my treasured issue of .NET Developer magazine."
				};

			TestSerialization(complaint);
		}

		[Test]
		public void Should_dispatch_an_interface_via_the_pipeline()
		{
			var pipeline = InboundPipelineConfigurator.CreateDefault(null);

			var consumer = new TestMessageConsumer<IComplaintAdded>();

			pipeline.ConnectInstance(consumer);

			var user = new User("Chris", "noone@nowhere.com");
			IComplaintAdded complaint = new ComplaintAdded(user, "No toilet paper", BusinessArea.Appearance)
				{
					Body = "There was no toilet paper in the stall, forcing me to use my treasured issue of .NET Developer magazine."
				};

			pipeline.Dispatch(complaint);

			consumer.ShouldHaveReceivedMessage(complaint);
		}
	}


	[TestFixture]
	public class WhenUsingJson :
		Deserializing_an_interface<JsonMessageSerializer>
	{
		
	}

	public interface IComplaintAdded
	{
		IUser AddedBy { get; }

		DateTime AddedAt { get; }

		string Subject { get; }

		string Body { get; }

		BusinessArea Area { get; }
	}

	public enum BusinessArea
	{
		Unknown = 0,
		Appearance,
		Courtesy,
	}

	public interface IUser
	{
		string Name { get; }
		string Email { get; }
	}

	public class User : IUser
	{
		public User(string name, string email)
		{
			Name = name;
			Email = email;
		}

		protected User()
		{
		}

		public string Name { get; set; }

		public string Email { get; set; }

		public bool Equals(IUser other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Name, Name) && Equals(other.Email, Email);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if(!typeof(IUser).IsAssignableFrom(obj.GetType())) return false;
			return Equals((IUser) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Email != null ? Email.GetHashCode() : 0);
			}
		}
	}

	public class ComplaintAdded :
		IComplaintAdded
	{
		public ComplaintAdded(IUser addedBy, string subject, BusinessArea area)
		{
			DateTime dateTime = DateTime.UtcNow;
			AddedAt = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second,
				dateTime.Millisecond, DateTimeKind.Utc);

			AddedBy = addedBy;
			Subject = subject;
			Area = area;
			Body = string.Empty;
		}

		protected ComplaintAdded()
		{
		}

		public IUser AddedBy { get; set; }

		public DateTime AddedAt { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }

		public BusinessArea Area { get; set; }

		public bool Equals(IComplaintAdded other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return AddedBy.Equals(other.AddedBy) && other.AddedAt.Equals(AddedAt) && Equals(other.Subject, Subject) && Equals(other.Body, Body) && Equals(other.Area, Area);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (!typeof(IComplaintAdded).IsAssignableFrom(obj.GetType())) return false;
			return Equals((IComplaintAdded)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (AddedBy != null ? AddedBy.GetHashCode() : 0);
				result = (result*397) ^ AddedAt.GetHashCode();
				result = (result*397) ^ (Subject != null ? Subject.GetHashCode() : 0);
				result = (result*397) ^ (Body != null ? Body.GetHashCode() : 0);
				result = (result*397) ^ Area.GetHashCode();
				return result;
			}
		}
	}
}