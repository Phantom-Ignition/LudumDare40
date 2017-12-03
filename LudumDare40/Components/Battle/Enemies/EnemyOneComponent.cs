using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Components.Player;
using LudumDare40.Components.Sprites;
using LudumDare40.FSM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace LudumDare40.Components.Battle.Enemies
{
    public class EnemyOneComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<EnemyOneState, EnemyOneComponent> _fsm;
        public FiniteStateMachine<EnemyOneState, EnemyOneComponent> FSM => _fsm;
        
        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.enemyOne);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 67, 62),
                new Rectangle(67, 0, 67, 62),
                new Rectangle(134, 0, 67, 62),
                new Rectangle(201, 0, 67, 62),
                new Rectangle(268, 0, 67, 62),
                new Rectangle(335, 0, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });
            
            sprite.CreateAnimation("walking", 0.15f);
            sprite.AddFrames("walking", new List<Rectangle>
            {
                new Rectangle(335, 124, 67, 62),
                new Rectangle(402, 124, 67, 62),
                new Rectangle(0, 186, 67, 62),
                new Rectangle(67, 186, 67, 62),
                new Rectangle(134, 186, 67, 62),
                new Rectangle(201, 186, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });

            sprite.CreateAnimation("punch", 0.25f);
            sprite.AddFrames("punch", new List<Rectangle>
            {
                new Rectangle(402, 0, 67, 62),
                new Rectangle(0, 32, 67, 62),
                new Rectangle(67, 32, 67, 62),
                new Rectangle(134, 32, 67, 62),
                new Rectangle(201, 32, 67, 62),
                new Rectangle(268, 32, 67, 62),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });

            sprite.CreateAnimation("dying", 0.2f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(268, 186, 64, 64),
                new Rectangle(335, 186, 64, 64),
            }, new[] { 0, 0, 0, 0, 0, 0 }, new[] { -4, -4, -4, -4, -4, -4 });

            // FSM
            _fsm = new FiniteStateMachine<EnemyOneState, EnemyOneComponent>(this, new EnemyOnePatrolState());
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 60;
            platformerObject.moveSpeed = 60;
        }

        public override void update()
        {
            _fsm.update();
            base.update();
        }
    }
}
