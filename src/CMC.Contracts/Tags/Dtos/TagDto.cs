using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Tags
{
	public class TagDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Required, StringLength(200, MinimumLength = 1)]
		[Display(Name = "Tag")]
		public string Name { get; set; } = string.Empty;
	}
}
