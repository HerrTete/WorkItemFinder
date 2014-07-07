namespace WorkItemFinder
{
    public class SearchWorkItem
    {
        public SearchWorkItem(int id, string title, string description, string state, string project, string urlPattern, string tfsUrl)
        {
            Id = id;
            Title = title;
            Description = description;
            State = state;
            Project = project;
            Link = string.Format("{0}/{1}/{3}{2}", tfsUrl, Project, Id, urlPattern);
            SearchStringShort = string.Concat(id, title).ToLower();
            SearchStringLong = string.Concat(SearchStringShort, description).ToLower();
        }

        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string State { get; private set; }
        public string Project { get; private set; }
        public string Link { get; private set; }
        public string SearchStringShort { get; private set; }
        public string SearchStringLong { get; private set; }
    }
}
