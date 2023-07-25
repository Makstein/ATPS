# ![](https://github.com/Makstein/ATPS/blob/master/Images/AnimationDemo.gif)

# ATPS

Unity个人练手项目，第三人称动作射击游戏，参考Unity官方第一人称射击游戏模板项目实现，更改模型、地图，将控制系统更改为使用Cinemachine Camera和Input System的第三人称控制，添加第三人称模型，并使用Animator Controller构建相应的动画状态机，本身包含敌人行为状态机、Nav Agent路径导航、武器及子弹控制系统、伤害血量及血量的全局显示、简单的游戏流程消息系统等。

非常简陋，尚未完善，本身使用Plastic作为版本控制，GitHub仅作备份及公开展示，可能不会非常频繁更新，如果有任何问题或建议，可以提Issue或Discussion，欢迎讨论。

## 关于 xLua 和热更新

点击“Generate Gameobject”按钮会在场景中生成正方体，在点击“HotReload"后再次点击会生成球形物体，想要使用功能请保证有本地或远程服务器，且服务器中有对应的热更AB包，并在xLuaHotLoad.cs中更改对应的服务器资源地址。

## 目前的开发计划

- [x] 解决子弹系统异常延迟bug
- [x] 完善玩家模型动画
- [x] 使用Lua重写或添加部分逻辑
- [x] 完善枪械上下瞄准
- [x] 修复射击时子弹生成位置问题
- [x] 添加敌人攻击
- [x] 添加拾取血包
- [ ] 添加锁定系统
- [x] 添加子弹碰撞特效
- [x] 背包系统
- [ ] 网络系统

## 已知问题

- 人物在冲刺时持枪动画错误
- 人物在斜向前进时动画错误
- 枪械子弹似乎还有一些问题

------

# ATPS

Personal practice unity project, a third person action shoot game, based on unity official fps template project, change the official project's control system to Cinema chine Camera & Input System third person system, add an third person player model, use Animator Controller to add some animation, the project include Enemy state machine, Nav Agent, Gun System, Damage & Health System, Enemy health display and a simple game flow event system.

Very WIP, and I use plastic for version control, GitHub just for demonstrating, so may not update frequently, if you have any question or suggestion, you can make an issue or discussion, questions/suggestions/discussions are welcome.

## About xLua and hotfix

Hit "Generate Gameobject" button will generate a cube in scene, if you hit "HotReload" button and then hit "Generate Gameobject" button again, it will generate a sphere in scene. if you want to test it, you must have a local or remote hosting having the hotfix assetbundle, and change the resource path in xLuaHotLoad.cs according to its URL.

## Todo List

- [x] Fix the weird delay when shooting
- [x] Improve player model animation
- [x] Add some lua for practice
- [x] Improve weapon vertical control
- [x] Fix the projectile position bug when shooting
- [x] Add enemy attack
- [x] Add pickup Health
- [ ] Add lock on system
- [x] Add projectile hit VFX
- [x] Inventory
- [ ] Network

## Known Issues

- Animation bug when sprint
- Animation bug when walk/run forward left/right
- Some bug about projectile
