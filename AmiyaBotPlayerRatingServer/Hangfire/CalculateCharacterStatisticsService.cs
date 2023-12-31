﻿using Aliyun.OSS;
using AmiyaBotPlayerRatingServer.Data;
using AmiyaBotPlayerRatingServer.Model;
using Newtonsoft.Json.Linq;
using static AmiyaBotPlayerRatingServer.Controllers.CalculateNowController;
using System.Text.RegularExpressions;
// ReSharper disable PossibleInvalidCastExceptionInForeachLoop
// ReSharper disable UnusedVariable
// ReSharper disable RedundantCast
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS8602
#pragma warning disable CS8600

namespace AmiyaBotPlayerRatingServer.Hangfire
{
    public class CalculateCharacterStatisticsService
    {
        private readonly IConfiguration _configuration;
        private readonly PlayerRatingDatabaseContext _dbContext;
        private readonly JObject _characterMap;

        public CalculateCharacterStatisticsService(IConfiguration configuration, PlayerRatingDatabaseContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CharacterMap.json");
            string fileContent = File.ReadAllText(filePath);
            _characterMap = JObject.Parse(fileContent);

        }
        
        public void Calculate(DateTime startDate, DateTime endDate)
        {
            string localDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "AliyunOss");
            if (!Directory.Exists(localDirectoryPath))
            {
                Directory.CreateDirectory(localDirectoryPath);
            }

            // 从appsettings.json获取OSS相关设置
            string endPoint = _configuration["Aliyun:Oss:EndPoint"]!;
            string bucketName = _configuration["Aliyun:Oss:Bucket"]!;
            string key = _configuration["Aliyun:Oss:Key"]!;
            string secret = _configuration["Aliyun:Oss:Secret"]!;

            // 初始化OSS客户端
            var client = new OssClient(endPoint, key, secret);

            string nextMarker = string.Empty;
            string prefix = "collected_data_v1/"; // Directory prefix
            
            do
            {
                var listObjectsRequest = new ListObjectsRequest(bucketName)
                {
                    Marker = nextMarker,
                    MaxKeys = 100,
                    Prefix = prefix
                };

                var result = client.ListObjects(listObjectsRequest);

                foreach (var summary in result.ObjectSummaries)
                {
                    string fileName = summary.Key;

                    // 检查本地文件是否已存在
                    string localFilePath = Path.Combine(localDirectoryPath, Path.GetFileName(fileName));
                    if (File.Exists(localFilePath))
                    {
                        // 如果文件已存在，跳过下载
                        continue;
                    }

                    // 否则，下载文件
                    var stream = client.GetObject(bucketName, fileName).Content;
                    using (FileStream fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                        fs.Flush();
                    }
                }

                nextMarker = result.NextMarker;

            } while (!string.IsNullOrEmpty(nextMarker));

            // 现在，从 localDirectoryPath 下读取这些文件
            // 首先，获取该目录下所有文件的名称
            string[] files = Directory.GetFiles(localDirectoryPath);

            Dictionary<string, string> latestFiles = new Dictionary<string, string>();

            // File name regular expression
            Regex regex = new Regex(@"chars\.(\w+)\.(\d{8})\.(\d{6})\.json");

            foreach (var filePath in files)
            {
                string fileName = Path.GetFileName(filePath);

                // 使用正则表达式从文件名中提取日期和时间信息
                var match = regex.Match(fileName);
                if (match.Success)
                {
                    string userId = match.Groups[1].Value;
                    string dateStr = match.Groups[2].Value;
                    string timeStr = match.Groups[3].Value;

                    DateTime fileDate;
                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out fileDate))
                    {
                        if (fileDate >= startDate && fileDate <= endDate)
                        {
                            // 检查这个文件是否是该用户的最新文件
                            if (latestFiles.ContainsKey(userId))
                            {
                                string existingFileName = latestFiles[userId];
                                var existingMatch = regex.Match(existingFileName);
                                string existingDateStr = existingMatch.Groups[2].Value;
                                string existingTimeStr = existingMatch.Groups[3].Value;

                                if (String.CompareOrdinal(dateStr + timeStr, existingDateStr + existingTimeStr) > 0)
                                {
                                    // 这个文件更新
                                    latestFiles[userId] = fileName;
                                }
                            }
                            else
                            {
                                latestFiles[userId] = fileName;
                            }
                        }
                    }
                }
            }

            // Now, process the latest file for each user

            Dictionary<string, AccumulatedCharacterData> accumulatedData =
                new Dictionary<string, AccumulatedCharacterData>();

            foreach (var userId in latestFiles.Keys)
            {
                string latestFile = latestFiles[userId];

                // Read the latest file content
                var stream = client.GetObject(bucketName, latestFile).Content;

                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonContent = reader.ReadToEnd();

                    // Handle the JSON content
                    JArray characterArray = JArray.Parse(jsonContent);

                    foreach (JObject character in characterArray)
                    {
                        string charId = character["charId"].ToString();
                        int evolvePhase = character["evolvePhase"].ToObject<int>();
                        int level = character["level"].ToObject<int>();
                        int mainSkillLvl = character["mainSkillLvl"].ToObject<int>();

                        if (_characterMap != null && _characterMap.ContainsKey(charId))
                        {

                            // 初始化或获取AccumulatedCharacterData
                            if (!accumulatedData.ContainsKey(charId))
                            {
                                accumulatedData[charId] = new AccumulatedCharacterData();
                            }

                            var data = accumulatedData[charId];

                            // 更新计数和其他统计信息
                            data.Count++;

                            data.TotalLevel += CalculateLevel(evolvePhase, level, charId);

                            data.TotalSkillLevel += mainSkillLvl;

                            // 更新平均专精等级
                            JArray skills = (JArray)character["skills"];
                            foreach (JObject skill in skills)
                            {
                                string skillId = skill["id"].ToString();
                                int specializeLevel = skill["specializeLevel"].ToObject<int>();

                                if (!data.SpecializeLevel.ContainsKey(skillId))
                                {
                                    data.SpecializeLevel[skillId] = (0, 0);
                                }

                                var (count, totalLevel) = data.SpecializeLevel[skillId];
                                data.SpecializeLevel[skillId] = (count + 1, totalLevel + specializeLevel);
                            }

                            // 更新平均模组等级
                            JArray equips = (JArray)character["equip"];
                            int equipIndex = 0;
                            foreach (JObject equip in equips)
                            {
                                int equipLevel = equip["level"].ToObject<int>();

                                if (!data.EquipLevel.ContainsKey(equipIndex))
                                {
                                    data.EquipLevel[equipIndex] = (0, 0);
                                }

                                var (count, totalLevel) = data.EquipLevel[equipIndex];
                                data.EquipLevel[equipIndex] = (count + 1, totalLevel + equipLevel);

                                equipIndex++;
                            }
                        }
                    }

                }
            }

            //最后，遍历accumulatedData并计算平均值，然后存入数据库：

            // 删除 CharacterStatistics 表中的所有记录(测试时临时代码)
            _dbContext.CharacterStatistics.RemoveRange(_dbContext.CharacterStatistics);

            // 提交更改以清空表
            _dbContext.SaveChanges();

            foreach (var charId in accumulatedData.Keys)
            {
                var data = accumulatedData[charId];

                if (_characterMap != null&& _characterMap.ContainsKey(charId))
                {
                    var rarity = _characterMap[charId]["rarity"].ToObject<int>();
                    // 创建新的 CharacterStatistics 实例
                    CharacterStatistics statistics = new CharacterStatistics
                    {
                        Id = Guid.NewGuid().ToString(),
                        VersionStart = startDate.ToUniversalTime(),
                        VersionEnd = endDate.ToUniversalTime(),
                        SampleCount = data.Count,
                        BatchCount = latestFiles.Keys.Count,
                        CharacterId = charId,
                        Rarity = rarity,
                        AverageLevel = data.TotalLevel / data.Count,
                        AverageSkillLevel = data.TotalSkillLevel / data.Count
                    };

                    // 计算平均专精等级
                    List<double> avgSpecializeLevels = new List<double>();
                    foreach (var (skillId, (count, totalLevel)) in data.SpecializeLevel)
                    {
                        avgSpecializeLevels.Add(totalLevel / (double)count);
                    }

                    statistics.AverageSpecializeLevel = avgSpecializeLevels;

                    // 计算平均模组等级
                    Dictionary<int, double> avgEquipLevels = new Dictionary<int, double>();
                    foreach (var (equipIndex, (count, totalLevel)) in data.EquipLevel)
                    {
                        avgEquipLevels[equipIndex] = totalLevel / (double)count;
                    }

                    statistics.AverageEquipLevel = avgEquipLevels;

                    // 添加到数据库
                    _dbContext.CharacterStatistics.Add(statistics);
                }
            }

            _dbContext.SaveChanges();

        }

        private double CalculateLevel(int evolvePhase, int level, string charId)
        {
            var rarity = _characterMap[charId]["rarity"].ToObject<int>();
            int baseIncrease = 0;
            int evolveIncrease = 0;

            switch (rarity)
            {
                case 5:
                    baseIncrease = 50;
                    evolveIncrease = 80;
                    break;
                case 4:
                    baseIncrease = 50;
                    evolveIncrease = 70;
                    break;
                case 3:
                    baseIncrease = 45;
                    evolveIncrease = 60;
                    break;
                case 2:
                    baseIncrease = 40;
                    break;
            }

            if (evolvePhase == 0) return level;
            if (evolvePhase == 1) return level + baseIncrease;
            if (evolvePhase == 2) return level + baseIncrease + evolveIncrease;

            return level;
        }

    }

}
