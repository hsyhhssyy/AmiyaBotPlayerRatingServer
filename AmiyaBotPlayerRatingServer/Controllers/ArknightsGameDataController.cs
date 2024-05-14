﻿using AmiyaBotPlayerRatingServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AmiyaBotPlayerRatingServer.Controllers
{
    [ApiController]
    [Route("api/arknights")]
    [Produces("application/json")]
    public class ArknightsGameDataController : ControllerBase
    {
        private readonly ArknightsMemoryCache _memeCache;

        public ArknightsGameDataController(ArknightsMemoryCache memeCache)
        {
            _memeCache = memeCache;
        }

        public class JsonPathQueryModel
        {
            public string File { get; set; }
            public string Query { get; set; }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpPost("/json-path")]
        public IActionResult Query([FromQuery] JsonPathQueryModel model)
        {
            try
            {
                var jsonData = _memeCache.GetJson(model.File);
                if (jsonData != null)
                {
                    var result = jsonData.SelectToken(model.Query);
                    return Ok(result);
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}