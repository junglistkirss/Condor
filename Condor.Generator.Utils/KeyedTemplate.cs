namespace Condor.Generator.Utils
{
    public record KeyedTemplate
    {
        public KeyedTemplate(string key, string template)
        {
            Key = key;
            Template = template;
        }

        public string Key { get; }
        public string Template { get; }

    }
}
