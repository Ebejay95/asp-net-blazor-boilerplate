using System.ComponentModel.DataAnnotations;

namespace CMC.Contracts.Tags
{
	public record CreateTagRequest(
		[property: Required, StringLength(200, MinimumLength = 1), Display(Name = "Tag")]
		string Name
	);
}
