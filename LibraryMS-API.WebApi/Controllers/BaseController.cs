using Microsoft.AspNetCore.Mvc;

namespace LibraryMS_API.WebApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
    }
}
