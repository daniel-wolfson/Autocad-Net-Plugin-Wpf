namespace ID.Infrastructure
{
    public interface IConfigurationBuilder
    {
        IConfigurationBuilder SetBasePath(string workPath);
        IConfigurationBuilder AddJsonFile(string fileName, bool optional = true, bool reloadOnChange = true);
        IConfigurationBuilder Build();
        T GetSection<T>(string sectionName = null);
    }
}