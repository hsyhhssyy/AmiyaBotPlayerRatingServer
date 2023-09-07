﻿using AmiyaBotPlayerRatingServer.Controllers.Policy;
using AmiyaBotPlayerRatingServer.Data;
using AmiyaBotPlayerRatingServer.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Polly;
using System.Security.Claims;

namespace AmiyaBotPlayerRatingServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Roles= "普通账户")]
    public class SKLandCredentialController : ControllerBase
    {
        private readonly PlayerRatingDatabaseContext _context;

        public SKLandCredentialController(PlayerRatingDatabaseContext context)
        {
            _context = context;
        }

        public class SKLandCredentialModel
        {
            public string Credential { get; set; }
            // 可能还有其他字段，比如昵称、头像URL等
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateCredential([FromBody] SKLandCredentialModel model)
        {
            // 从当前用户的Claims获取用户ID
            var userId = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // 验证Credential是否已经存在
            var existingCredential = await _context.SKLandCredentials
                .FirstOrDefaultAsync(c => c.Credential == model.Credential && c.UserId == userId);

            if (existingCredential != null)
            {
                return BadRequest("Credential already exists for this user.");
            }

            // 创建新的SKLandCredential
            var newCredential = new SKLandCredential
            {
                UserId = userId,
                Credential = model.Credential,
                // 如果有其他字段，例如昵称或头像URL，也应该在这里设置
            };

            _context.SKLandCredentials.Add(newCredential);

            // 保存更改
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // 记录异常或执行其他错误处理逻辑
                return StatusCode(500, "An error occurred while creating the credential.");
            }

            return Ok(new { Id = newCredential.Id, Message = "Credential successfully created." });
        }

        [HttpPut("Update/{credentialId}")]
        [Authorize(Policy = CredentialOwnerPolicy.Name)]
        public IActionResult UpdateCredential(string credentialId)
        {
            return Ok();
        }

        [HttpDelete("Delete/{credentialId}")]
        [Authorize(Policy = CredentialOwnerPolicy.Name)]
        public IActionResult DeleteCredential(string credentialId)
        {
            return Ok();
        }

        [HttpGet("List")]
        public IActionResult GetCredentials()
        {
            return Ok();
        }

        [HttpGet("Detail/{credentialId}")]
        [Authorize(Policy = CredentialOwnerPolicy.Name)]
        public IActionResult GetCredentialDetails(string credentialId)
        {
            return Ok();
        }
    }

}
