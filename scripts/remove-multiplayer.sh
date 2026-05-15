# 多人模式代码删除脚本
# 执行方式: 在项目根目录运行 bash scripts/remove-multiplayer.sh

echo "=== 删除多人模式相关代码 ==="

# 1. Transport 层 - 网络传输
echo "删除 Steam 和 ENet 传输层..."
rm -rf src/Core/Multiplayer/Transport/Steam
rm -rf src/Core/Multiplayer/Transport/ENet

# 2. Messages - 网络消息
echo "删除网络消息目录..."
rm -rf src/Core/Multiplayer/Messages

# 3. Lobby - 大厅管理
echo "删除大厅管理目录..."
rm -rf src/Core/Multiplayer/Game/Lobby

# 4. PeerInput - 玩家输入同步
echo "删除玩家输入同步目录..."
rm -rf src/Core/Multiplayer/Game/PeerInput

# 5. Connection - 连接初始化
echo "删除连接初始化目录..."
rm -rf src/Core/Multiplayer/Connection

# 6. Serialization - 网络序列化
echo "删除网络序列化目录..."
rm -rf src/Core/Multiplayer/Serialization

# 7. Quality - 网络质量
echo "删除网络质量目录..."
rm -rf src/Core/Multiplayer/Quality

# 8. Replay - 回放系统
echo "删除回放系统目录..."
rm -rf src/Core/Multiplayer/Replay

# 9. Steam 平台工具
echo "删除 Steam 平台目录..."
rm -rf src/Core/Platform/Steam

# 10. 多人 UI 节点
echo "删除多人 UI 节点目录..."
rm -rf src/Core/Nodes/Multiplayer

# 11. 多人调试工具
echo "删除多人调试工具目录..."
rm -rf src/Core/Nodes/Debug/Multiplayer

# 12. 多人实体
echo "删除多人实体目录..."
rm -rf src/Core/Entities/Multiplayer

# 13. 多人游戏行动队列
echo "删除多人行动队列目录..."
rm -rf src/Core/GameActions/Multiplayer

# 14. 单个多人服务文件
echo "删除多人服务实现文件..."
rm -f src/Core/Multiplayer/NetHostGameService.cs
rm -f src/Core/Multiplayer/NetClientGameService.cs
rm -f src/Core/Multiplayer/NetReplayGameService.cs
rm -f src/Core/Multiplayer/NetMessageBus.cs
rm -f src/Core/Multiplayer/NetConst.cs
rm -f src/Core/Multiplayer/MultiplayerDebugUtil.cs

# 15. 多人同步器文件
echo "删除多人同步器文件..."
rm -f src/Core/Multiplayer/Game/ActChangeSynchronizer.cs
rm -f src/Core/Multiplayer/Game/EventSynchronizer.cs
rm -f src/Core/Multiplayer/Game/FlavorSynchronizer.cs
rm -f src/Core/Multiplayer/Game/MapSelectionSynchronizer.cs
rm -f src/Core/Multiplayer/Game/MapVote.cs
rm -f src/Core/Multiplayer/Game/OneOffSynchronizer.cs
rm -f src/Core/Multiplayer/Game/ReactionSynchronizer.cs
rm -f src/Core/Multiplayer/Game/RestSiteSynchronizer.cs
rm -f src/Core/Multiplayer/Game/RewardSynchronizer.cs
rm -f src/Core/Multiplayer/Game/TreasureRoomRelicSynchronizer.cs
rm -f src/Core/Multiplayer/Game/JoinFlow.cs
rm -f src/Core/Multiplayer/Game/JoinResult.cs
rm -f src/Core/Multiplayer/Game/NetLoadingHandle.cs
rm -f src/Core/Multiplayer/Game/RunLocationTargetedMessageBuffer.cs
rm -f src/Core/Multiplayer/Game/StateDivergenceException.cs
rm -f src/Core/Multiplayer/Game/ChecksumTracker.cs
rm -f src/Core/Multiplayer/CombatStateSynchronizer.cs

# 16. 多人屏幕文件
echo "删除多人屏幕文件..."
rm -f src/Core/Nodes/Screens/CharacterSelect/NMultiplayerLoadGameScreen.cs
rm -f src/Core/Nodes/Screens/MainMenu/NJoinFriendScreen.cs
rm -f src/Core/Nodes/Screens/MainMenu/NJoinFriendButton.cs

# 17. Transport 基类（如果不再需要）
# 保留 NetClient.cs, NetHost.cs 作为接口

# 18. INetClientGameService 和 INetHostGameService
rm -f src/Core/Multiplayer/Game/INetClientGameService.cs
rm -f src/Core/Multiplayer/Game/INetHostGameService.cs

echo "=== 删除完成 ==="
echo "注意: 以下文件需要保留:"
echo "  - src/Core/Multiplayer/NetSingleplayerGameService.cs (单人模式服务)"
echo "  - src/Core/Multiplayer/Game/INetGameService.cs (接口定义)"
echo "  - src/Core/Multiplayer/Game/NetGameType.cs (枚举，需简化)"
