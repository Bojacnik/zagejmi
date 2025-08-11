using Microsoft.AspNetCore.Mvc;
using Zagejmi.Server.Write.Features.People.Models;

namespace Zagejmi.Server.Write.Features.People.Controllers;

public class PersonController : Controller
{
    [HttpGet]
    public ActionResult<ModelPerson> Post()
    {
        return new ModelPerson
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Null"
        };

    } 
}