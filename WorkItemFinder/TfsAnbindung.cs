using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace WorkItemFinder
{
    public class TfsAnbindung
    {
        private static TfsAnbindungConfig _config = new TfsAnbindungConfig();

        public static void GetItemsAsync(Action<SearchWorkItem> appendAction, Action cachingFinished)
        {
            Task.Factory.StartNew(
                () =>
                {
                    GetItems(appendAction);
                    if (cachingFinished != null)
                    {
                        cachingFinished();
                    }
                });
        }

        public static void GetItems(Action<SearchWorkItem> appendAction)
        {
            _config.LoadConfig();
            var stopWatch = Stopwatch.StartNew();
            var tpc = new TfsTeamProjectCollection(new Uri(_config.TfsUrl));
            var workItemStore = (WorkItemStore)tpc.GetService(typeof(WorkItemStore));
            var result = workItemStore.Query(_config.QueryString);
            //var result = workItemStore.Query("Select * From WorkItems where [Created Date] > @Today-356");

            foreach (WorkItem workitem in result)
            {
                /*foreach (Field field in workitem.Fields)
                {
                    Trace.WriteLine(field.Name);
                }*/
                var id = workitem.Id;
                var title = workitem.Fields[_config.TitleField].Value.ToString();
                var description = workitem.Fields[_config.DescriptionField].Value.ToString();
                var state = workitem.Fields[_config.StateField].Value.ToString();
                var project = workitem.Fields[_config.TeamProjectField].Value.ToString();
                var newSearchWorkItem = new SearchWorkItem(id, title, description, state, project, _config.UrlPattern, _config.TfsUrl);
                appendAction(newSearchWorkItem);
            }
            stopWatch.Stop();
            Trace.WriteLine("LadeZeit: " + stopWatch.Elapsed.TotalMilliseconds);
        }
    }

    internal class TfsAnbindungConfig
    {
        private bool configIsLoaded = false;

        internal void LoadConfig()
        {
            if (!configIsLoaded)
            {
                var configFullPath =
                    Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "WorkItemFinder",
                        "WorkItemFinderTfsConfig.json");
                if (File.Exists(configFullPath))
                {
                    var serializer = new JavaScriptSerializer();
                    var configString = File.ReadAllText(configFullPath);
                    var config = serializer.Deserialize<TfsAnbindungConfig>(configString);
                    
                    TfsUrl = config.TfsUrl;
                    QueryString = config.QueryString;
                    TitleField = config.TitleField;
                    DescriptionField = config.DescriptionField;
                    StateField = config.StateField;
                    TeamProjectField = config.TeamProjectField;
                    UrlPattern = config.UrlPattern;
                }
                else
                {
                    var defaultConfig = new TfsAnbindungConfig
                    {
                        QueryString = "Select [State], [Title], [Id], [Description], [Team Project] From WorkItems where [Created Date] > @Today-365 Order By [Created Date] Desc",
                        TitleField = "Title",
                        DescriptionField = "Description",
                        StateField = "State",
                        TeamProjectField = "Team Project",
                        UrlPattern = "_workitems#_a=edit&id=",
                        TfsUrl = "http://yourTfsUrl:8080/tfs/defaultcollection"
                    };
                    var serializer = new JavaScriptSerializer();
                    var configString = serializer.Serialize(defaultConfig);
                    var configPath = Path.GetDirectoryName(configFullPath);
                    if (!Directory.Exists(configPath))
                    {
                        Directory.CreateDirectory(configPath);
                    }
                    File.WriteAllText(configFullPath, configString);

                    throw new Exception("Please setup your config at " + configFullPath);
                }

                configIsLoaded = true;
            }
        }

        public string TfsUrl { get; set; }
        public string QueryString { get; set; }
        public string TitleField { get; set; }
        public string DescriptionField { get; set; }
        public string StateField { get; set; }
        public string TeamProjectField { get; set; }
        public string UrlPattern { get; set; }
    }
}