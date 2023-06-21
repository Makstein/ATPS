# ![](https://github.com/Makstein/ATPS/blob/master/Images/AnimationDemo.gif)

# ATPS

Unity个人练手项目，第三人称动作射击游戏，参考Unity官方第一人称射击游戏模板项目实现，更改模型、地图，将控制系统更改为使用Cinemachine Camera和Input System的第三人称控制，添加第三人称模型，并使用Animator Controller构建相应的动画状态机，本身包含敌人行为状态机、Nav Agent路径导航、武器及子弹控制系统、伤害血量及血量的全局显示、简单的游戏流程消息系统等。

非常简陋，尚未完善，本身使用Plastic作为版本控制，GitHub仅作备份及公开展示，可能不会非常频繁更新

## 关于 xLua 和热更新

点击“Generate Gameobject”按钮会在场景中生成正方体，在点击“HotReload"后再次点击会生成球形物体，想要使用功能请保证有本地或远程服务器，且服务器中有对应的热更AB包，并在xLuaHotLoad.cs中更改对应的服务器资源地址

------

目前的开发计划：

- [x] 解决子弹系统异常延迟bug
- [x] 完善玩家模型动画
- [x] 使用Lua重写或添加部分逻辑
- [x] 完善枪械上下瞄准
- [x] 修复射击时子弹生成位置问题
- [ ] 添加敌人攻击
- [ ] 添加拾取血包
- [ ] 添加锁定系统

# ATPS

Personal practice unity project, a third person action shoot game, based on unity official fps template project, change the official project's control system to Cinemachine Caera & Input System third person system, add an third person player model, use Animator Controller to add some animation, the project include Enemy state machine, Nav Agent, Gun System, Damage & Health System, Enemy health dispaly and a simple gameflow event system.

Very WIP, and I use plastic for version control, GitHub just for demonstrating, so may not update frequently.

## About xlua and hotfix

Hit "Generate Gameobject" button will generate a cube in scene, if you hit "HotReload" button and then hit "Generate Gameobject" button again, it will generate a sphere in scene. if you want to test it, you must have a local or remote hosting having the hotfix assetbundle, and change the resource path in xLuaHotLoad.cs according to its url.

------

Todo List:

- [x] Fix the werid delay when shooting
- [x] Improve player model animation
- [x] Add some lua for practice
- [x] Improve weapon vertical control
- [x] Fix the projectile position bug when shooting
- [ ] Add enemy attack
- [ ] Add pickup Health
- [ ] Add lock on system
