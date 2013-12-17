namespace Burrows.Tests.Messages
{
    using System;

    [Serializable]
	public class ClientMessage
	{
		private string _name;

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}
	}
}