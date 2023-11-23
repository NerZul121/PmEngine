using Microsoft.EntityFrameworkCore;
using PmEngine.Core.BaseClasses;
using PmEngine.Core.Interfaces;

namespace PmEngine.Core.Tests
{
    [Attributes.Priority(100)]
    internal class TestContext : BaseContext
    {
        public TestContext(IEngineConfigurator conf) : base(conf)
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var connString = "DataSource=file::memory:?cache=shared";
            optionsBuilder.UseLazyLoadingProxies().UseSqlite(connString);
        }
    }
}