using System.Collections.Generic;
using System.Linq;

namespace Passingwind.LibraryGallery.Domains
{
    public class Library
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public LibraryType Type { get; set; }

        public string Link { get; set; }

        public string UserId { get; set; }

        public List<LibraryTags> Tags { get; set; }

        public Library()
        {
            Tags = new List<LibraryTags>();
        }

        public void AddTags(string tag)
        {
            if (!Tags.Any(x => x.Name == tag))
            {
                Tags.Add(new LibraryTags() { Name = tag });
            }
        }

        public void ClearTags()
        {
            Tags.Clear();
        }
    }
}
