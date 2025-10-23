using System;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;
using System.IO;

namespace Infrastructure.NHibernate
{
    public static class NHibernateHelper
    {
        public static ISessionFactory BuildSessionFactory(string cfgPath = null)
        {
            var cfg = new Configuration();
            if (string.IsNullOrEmpty(cfgPath))
            {
                var baseDir = AppContext.BaseDirectory;
                cfgPath = Path.Combine(baseDir, "nhibernate.cfg.xml");
            }

            cfg.Configure(cfgPath);

            // Ensure mappings resolved: the nhibernate.cfg.xml contains mapping files with relative paths
            return cfg.BuildSessionFactory();
        }

        public static void CreateSchema(string cfgPath = null)
        {
            var cfg = new Configuration();
            if (string.IsNullOrEmpty(cfgPath))
            {
                var baseDir = AppContext.BaseDirectory;
                cfgPath = Path.Combine(baseDir, "nhibernate.cfg.xml");
            }

            cfg.Configure(cfgPath);

            var export = new SchemaExport(cfg);
            export.Create(true, true);
        }
    }
}
