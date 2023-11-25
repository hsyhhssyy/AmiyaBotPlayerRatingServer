﻿using System.Security.Claims;
using AmiyaBotPlayerRatingServer.Data;
using AmiyaBotPlayerRatingServer.Hangfire;
using AmiyaBotPlayerRatingServer.Model;
using Hangfire;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AmiyaBotPlayerRatingServer.Controllers.MAAControllers
{
    [ApiController]
    [Route("api/maaConnections")]
    public class MAAConnectionController : ControllerBase
    {
        private readonly PlayerRatingDatabaseContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ILogger<MAAConnectionController> _logger;

        public MAAConnectionController(
            PlayerRatingDatabaseContext context,
            IBackgroundJobClient backgroundJobClient,
            IRecurringJobManager recurringJobManager,
            ILogger<MAAConnectionController> logger)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _recurringJobManager = recurringJobManager;
            _logger = logger;
        }

        #region Data Objects

#pragma warning disable CS8618
        // ReSharper disable UnusedAutoPropertyAccessor.Global

        public class AddTaskModel
        {
            public String Type { get; set; }
            public String Parameters { get; set; }
        }

        public class AddRepetitiveTaskModel
        {
            public String Name { get; set; }
            public String Type { get; set; }
            public String Parameters { get; set; }
            public String UtcCronString { get; set; }
            public DateTime AvailableFrom { get; set; }
            public DateTime? AvailableTo { get; set; }
        }

        public class CompleteConnectionModel
        {
            public String DeviceIdentity { get; set; }
            public String Name { get; set; }
        }

        // ReSharper restore UnusedAutoPropertyAccessor.Global
#pragma warning restore CS8618

        #endregion

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet]
        public async Task<IActionResult> ListConnections()
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                // 查询数据库获取该用户的所有MAAConnections
                var connections = await _context.MAAConnections
                    .Where(c => c.UserId == userId&&!String.IsNullOrWhiteSpace(c.Name))
                    .Select(c => new
                    {
                        c.Id,
                        c.DeviceIdentity,
                        c.UserIdentity,
                        c.Name
                    })
                    .ToListAsync();

                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "列出连接时发生错误。"); // 使用Logger记录错误
                return StatusCode(500, "获取连接列表时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConnection(Guid id)
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                // 查询数据库获取该用户的所有MAAConnections
                var connection = await _context.MAAConnections
                    .Where(c => c.UserId == userId && c.Id == id)
                    .Select(c => new
                    {
                        c.Id,
                        c.DeviceIdentity,
                        c.UserIdentity,
                        c.Name
                    })
                    .FirstOrDefaultAsync();

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                return Ok(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取连接时发生错误。"); // 使用Logger记录错误
                return StatusCode(500, "获取连接时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpPost]
        public async Task<IActionResult> InitializeConnection()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            // 生成UserIdentity
            var userIdentity = Guid.NewGuid().ToString("N");

            var connection = new MAAConnection
            {
                UserId = userId,
                UserIdentity = userIdentity,
                DeviceIdentity = null // 初始时DeviceIdentity为空
            };

            await _context.MAAConnections.AddAsync(connection);
            await _context.SaveChangesAsync();

            // 使用Hangfire创建删除任务
            _backgroundJobClient.Schedule(
                () => DeleteUnconfirmedConnection(connection.Id),
                TimeSpan.FromMinutes(5));

            return Ok(new { connection.Id, connection.UserIdentity });
        }

        [NonAction]
        [UsedImplicitly]
        public async Task DeleteUnconfirmedConnection(Guid connectionId)
        {
            try
            {
                var connection = await _context.MAAConnections.FindAsync(connectionId);
                if (connection != null && string.IsNullOrEmpty(connection.DeviceIdentity))
                {
                    _context.MAAConnections.Remove(connection);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "尝试删除未确认的连接时发生错误，连接ID：{ConnectionId}", connectionId);
            }
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> CompleteConnection(Guid id, [FromBody] CompleteConnectionModel model)
        {
            if (string.IsNullOrEmpty(model.DeviceIdentity))
            {
                return BadRequest("DeviceIdentity不能为空。");
            }

            var connection = await _context.MAAConnections.FindAsync(id);

            if (connection == null)
            {
                return NotFound("连接未找到。");
            }

            if (connection.UserId != User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                return NotFound("连接未找到。");
            }

            // 确认该连接没有被其他设备占用
            var existingConnectionWithDevice = await _context.MAAConnections
                .AnyAsync(c => c.DeviceIdentity == model.DeviceIdentity);
            if (existingConnectionWithDevice)
            {
                return BadRequest("DeviceIdentity已经被占用。");
            }

            // 完成Connection的创建
            connection.DeviceIdentity = model.DeviceIdentity;
            connection.Name = model.Name;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "成功创建连接。" });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConnection(Guid id)
        {
            // 获取当前用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            // 从数据库中找到对应的Connection
            var connectionToDelete = await _context.MAAConnections
                .FirstOrDefaultAsync(c => c.Id == id);

            // 检查是否存在该Connection
            if (connectionToDelete == null)
            {
                return NotFound("连接未找到。");
            }

            // 检查用户是否有权限删除该Connection
            if (connectionToDelete.UserId != userId)
            {
                return NotFound("连接未找到。");
            }

            // 删除该Connection
            _context.MAAConnections.Remove(connectionToDelete);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "成功删除连接。" });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{id}/maaTasks")]
        public async Task<IActionResult> ListTasks(Guid id, [FromQuery] string? repetitiveTaskId, [FromQuery]int page, [FromQuery]int size, [FromQuery]bool showSystem=false)
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }
                
                var tasks = await _context.MAATasks
                    .Where(t => t.ConnectionId == connection.Id && (showSystem || !t.IsSystemGenerated))
                    .OrderByDescending(t => t.CreatedAt)
                    .Where(t=> repetitiveTaskId == null||t.ParentRepetitiveTaskId==Guid.Parse(repetitiveTaskId))
                    .Skip(page * size)
                    .Take(size)
                    .Select(t => new
                    {
                        t.Id,
                        t.Type,
                        t.Parameters,
                        t.IsCompleted,
                        t.CreatedAt,
                        t.CompletedAt,
                        t.IsSystemGenerated,
                        t.ParentRepetitiveTaskId,
                    })
                    .ToListAsync();

                var total = await _context.MAATasks.Where(t => repetitiveTaskId == null || t.ParentRepetitiveTaskId == Guid.Parse(repetitiveTaskId)).CountAsync(t => t.ConnectionId == connection.Id);

                return Ok(new
                {
                    tasks=tasks,
                    total= total,
                    maxPage= tasks.Count==0?0: Math.Ceiling((double)total / size),
                    page =page,
                    size=size,

                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "列出任务时发生错误。");
                return StatusCode(500, "获取任务列表时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{id}/maaTasks/{taskId}")]
        public async Task<IActionResult> GetTask(Guid id, Guid taskId)
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                var task = await _context.MAATasks
                    .Where(t => t.ConnectionId == connection.Id && t.Id == taskId)
                    .Select(t => new
                    {
                        t.Id,
                        t.Type,
                        t.Parameters,
                        t.IsCompleted,
                        t.CreatedAt,
                        t.CompletedAt
                    })
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    return NotFound("指定的任务不存在。");
                }

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务时发生错误。");
                return StatusCode(500, "获取任务时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{id}/image")]
        public async Task<IActionResult> GetConnectionImage(Guid id, [FromQuery] String? type)
        {
            //type: original/null, thumbnail

            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                var latestScreenShot = await _context.MAATasks
                    .Where(s => s.ConnectionId == id && s.IsCompleted == true && (s.Type == "CaptureImage" || s.Type == "CaptureImageNow"))
                    .OrderByDescending(s => s.CompletedAt)
                    .FirstOrDefaultAsync();

                if (latestScreenShot == null)
                {
                    return Ok(new
                    {
                        Image = ""
                    });
                }

                var result = _context.MAAResponses.FirstOrDefault(r => r.TaskId == latestScreenShot.Id);

                if (result == null)
                {
                    return Ok(new
                    {
                        Image = ""
                    });
                }

                if (type == "thumbnail")
                {
                    return Ok(new
                    {
                        Image = result.ImagePayloadThumbnail
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Image = result.ImagePayload
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务时发生错误。");
                return StatusCode(500, "获取任务时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{id}/maaTasks/{taskId}/image")]
        public async Task<IActionResult> GetTaskImage(Guid id, Guid taskId, [FromQuery] String? type)
        {
            //type: original/null, thumbnail

            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                var task = await _context.MAATasks
                    .Where(t => t.ConnectionId == connection.Id && t.Id == taskId)
                    .Include(t => t.SubTasks)
                    .FirstOrDefaultAsync();

                if (task == null)
                {
                    return NotFound("指定的任务不存在。");
                }

                MAAResponse? result = null;
                if (task.Type == "CaptureImage" || task.Type == "CaptureImageNow")
                {
                    result = _context.MAAResponses.FirstOrDefault(r => r.TaskId == task.Id);
                }
                else
                {
                    if (task.SubTasks?.Count > 0)
                    {
                        var capSubTask = task.SubTasks.FirstOrDefault(t => t.Type == "CaptureImage")?.Id;
                        result = _context.MAAResponses.FirstOrDefault(r => r.TaskId == capSubTask);
                    }
                }

                if (result == null)
                {
                    return Ok(new
                    {
                        Image = ""
                    });
                }

                if (type == "thumbnail")
                {
                    return Ok(new
                    {
                        Image = result.ImagePayloadThumbnail
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Image = result.ImagePayload
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务时发生错误。");
                return StatusCode(500, "获取任务时发生内部错误。");
            }
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpPost("{id}/maaTasks")]
        public async Task<IActionResult> AddTask(Guid id, [FromBody] AddTaskModel taskModel)
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                var userTask = new MAATask
                {
                    ConnectionId = connection.Id,
                    Type = taskModel.Type,
                    Parameters = taskModel.Parameters,
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false,
                    IsSystemGenerated = false
                };

                MAATask? captureTask = null;

                if (userTask.Type != "CaptureImage" && userTask.Type != "CaptureImageNow")
                {
                    captureTask = new MAATask
                    {
                        ConnectionId = connection.Id,
                        Type = "CaptureImage",
                        Parameters = null,
                        CreatedAt = DateTime.UtcNow,
                        IsCompleted = false,
                        IsSystemGenerated = true,
                        ParentTask = userTask
                    };
                }

                await _context.MAATasks.AddAsync(userTask);
                if (captureTask != null)
                {
                    await _context.MAATasks.AddAsync(captureTask);
                }
                await _context.SaveChangesAsync();
                

                return Ok(new
                {
                    userTask.Id,
                    userTask.Type,
                    userTask.Parameters,
                    userTask.IsCompleted,
                    userTask.CreatedAt,
                    userTask.CompletedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务时发生错误。");
                return StatusCode(500, "创建任务时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpPost("{connectionId}/maaRepetitiveTasks")]
        public async Task<ActionResult<MAARepetitiveTask>> AddRepetitiveTask(Guid connectionId, [FromBody] AddRepetitiveTaskModel taskModel)
        {
            // 从JWT中提取用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            try
            {
                var connection = await _context.MAAConnections
                    .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId);

                if (connection == null)
                {
                    return NotFound("指定的连接不存在。");
                }

                var userTask = new MAARepetitiveTask
                {
                    Name = taskModel.Name,

                    ConnectionId = connection.Id,
                    Type = taskModel.Type,
                    Parameters = taskModel.Parameters,
                    UtcCronString = taskModel.UtcCronString,

                    CreatedAt = DateTime.UtcNow,

                    AvailableFrom = taskModel.AvailableFrom.ToUniversalTime(),
                    AvailableTo = taskModel.AvailableTo?.ToUniversalTime(),
                };
                
                await _context.MAARepetitiveTasks.AddAsync(userTask);
                await _context.SaveChangesAsync();

                _backgroundJobClient.Enqueue<MAAExecuteRepetitiveTaskService>(service => service.CreateTask(userTask.Id.ToString()));

                return Ok(new
                {
                    userTask.Id,
                    userTask.Name,
                    userTask.Type,
                    userTask.Parameters,
                    userTask.UtcCronString,
                    userTask.CreatedAt,
                    userTask.AvailableFrom,
                    userTask.AvailableTo,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务时发生错误。");
                return StatusCode(500, "创建任务时发生内部错误。");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpDelete("{connectionId}/maaRepetitiveTasks/{taskId}")]
        public async Task<IActionResult> DeleteRepetitiveTask(Guid connectionId, Guid taskId)
        {
            // 获取当前用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }

            var userId = userIdClaim.Value;

            // 从数据库中找到对应的Connection
            var connection = await _context.MAAConnections
                .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId);

            // 检查是否存在该Connection
            if (connection == null)
            {
                return NotFound("连接未找到。");
            }

            // 从数据库中找到对应的RepetitiveTask
            var repetitiveTask = await _context.MAARepetitiveTasks
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ConnectionId == connectionId);

            // 检查是否存在该RepetitiveTask
            if (repetitiveTask == null)
            {
                return NotFound("任务未找到。");
            }

            // 检查用户是否有权限删除该RepetitiveTask
            if (repetitiveTask.ConnectionId != connectionId)
            {
                return NotFound("任务未找到。");
            }
            
            // RepetitiveTask不可以真正的删除，只能标记为已删除，因为它的子任务可能还在运行并且用户查看任务历史时需要查看已删除的任务
            repetitiveTask.IsDeleted = true;
            var jobId = $"MAARepetitiveTask-{repetitiveTask.Id}";
            _recurringJobManager.RemoveIfExists(jobId);
            _context.MAARepetitiveTasks.Update(repetitiveTask);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "成功删除任务。" });
            
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "普通账户")]
        [HttpGet("{connectionId}/maaRepetitiveTasks")]
        public async Task<ActionResult<IEnumerable<MAARepetitiveTask>>> ListRepetitiveTasks(Guid connectionId, [FromQuery] int? page, [FromQuery] int? size)
        {
            // 获取当前用户ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("用户未登录。");
            }
            var userId = userIdClaim.Value;

            // 从数据库中找到对应的Connection
            var connection = await _context.MAAConnections
                .FirstOrDefaultAsync(c => c.Id == connectionId && c.UserId == userId);

            // 检查是否存在该Connection
            if (connection == null)
            {
                return NotFound("连接未找到。");
            }

            IQueryable<MAARepetitiveTask> repetitiveTasks = _context.MAARepetitiveTasks
                .Where(t => t.ConnectionId == connectionId && t.IsDeleted==false)
                .OrderByDescending(t => t.CreatedAt);
            if (page != null && size != null)
            {
                repetitiveTasks = repetitiveTasks
                    .Skip(page.Value * size.Value)
                    .Take(size.Value);
            }
            var repetitiveTasksResult = await repetitiveTasks.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Type,
                    t.Parameters,
                    t.UtcCronString,
                    t.CreatedAt,
                    t.AvailableFrom,
                    t.AvailableTo,
                })
                .ToListAsync();

            var total = await _context.MAARepetitiveTasks.CountAsync(t => t.ConnectionId == connectionId);

            return Ok(new
            {
                repetitiveTasks = repetitiveTasks,
                total = total,
                maxPage = repetitiveTasksResult.Count==0||size==null?0:Math.Ceiling((double)total / size.Value),
                page = page??0,
                size = size ?? 0,
            });
        }

    }

}
