using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data
{
    public class NopObjectContextFactory : IDesignTimeDbContextFactory<NopObjectContext>
    {
        public NopObjectContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<NopObjectContext>();
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=nop;Integrated Security=False;Persist Security Info=False;User ID=sa;Password=\"9o0p-[=]\"");

            return new NopObjectContext(optionsBuilder.Options);
        }
    }
}
