using Microsoft.EntityFrameworkCore;
using ProgrammersBlog.Data.Abstract;
using ProgrammersBlog.Data.Concrete.EntityFramework.Contexts;
using ProgrammersBlog.Entities.Concrete;
using ProgrammersBlog.Shared.Data.Concrete.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammersBlog.Data.Concrete.EntityFramework.Repositories
{
    public class EfCategoryRepository : EfEntityRepository<Category>, ICategoryRepository
    {
        public EfCategoryRepository(DbContext context) : base(context)
        {
        }


        private ProgrammersBlogContext ProgrammersBlogContext
        {
            get
            {
                return _context as ProgrammersBlogContext;
            }
        }


        public async Task<Category>GetById(int categoryId)
        {
            return await ProgrammersBlogContext.Categories.SingleOrDefaultAsync(c => c.Id == categoryId);
        }



    }
}
