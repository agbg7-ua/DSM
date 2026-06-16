using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NhEnvironment = global::NHibernate.Cfg.Environment;

namespace Infrastructure.NHibernate;

public static class NHibernateHelper
{
    public static ISessionFactory BuildSessionFactory(string? connectionStringOverride = null)
    {
        var config = LoadConfiguration(connectionStringOverride);
        return config.BuildSessionFactory();
    }

    public static Configuration LoadConfiguration(string? connectionStringOverride = null)
    {
        var baseDir = AppContext.BaseDirectory;
        var cfgPath = ResolveConfigPath(baseDir);

        Configuration config;
        if (cfgPath is not null)
        {
            config = new Configuration().Configure(cfgPath);
            AddMappingsFromDirectory(config, AppContext.BaseDirectory);
        }
        else
        {
            config = new Configuration();
            AddMappingsFromDirectory(config, baseDir);
        }

        if (!string.IsNullOrWhiteSpace(connectionStringOverride))
        {
            config.SetProperty(NhEnvironment.ConnectionString, connectionStringOverride);
        }

        return config;
    }

    public static void ExportSchema(Configuration config)
    {
        var exporter = new SchemaExport(config);
        exporter.Create(true, true);
    }

    private static string? ResolveConfigPath(string baseDir)
    {
        var candidates = new[]
        {
            Path.Combine(baseDir, "NHibernate.cfg.xml"),
            Path.Combine(baseDir, "NHibernate", "NHibernate.cfg.xml")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static void AddMappingsFromDirectory(Configuration config, string baseDir)
    {
        var addedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var searchDirs = new List<string>
        {
            Path.Combine(baseDir, "NHibernate", "Mappings"),
            Path.Combine(baseDir, "Mappings")
        };

        if (!TryAddMappingsFromDirs(config, searchDirs, addedFiles))
        {
            var devFallback = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "NHibernate", "Mappings");
            TryAddMappingsFromDirs(config, new[] { devFallback }, addedFiles);
        }
    }

    private static bool TryAddMappingsFromDirs(
        Configuration config,
        IEnumerable<string> searchDirs,
        HashSet<string> addedFiles)
    {
        var addedAny = false;

        foreach (var dir in searchDirs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var file in Directory.GetFiles(dir, "*.hbm.xml"))
            {
                var absolutePath = Path.GetFullPath(file);
                if (addedFiles.Add(absolutePath))
                {
                    config.AddFile(absolutePath);
                    addedAny = true;
                }
            }
        }

        return addedAny;
    }
}
