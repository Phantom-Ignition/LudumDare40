﻿using LudumDare40.Components.Battle.Enemies;
using Nez;

namespace LudumDare40.NPCs
{
    class BossBattleScript : NpcBase
    {
        private Entity bossEntity;
        private Entity playerEntity;

        public BossBattleScript(string name) : base(name)
        {
            Invisible = true;
        }

        public override void onAddedToEntity()
        {
            bossEntity = entity.scene.findEntity("boss");
            playerEntity = entity.scene.findEntity("player");
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            cinematicIn(30, 1);
            playerMessage("W-what's that?!");
            closePlayerMessage();
            wait(0.5f);
            focusCamera(bossEntity);
            wait(2f);
            var boss = bossEntity.getComponent<EnemyBossComponent>();
            executeAction(boss.wakeUp);
            wait(3f);
            focusCamera(playerEntity);
            playerMessage("Shit.");
            closePlayerMessage();
            executeAction(() =>
            {
                boss.canStartTheAttacks = true;
            });
            cinematicOut(0, 1);
        }
    }
}
