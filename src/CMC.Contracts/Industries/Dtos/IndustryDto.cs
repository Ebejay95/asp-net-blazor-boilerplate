using System;
using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Industries
{
	public class IndustryDto
	{
		[ScaffoldColumn(false)]
		public Guid Id { get; set; }

		[Required]
		[StringLength(200, MinimumLength = 1)]
		[Display(Name = "Branche")]
		public string Name { get; set; } = string.Empty;
	}
}
