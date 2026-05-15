# 多人模式代码删除脚本 - PowerShell 版本
# 执行方式: 在项目根目录运行 powershell -ExecutionPolicy Bypass -File scripts/remove-multiplayer.ps1

Write-Host "=== 删除多人模式相关代码 ===" -ForegroundColor Cyan

# 1. Transport 层 - 网络传输
Write-Host "删除 Steam 和 ENet 传输层..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Transport/Steam" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Transport/ENet" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Messages - 网络消息
Write-Host "删除网络消息目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Messages" -Recurse -Force -ErrorAction SilentlyContinue

# 3. Lobby - 大厅管理
Write-Host "删除大厅管理目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/Lobby" -Recurse -Force -ErrorAction SilentlyContinue

# 4. PeerInput - 玩家输入同步
Write-Host "删除玩家输入同步目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/PeerInput" -Recurse -Force -ErrorAction SilentlyContinue

# 5. Connection - 连接初始化
Write-Host "删除连接初始化目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Connection" -Recurse -Force -ErrorAction SilentlyContinue

# 6. Serialization - 网络序列化
Write-Host "删除网络序列化目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Serialization" -Recurse -Force -ErrorAction SilentlyContinue

# 7. Quality - 网络质量
Write-Host "删除网络质量目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Quality" -Recurse -Force -ErrorAction SilentlyContinue

# 8. Replay - 回放系统
Write-Host "删除回放系统目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Replay" -Recurse -Force -ErrorAction SilentlyContinue

# 9. Steam 平台工具
Write-Host "删除 Steam 平台目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Platform/Steam" -Recurse -Force -ErrorAction SilentlyContinue

# 10. 多人 UI 节点
Write-Host "删除多人 UI 节点目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Nodes/Multiplayer" -Recurse -Force -ErrorAction SilentlyContinue

# 11. 多人调试工具
Write-Host "删除多人调试工具目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Nodes/Debug/Multiplayer" -Recurse -Force -ErrorAction SilentlyContinue

# 12. 多人实体
Write-Host "删除多人实体目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Entities/Multiplayer" -Recurse -Force -ErrorAction SilentlyContinue

# 13. 多人游戏行动队列
Write-Host "删除多人行动队列目录..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/GameActions/Multiplayer" -Recurse -Force -ErrorAction SilentlyContinue

# 14. 单个多人服务文件
Write-Host "删除多人服务实现文件..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/NetHostGameService.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/NetClientGameService.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/NetReplayGameService.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/NetMessageBus.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/NetConst.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/MultiplayerDebugUtil.cs" -Force -ErrorAction SilentlyContinue

# 15. 多人同步器文件
Write-Host "删除多人同步器文件..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/ActChangeSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/EventSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/FlavorSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/MapSelectionSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/MapVote.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/OneOffSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/ReactionSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/RestSiteSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/RewardSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/TreasureRoomRelicSynchronizer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/JoinFlow.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/JoinResult.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/ChecksumTracker.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/CombatStateSynchronizer.cs" -Force -ErrorAction SilentlyContinue

# 16. 多人屏幕文件
Write-Host "删除多人屏幕文件..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Nodes/Screens/CharacterSelect/NMultiplayerLoadGameScreen.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Nodes/Screens/MainMenu/NJoinFriendScreen.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Nodes/Screens/MainMenu/NJoinFriendButton.cs" -Force -ErrorAction SilentlyContinue

# 17. 接口定义文件
Write-Host "删除多人接口定义..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/INetClientGameService.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/INetHostGameService.cs" -Force -ErrorAction SilentlyContinue

# 18. NetGameTypeExtensions
Write-Host "删除 NetGameTypeExtensions..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/NetGameTypeExtensions.cs" -Force -ErrorAction SilentlyContinue

# 19. NetLoadingHandle
Write-Host "删除 NetLoadingHandle..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/NetLoadingHandle.cs" -Force -ErrorAction SilentlyContinue

# 20. 其他同步器文件
Write-Host "删除剩余同步器和消息文件..." -ForegroundColor Yellow
Remove-Item -Path "src/Core/Multiplayer/Game/RunLocationTargetedMessageBuffer.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/StateDivergenceException.cs" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "src/Core/Multiplayer/Game/MessageHandlerDelegate.cs" -Force -ErrorAction SilentlyContinue

Write-Host "=== 删除完成 ===" -ForegroundColor Green
Write-Host "注意: 以下文件已保留并简化:" -ForegroundColor Cyan
Write-Host "  - src/Core/Multiplayer/NetSingleplayerGameService.cs (单人模式服务)" -ForegroundColor White
Write-Host "  - src/Core/Multiplayer/Game/INetGameService.cs (简化接口定义)" -ForegroundColor White
Write-Host "  - src/Core/Multiplayer/Game/NetGameType.cs (简化枚举，仅 Singleplayer)" -ForegroundColor White
