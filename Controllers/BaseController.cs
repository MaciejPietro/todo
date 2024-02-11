using Microsoft.AspNetCore.Mvc;

namespace todo.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        //public readonly ApplicationDbContext _db;

        //public BaseController(ApplicationDbContext db)
        //{
        //    _db = db;
        //}
        protected ApplicationDbContext DbContext => HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

    }
}
