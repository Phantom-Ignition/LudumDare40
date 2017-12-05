using System.Collections.Generic;
using LudumDare40.Components;
using LudumDare40.Components.Player;
using LudumDare40.Managers;
using LudumDare40.NPCs;
using Nez;

namespace LudumDare40.Systems
{
    class NpcInteractionSystem : EntityProcessingSystem
    {
        private PlayerComponent _player;
        private InteractionCollider _interactionCollider;

        public List<NpcBase> _autorunNpcs;

        public bool IsBusy { get; set; }

        public NpcInteractionSystem(PlayerComponent player) : base(Matcher.empty())
        {
            _player = player;
            _interactionCollider = _player.getComponent<InteractionCollider>();
            _autorunNpcs = new List<NpcBase>();
        }

        public void addAutorun(NpcBase npc)
        {
            _autorunNpcs.Add(npc);
        }

        public void executeNpc(NpcBase npc)
        {
            executeActionList(npc, false);
        }

        public void mapStart()
        {
            foreach (var npc in _autorunNpcs)
            {
                executeActionList(npc, false);
            }
            _autorunNpcs.Clear();
        }

        public override void onChange(Entity entity)
        {
            var contains = _entities.Contains(entity);
            var interest = entity.getComponent<NpcBase>() != null;

            if (interest && !contains)
            {
                add(entity);
            }
            else if (!interest && contains)
            {
                remove(entity);
            }
        }

        protected override void process(List<Entity> entities)
        {
            var inputManager = Core.getGlobalManager<InputManager>();
            if (!inputManager.IsBusy)
            {
                base.process(entities);
            }
        }

        public override void process(Entity entity)
        {
        }

        private void executeActionList(NpcBase npc, bool turnToPlayer)
        {
            Core.getGlobalManager<InputManager>().IsBusy = true;
            npc.executeActionList(turnToPlayer);
        }
    }
}
