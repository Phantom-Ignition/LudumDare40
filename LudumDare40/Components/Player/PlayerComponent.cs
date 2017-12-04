using LudumDare40.Components.Battle;
using LudumDare40.Components.Map;
using LudumDare40.Components.Sprites;
using LudumDare40.Extensions;
using LudumDare40.FSM;
using LudumDare40.Managers;
using LudumDare40.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tiled;
using System;
using System.Collections.Generic;

namespace LudumDare40.Components.Player
{
    public class PlayerComponent : Component, IUpdatable, IBattleEntity
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            JumpPreparation,
            JumpUpwards,
            JumpFalling,
            JumpLanding,
            AttackOne,
            AttackTwo,
            Hit,
            Rolling,
            Dying,
            SlidingWall
        }

        private Dictionary<Animations, string> _animationMap;

        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;
        public AnimatedSprite coreSprite;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState CollisionState => _platformerObject.collisionState;
        public bool ForcedGround { get; set; }

        //--------------------------------------------------
        // Velocity

        public Vector2 Velocity => _platformerObject.velocity;

        //--------------------------------------------------
        // Footstep sound cooldown

        private float _footstepCooldown;
        //--------------------------------------------------
        // HP

        public const int MAX_HP = 10;

        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<PlayerState, PlayerComponent> _fsm;
        public FiniteStateMachine<PlayerState, PlayerComponent> FSM => _fsm;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;
        private bool _walljumpForcedMovement;

        //--------------------------------------------------
        // Knockback

        private Vector2 _knockbackVelocity;
        private Vector2 _knockbackTick;

        //--------------------------------------------------
        // Effects

        private Texture2D _wallSlideEffectTexture;
        private Texture2D _jumpEffectTexture;
        private Texture2D _rollEffectTexture;
        private int _effectsCount;

        //--------------------------------------------------
        // Battle Component

        public BattleComponent battleComponent;

        //--------------------------------------------------
        // Rolling

        public bool isRolling;

        //--------------------------------------------------
        // Player Manager

        public PlayerManager playerManager;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => true;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.player);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.JumpPreparation, "jumpPreparation"},
                {Animations.JumpUpwards, "jumpUpwards"},
                {Animations.JumpFalling, "jumpFalling"},
                {Animations.JumpLanding, "jumpLanding"},
                {Animations.AttackOne, "attack1"},
                {Animations.AttackTwo, "attack2"},
                {Animations.SlidingWall, "slidingWall"},
                {Animations.Rolling, "rolling"},
                {Animations.Hit, "jumpLanding"},
                {Animations.Dying, "dying"},
            };

            var am = _animationMap;

            sprite = entity.addComponent(new AnimatedSprite(texture, am[Animations.Stand]));
            sprite.CreateAnimation(am[Animations.Stand], 0.1f);
            sprite.AddFrames(am[Animations.Stand], new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 64),
                new Rectangle(64, 0, 64, 64),
                new Rectangle(128, 0, 64, 64),
                new Rectangle(192, 0, 64, 64),
                new Rectangle(256, 0, 64, 64),
                new Rectangle(320, 0, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.Walking], 0.1f);
            sprite.AddFrames(am[Animations.Walking], new List<Rectangle>()
            {
                new Rectangle(0, 64, 64, 64),
                new Rectangle(64, 64, 64, 64),
                new Rectangle(128, 64, 64, 64),
                new Rectangle(192, 64, 64, 64),
                new Rectangle(256, 64, 64, 64),
                new Rectangle(320, 64, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.JumpPreparation]);
            sprite.AddFrames(am[Animations.JumpPreparation], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpUpwards]);
            sprite.AddFrames(am[Animations.JumpUpwards], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpFalling]);
            sprite.AddFrames(am[Animations.JumpFalling], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.JumpLanding]);
            sprite.AddFrames(am[Animations.JumpLanding], new List<Rectangle>()
            {
                new Rectangle(0, 128, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.AttackOne], 0.08f, false);
            sprite.AddFrames(am[Animations.AttackOne], new List<Rectangle>
            {
                new Rectangle(128, 128, 64, 64),
                new Rectangle(192, 128, 64, 64),
                new Rectangle(256, 128, 64, 64),
                new Rectangle(320, 128, 64, 64),
            }, new [] { 0, 0, 0, 0 }, new[] { -9, -9, -9, -9 });
            sprite.AddAttackCollider(am[Animations.AttackOne], new List<List<Rectangle>>
            {
                new List<Rectangle>() { },
                new List<Rectangle>() { new Rectangle(0, -10, 34, 29) },
                new List<Rectangle>() { new Rectangle(4, -9, 29, 25) },
                new List<Rectangle>() { new Rectangle(5, -10, 16, 11) },
            });
            sprite.AddFramesToAttack(am[Animations.AttackOne], 1, 2, 3);
            
            sprite.CreateAnimation(am[Animations.AttackTwo], 0.08f, false);
            sprite.AddFrames(am[Animations.AttackTwo], new List<Rectangle>
            {
                new Rectangle(0, 192, 64, 64),
                new Rectangle(64, 192, 64, 64),
                new Rectangle(128, 192, 64, 64),
                new Rectangle(192, 192, 64, 64),
            }, new int[] { 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9 });
            sprite.AddAttackCollider(am[Animations.AttackTwo], new List<List<Rectangle>>
            {
                new List<Rectangle>() { new Rectangle(5, -10, 16, 11) },
                new List<Rectangle>() { new Rectangle(-20, -8, 55, 24) },
                new List<Rectangle>() { new Rectangle(-22, -14, 45, 28) },
                new List<Rectangle>() { new Rectangle(-22, -14, 22, 20) },
            });
            sprite.AddFramesToAttack(am[Animations.AttackTwo], 0, 1, 2, 3);

            sprite.CreateAnimation(am[Animations.SlidingWall], 0.1f, false);
            sprite.AddFrames(am[Animations.SlidingWall], new List<Rectangle>()
            {
                new Rectangle(320, 192, 64, 64),
            }, new int[] { 0 }, new int[] { -9 });

            sprite.CreateAnimation(am[Animations.Rolling], 0.05f, false);
            sprite.AddFrames(am[Animations.Rolling], new List<Rectangle>()
            {
                new Rectangle(256, 256, 64, 64),
                new Rectangle(320, 256, 64, 64),
                new Rectangle(0, 320, 64, 64),
                new Rectangle(64, 320, 64, 64),
                new Rectangle(128, 320, 64, 64),
                new Rectangle(192, 320, 64, 64),
                new Rectangle(256, 320, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.Hit], 0.1f, false);
            sprite.AddFrames(am[Animations.Hit], new List<Rectangle>()
            {
                new Rectangle(0, 256, 64, 64),
                new Rectangle(64, 256, 64, 64),
                new Rectangle(128, 256, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9 });

            sprite.CreateAnimation(am[Animations.Dying], 0.2f, false);
            sprite.AddFrames(am[Animations.Dying], new List<Rectangle>()
            {
                new Rectangle(0, 384, 64, 64),
                new Rectangle(64, 384, 64, 64),
                new Rectangle(128, 384, 64, 64),
                new Rectangle(192, 384, 64, 64),
                new Rectangle(256, 384, 64, 64),
                new Rectangle(320, 384, 64, 64),
                new Rectangle(0, 448, 64, 64),
                new Rectangle(64, 448, 64, 64),
                new Rectangle(128, 448, 64, 64),
                new Rectangle(192, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
                new Rectangle(256, 448, 64, 64),
            }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new int[] { -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9, -9 });

            // Create the core sprite
            var coreTexture = entity.scene.content.Load<Texture2D>(Content.Characters.core);
            coreSprite = entity.addComponent(new AnimatedSprite(coreTexture, am[Animations.Stand]));
            coreSprite.CloneAnimationsFrom(sprite, coreTexture);
            coreSprite.setEnabled(false);

            // init fsm
            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());

            // init effects
            _wallSlideEffectTexture = entity.scene.content.Load<Texture2D>(Content.Effects.wallSlideEffect);
            _jumpEffectTexture = entity.scene.content.Load<Texture2D>(Content.Effects.jumpEffect);
            _rollEffectTexture = entity.scene.content.Load<Texture2D>(Content.Effects.rollEffect);

            // Set player manager
            playerManager = Core.getGlobalManager<PlayerManager>();
            //playerManager.HoldingCore = true;
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();
            battleComponent = entity.getComponent<BattleComponent>();
            battleComponent.setHp(MAX_HP);
            battleComponent.battleEntity = this;
            battleComponent.ImmunityDuration = 0.5f;
            battleComponent.destroyEntityAction = destroyEntity;

            entity.setTag(SceneMap.PLAYER);
        }

        public void onHit(Vector2 knockback)
        {
            (entity.scene as SceneMap)?.startScreenShake(1, 200);
            _knockbackTick = new Vector2(0.06f, 0.04f);
            _knockbackVelocity = new Vector2(knockback.X * 60, -5);
            FSM.changeState(new HitState());
        }
        
        public void onDeath()
        {
            FSM.changeState(new DyingState());
        }

        public void destroyEntity()
        {
            entity.setEnabled(false);
            Core.startSceneTransition(new SquaresTransition(() => new SceneMap()));
        }

        public void forceMovement(Vector2 velocity, bool walljumpForcedMovement = false)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
            _walljumpForcedMovement = walljumpForcedMovement;
        }

        public void takeThrow()
        {
            var collider = entity.getComponent<BoxCollider>();

            if (playerManager.HoldingCore)
            {
                var reactors = entity.scene.findEntitiesWithTag(SceneMap.REACTORS);
                foreach (var reactor in reactors)
                {
                    var reactorCollider = reactor.getComponent<BoxCollider>();
                    CollisionResult collisionResult;
                    if (collider.collidesWith(reactorCollider, out collisionResult))
                    {
                        var reactorComponent = reactor.getComponent<ReactorComponent>();
                        reactorComponent.setActivated();
                        playerManager.HoldingCore = false;
                        return;
                    }
                }
                (entity.scene as SceneMap)?.createCoreDrop(transform.position);
                playerManager.HoldingCore = false;
                return;
            }

            var coreDrops = entity.scene.findEntitiesWithTag(SceneMap.COREDROPS);
            foreach (var coreDrop in coreDrops)
            {
                var reactorCollider = coreDrop.getComponent<BoxCollider>();
                CollisionResult collisionResult;
                if (collider.collidesWith(reactorCollider, out collisionResult))
                {
                    coreDrop.destroy();
                    playerManager.HoldingCore = true;
                    return;
                }
            }
        }

        public void update()
        {
            // Update FSM
            _fsm.update();

            // apply knockback before movement
            if (applyKnockback())
                return;

            var axis = Core.getGlobalManager<InputManager>().MovementAxis.value;
            var velocity = _forceMovement ? _forceMovementVelocity.X : axis;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                if (isOnGround() && axis != 0 && _footstepCooldown <= 0.0f)
                {
                    _footstepCooldown = 0.25f;
                    AudioManager.footstep.Play(0.4f);
                }
                else
                {
                    _footstepCooldown -= Time.deltaTime;
                }
                    

                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = _walljumpForcedMovement ? po.gravity * mms : po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }

                if (isRolling)
                {
                    moveSpeed *= 1.5f;
                    mms *= 1.5f;
                }
                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.deltaTime, -mms, mms);

                if (platformerObject.grabbingWall)
                {
                    po.velocity.X = po.grabbingWallSide * mms;
                    sprite.spriteEffects = po.grabbingWallSide == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;
                }
                else
                {
                    sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                }
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }

            // Match the core sprite with the default one
            coreSprite.spriteEffects = sprite.spriteEffects;
            coreSprite.setEnabled(playerManager.HoldingCore);

            ForcedGround = false;
        }

        private bool applyKnockback()
        {
            if (_knockbackTick.X > 0)
            {
                _knockbackTick.X -= Time.deltaTime;
            }
            if (_knockbackTick.Y > 0)
            {
                _knockbackTick.Y -= Time.deltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed;
            var velx = _platformerObject.velocity.X;
            var vely = _platformerObject.velocity.Y;
            bool appliedKb = false;
            if (_knockbackTick.X > 0)
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + _platformerObject.moveSpeed * _knockbackVelocity.X * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            if (_knockbackTick.Y > 0)
            {
                _platformerObject.velocity.Y = MathHelper.Clamp(vely + _platformerObject.moveSpeed * _knockbackVelocity.Y * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            return appliedKb;
        }

        public void createJumpEffect()
        {
            var effect = entity.scene.createEntity($"effect:playerComponent({getEffectCount()})");
            var effectSprite = effect.addComponent(new AnimatedSprite(_jumpEffectTexture, "default"));
            effectSprite.CreateAnimation("default", 0.08f, false);
            effectSprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 17, 10),
                new Rectangle(17, 0, 17, 10),
                new Rectangle(34, 0, 17, 10),
            });
            effectSprite.spriteEffects = sprite.spriteEffects;
            var collider = entity.getComponent<BoxCollider>();
            effect.position = new Vector2(collider.bounds.center.X, collider.bounds.bottom - effectSprite.height / 2);
            effect.addComponent<SpriteEffectComponent>();
        }

        public void createRollEffect()
        {
            var effect = entity.scene.createEntity($"effect:playerComponent({getEffectCount()})");
            var effectSprite = effect.addComponent(new AnimatedSprite(_rollEffectTexture, "default"));
            effectSprite.CreateAnimation("default", 0.08f, false);
            effectSprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 17, 10),
                new Rectangle(17, 0, 17, 10),
                new Rectangle(34, 0, 17, 10),
            });
            effectSprite.spriteEffects = sprite.spriteEffects;
            var collider = entity.getComponent<BoxCollider>();
            effect.position = new Vector2(collider.bounds.center.X, collider.bounds.bottom - effectSprite.height / 2);
            effect.position += effectSprite.getDirection() * -7 * Vector2.UnitX;
            effect.addComponent<SpriteEffectComponent>();
        }

        public void createWallSlideEffect()
        {
            var effect = entity.scene.createEntity($"effect:playerComponent({getEffectCount()})");
            var effectSprite = effect.addComponent(new AnimatedSprite(_wallSlideEffectTexture, "default"));
            effectSprite.CreateAnimation("default", 0.08f, false);
            effectSprite.AddFrames("default", new List<Rectangle>()
            {
                new Rectangle(0, 0, 10, 17),
                new Rectangle(10, 0, 10, 17),
                new Rectangle(20, 0, 10, 17),
            });
            effectSprite.spriteEffects = sprite.spriteEffects;
            var collider = entity.getComponent<BoxCollider>();
            effect.position = new Vector2(collider.bounds.center.X, collider.bounds.top + effectSprite.height / 2 - 4);
            effect.position += effectSprite.getDirection() *(( collider.bounds.width / 2) - 3) * Vector2.UnitX;
            effect.addComponent<SpriteEffectComponent>();
        }

        private int getEffectCount()
        {
            _effectsCount++;
            _effectsCount %= 100;
            return _effectsCount;
        }

        private bool canMove()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable() || _forceMovement;
        }

        public bool isOnGround()
        {
            return ForcedGround || _platformerObject.collisionState.below;
        }

        public void SetAnimation(Animations animation)
        {
            var animationStr = _animationMap[animation];
            if (sprite.CurrentAnimation != animationStr)
            {
                sprite.play(animationStr);
                coreSprite.play(sprite.CurrentAnimation);
            }
        }

        public void Jump()
        {
            _platformerObject.jump();
        }

        public float hpRate()
        {
            return battleComponent.HP / MAX_HP;
        }
    }
}
