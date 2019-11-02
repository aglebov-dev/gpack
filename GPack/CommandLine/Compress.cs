using CommandLine;

namespace gpack.CommandLine
{
    [Verb("compress", HelpText = "Compress file")]
    public class Compress: Command
    {
        [Value(0, MetaName = "source", Required = true, HelpText = "Source file path")]
        public string Source { get; set; }

        [Value(1, MetaName = "target", Required = true, HelpText = "Destination file path")]
        public string Target { get; set; }
    }
}
