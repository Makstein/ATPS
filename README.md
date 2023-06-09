# ATPS

Unity个人练手项目，第三人称动作射击游戏，参考Unity官方第一人称射击游戏模板项目实现，更改模型、地图，将控制系统更改为使用Cinemachine Camera和Input System的第三人称控制，添加第三人称模型，并使用Animator Controller构建相应的动画状态机，本身包含敌人行为状态机、Nav Agent路径导航、武器及子弹控制系统、伤害血量及血量的全局显示、简单的游戏流程消息系统等。

非常简陋，尚未完善，本身使用Plastic作为版本控制，GitHub仅作备份及公开展示，可能不会非常频繁更新

------

目前的开发计划：

- [x] 解决子弹系统异常延迟bug
- [ ] 完善玩家模型动画
- [ ] 使用Lua重写或添加部分逻辑
- [ ] 添加锁定系统

# ATPS

Personal practice unity project, a third person action shoot game, based on unity official fps template project, change the official project's control system to Cinemachine Caera & Input System third person system, add an third person player model, use Animator Controller to add some animation, the project include Enemy state machine, Nav Agent, Gun System, Damage & Health System, Enemy health dispaly and a simple gameflow event system.

Very WIP, and I use plastic for version control, GitHub just for demonstrating, so may not update frequently.

------

Todo List:

- [x] Fix the werid delay when shooting
- [ ] Improve player model animation
- [ ] Add some lua for practice
- [ ] Add lock on system

---

# 更新记录/Update History

## 2023/06/09

为角色添加了阳光且类人的持枪奔跑、左右侧移、不变向后退动画，使用Animation Rigging做了手部IK，添加了新的动画状态机，同时由于动画修改尚未完善，角色目前仅有上述几种动画，且由于设计问题，目前角色开枪会有问题，动画太难了，三周全做这玩意了，还没做完，动画状态机部分有参考@[lhcmt/Unity3D-TPS: 这是之前第三人称射击游戏项目的网络版本 (github.com)](https://github.com/lhcmt/Unity3D-TPS)，感谢大佬



Add run with gun, left and right strafe, jog back animations to player, use Aniamtion Rigging make hand IK, a new animator controller, and because I have not finished animation works, the player now ONLY have these aniamtions, and because my code logic, these unfinished animations will cause problems while shooting. Animation is too hard for me, I just start learning it three weeks ago, and spent almost all spare time doing it in past three weeks, and not finish it yet :(

Thanks for @[lhcmt/Unity3D-TPS: 这是之前第三人称射击游戏项目的网络版本 (github.com)](https://github.com/lhcmt/Unity3D-TPS) Animation Controller




