using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Controllers
{
    [ApiController]
    public class MyBaseController<T> : ControllerBase
    {
        private ISender _sender = null;

        public ISender Sender => _sender ??= HttpContext.RequestServices.GetService<ISender>();

    }
}
