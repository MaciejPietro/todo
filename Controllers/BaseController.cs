using Microsoft.AspNetCore.Mvc;

namespace todo.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected ApplicationDbContext? DbContext => HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

    }
}
