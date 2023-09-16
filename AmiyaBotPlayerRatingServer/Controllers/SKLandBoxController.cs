﻿using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AmiyaBotPlayerRatingServer.Controllers.Policy;
using AmiyaBotPlayerRatingServer.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using static AmiyaBotPlayerRatingServer.Controllers.SKLandCredentialController;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Newtonsoft.Json.Linq;

namespace AmiyaBotPlayerRatingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SKLandBoxController : ControllerBase
    {
        private readonly PlayerRatingDatabaseContext _context;

        public SKLandBoxController(PlayerRatingDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet("GetBoxByCredential/{credentialId}")]
        [Authorize(Policy = CredentialOwnerPolicy.Name)]
        public async Task<IActionResult> GetBoxByCredential(string credentialId)
        {
            // 获取当前用户ID
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 从数据库中找到对应的CharacterBox
            var characterBox = await _context.SKLandCharacterBoxes
                .Include(box => box.Credential)  // Include the related SKLandCredential
                .AsNoTracking()
                .FirstOrDefaultAsync(box => box.CredentialId == credentialId);

            if (characterBox == null)
            {
                return NotFound("Character box not found.");
            }

            return Ok(new
            {
                Id = characterBox.Id,
                CredentialId = characterBox.CredentialId,
                CharacterBoxJson = characterBox.CharacterBoxJson
            });
        }

        public class SKLandGetBoxModel
        {
            public string PartList { get; set; }
        }

        [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
        [HttpPost("GetBox")]
        public async Task<IActionResult> GetBox([FromBody] SKLandGetBoxModel model)
        {
            // 获取当前用户ID
            var userId = User.FindFirst(Claims.Subject)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var credClaimValue = User.FindFirst("SKLandCredentialId")?.Value;

            if (string.IsNullOrEmpty(credClaimValue))
            {
                return NotFound("该凭据没有对应的森空岛凭据.");
            }

            // 从数据库中找到对应的CharacterBox
            var characterBox = await _context.SKLandCharacterBoxes
                .Where(b=>b.CredentialId== credClaimValue).OrderByDescending(b=>b.RefreshedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (characterBox == null)
            {
                return NotFound("Character box not found.");
            }

            var infoData = JObject.Parse(characterBox.CharacterBoxJson);

            var boxParts = model.PartList.Split(',');

            var retObject = new Dictionary<String, Object>();

            foreach (var boxPart in boxParts)
            {
                if (boxPart == "status")
                {
                    //不允许直接访问Status块
                    var tempStatusBlock = new Dictionary<String, object>();
                    var statusData = infoData?["status"];
                    tempStatusBlock.Add("name", statusData["name"]);
                    tempStatusBlock.Add("level", statusData["level"]);
                    tempStatusBlock.Add("avatar", statusData["avatar"]);
                    tempStatusBlock.Add("mainStageProgress", statusData["mainStageProgress"]);
                    tempStatusBlock.Add("secretary", statusData["secretary"]);
                }
                else
                {
                    if (infoData.ContainsKey(boxPart))
                    {
                        retObject[boxPart] = infoData[boxPart];
                    }
                }
            }

            return Ok(new
            {
                Id = characterBox.Id,
                CredentialId = characterBox.CredentialId,
                code= 0,
                message= "OK",
                data = retObject
            });
        }
    }
}