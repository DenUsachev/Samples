using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Samshit.CrossCutting
{
    public class ApiControllerBase : ControllerBase
    {
        //public override OkResult Ok()
        //{
        //    var okResult = new Samshit.DataModels.Mics.EmptyResult((int) HttpStatusCode.OK, nameof(HttpStatusCode.OK));
        //    return base.Ok(okResult);
        //}
    }
}
