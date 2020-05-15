using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dung.Ninja;
using Dung.Ninja.Objects;
using Serilog;
using YamlDotNet.RepresentationModel;

namespace Dung.Lib
{
    public abstract class Project : IDependency
    {
        protected Project(string rootDir, string sourceDir, string buildDir)
        {
            BuildDir = buildDir;
            Log.Information("Project dir: {@string}", rootDir);
            Log.Information("Source dir : {@string}", sourceDir);
            Log.Information("Build dir  : {@string}", buildDir);
            if (!Directory.Exists(BuildDir)) Directory.CreateDirectory(BuildDir);
            string projectFile = Path.Join(rootDir, "project.yml");
            Variables = new Dictionary<string, string>();
            Name = Path.GetFileName(rootDir);

            if (!File.Exists(projectFile)) return;
            Log.Information($"Found configuration file at {projectFile}");
            using var reader = File.OpenText(projectFile);
            var yaml = new YamlStream();
            yaml.Load(reader);
            Configuration = yaml.Documents[0];

            if (!(Configuration.RootNode is YamlMappingNode node)) return;
            var nodeName = new YamlScalarNode("name");
            if (!node.Children.ContainsKey(nodeName)) return;
            Name = node.Children[nodeName].ToString();
        }

        public Dictionary<string, string> Variables { get; set; }

        protected YamlDocument? Configuration { get; }
        protected abstract IDependency Entrypoint { get; }
        public string BuildDir { get; }

        public string Name { get; }


        public IEnumerable<IDependency>? Dependencies => new[]
        {
            Entrypoint
        };

        public IEnumerable<IDependency> FlattenDependencies()
        {
            return Dependencies.Concat(Dependencies.SelectMany(d => d.FlattenDependencies()));
        }

        public void WriteNinja()
        {
            var ninjaFile = Path.Join(BuildDir, "build.ninja");
            using var stream = File.Create(ninjaFile);
            using var streamWriter = new StreamWriter(stream) {AutoFlush = true};
            WriteNinja(streamWriter);
            Log.Information("Ninja file written at {@string}", ninjaFile);
        }

        public void WriteNinja(StreamWriter writer)
        {
            List<IBuildable> buildables = FlattenDependencies().OfType<IBuildable>().ToList();

            var ninja = new NinjaSyntax(writer);
            ninja.Comment(
                "This file was generated by the dung build system. Please do not edit, as changes will be automatically overriden.");

            ninja.Newline();
            ninja.Comment("Global variables");
            foreach ((string key, string value) in Variables) ninja.Variable(key, value);

            ninja.Newline();
            ninja.Comment("Build rules");
            foreach (Rule rule in buildables.Select(b => b.Rule).ToHashSet(new Rule.Comparer())) ninja.Rule(rule);

            ninja.Newline();
            ninja.Comment("Build artifacts");
            foreach (Build build in buildables.Select(b => b.GetBuild()).ToHashSet(new Build.Comparer()))
                ninja.Build(build);


            if (!(Entrypoint is IBuildable buildable)) return;
            ninja.Newline();
            ninja.Comment("Default artifact to build when invoked directly (without parameters)");
            ninja.Default(buildable.GetBuild());
        }
    }
}