using System;

namespace CMC.Domain.Entities
{
	public class Industry
	{
		public Guid Id { get; private set; }
		public string Name { get; private set; } = string.Empty;

		private Industry() { }

		public Industry(string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
			Id = Guid.NewGuid();
			Name = name.Trim();
		}

        public void Rename(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            Name = name.Trim();
        }
	}
}
