using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LudumDare40.Components;
using LudumDare40.Components.Battle;
using LudumDare40.Components.Battle.Enemies;
using LudumDare40.Components.Map;
using LudumDare40.Components.Player;
using LudumDare40.Components.Sprites;
using LudumDare40.Components.Windows;
using LudumDare40.Extensions;
using LudumDare40.Managers;
using LudumDare40.NPCs;
using LudumDare40.PostProcessors;
using LudumDare40.Scenes.SceneMapExtensions;
using LudumDare40.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Particles;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;

namespace LudumDare40.Scenes
{
    class SceneMap : Scene
    {
        //--------------------------------------------------
        // Render Layers Constants

        public const int BACKGROUND_RENDER_LAYER = 10;
        public const int TILED_MAP_RENDER_LAYER = 9;
        public const int WATER_RENDER_LAYER = 6;
        public const int MISC_RENDER_LAYER = 5; // NPCs, Text, etc.
        public const int ENEMIES_RENDER_LAYER = 4; // NPCs, Text, etc.
        public const int PLAYER_RENDER_LAYER = 3;
        public const int PARTICLES_RENDER_LAYER = 2;
        public const int HUD_BACK_RENDER_LAYER = 1;
        public const int HUD_FILL_RENDER_LAYER = 0;

        //--------------------------------------------------
        // Scene tags

        public const int REACTORS = 1;
        public const int COREDROPS = 2;
        public const int ENEMIES = 3;
        public const int SHOTS = 4;
        public const int PLAYER = 5;

        //--------------------------------------------------
        // Post Processors

        private CinematicLetterboxPostProcessor _cinematicPostProcessor;

        //--------------------------------------------------
        // Map
        
        private TiledMap _tiledMap;
        private Sprite _backgroundSprite;

        //--------------------------------------------------
        // Camera

        private CameraSystem _camera;

        //--------------------------------------------------
        // Reactors

        private List<Entity> _reactors;

        //--------------------------------------------------
        // Boss battle active

        private bool _bossBattleActive;

        //--------------------------------------------------
        // Entities

        public Entity playerEntity;
        public Entity bossEntity;

        //--------------------------------------------------
        // Scripts

        private NpcBase _bossBattleScript;

        //--------------------------------------------------
        // HUD

        private Sprite _playerHudFillSprite;
        private Sprite _bossHudFillSprite;

        //--------------------------------------------------
        // Ambience

        private SoundEffectInstance _ambienceEffectInstance;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(58, 61, 101);
            setupMap();
            setupPlayer();
            setupEnemies();
            setupReactors();
            setupCoreDrops();
            setupNpcs();
            setupParticles();
            setupLadders();
            setupWater();
            setupHud();
            setupPostProcessors();
            setupScripts();
        }

        public override void onStart()
        {
            setupEntityProcessors();

            getEntityProcessor<NpcInteractionSystem>().mapStart();
            _ambienceEffectInstance = AudioManager.ambience.CreateInstance();
            _ambienceEffectInstance.IsLooped = true;

            MediaPlayer.Stop();
            _ambienceEffectInstance.Play();
        }

        private TiledMapComponent tiledMapComponent;

        private void setupMap()
        {
            var background = createEntity("map-background");
            var texture = content.Load<Texture2D>(Content.Maps.background);
            _backgroundSprite = background.addComponent(new Sprite(texture) { renderLayer = BACKGROUND_RENDER_LAYER});
            background.position = (virtualSize.ToVector2() / 2);

            var sysManager = Core.getGlobalManager<SystemManager>();
            _tiledMap = content.Load<TiledMap>("maps/map");
            sysManager.setTiledMapComponent(_tiledMap);

            var tiledEntity = createEntity("tiled-map");
            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var defaultLayers = _tiledMap.properties["defaultLayers"].Split(',').Select(s => s.Trim()).ToArray();

            tiledMapComponent = tiledEntity.addComponent(new TiledMapComponent(_tiledMap, collisionLayer) { renderLayer = TILED_MAP_RENDER_LAYER });
            tiledMapComponent.setLayersToRender(defaultLayers);
            
            if (_tiledMap.properties.ContainsKey("aboveWaterLayer"))
            {
                var aboveWaterLayer = _tiledMap.properties["aboveWaterLayer"];
                var tiledAboveWater = tiledEntity.addComponent(new TiledMapComponent(_tiledMap) { renderLayer = WATER_RENDER_LAYER });
                tiledAboveWater.setLayerToRender(aboveWaterLayer);
            }
        }

        private void setupPlayer()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            Vector2? playerSpawn;

            if (sysManager.SpawnPosition.HasValue)
            {
                playerSpawn = sysManager.SpawnPosition;
            }
            else
            {
                playerSpawn = _tiledMap.getObjectGroup("objects").objectWithName("playerSpawn").position;
            }

            var player = createEntity("player");
            player.transform.position = playerSpawn.Value;
            player.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            player.addComponent(new BoxCollider(-5f, -10f, 11f, 30f));
            player.addComponent<PlatformerObject>();
            player.addComponent<TextWindowComponent>();
            player.addComponent<BattleComponent>();
            var playerComponent = player.addComponent<PlayerComponent>();
            playerComponent.sprite.renderLayer = PLAYER_RENDER_LAYER;
            playerComponent.coreSprite.renderLayer = MISC_RENDER_LAYER;

            Core.getGlobalManager<SystemManager>().setPlayer(player);
            playerEntity = player;
        }

        private void setupEnemies()
        {
            var collisionLayer = _tiledMap.properties["collisionLayer"];

            var enemiesGroup = _tiledMap.getObjectGroup("enemies");
            if (enemiesGroup == null) return;
            foreach (var enemy in enemiesGroup.objects)
            {
                var name = findEntitiesWithTag(ENEMIES).Count;
                var entity = createEntity($"enemy:{name}");
                entity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
                entity.addComponent<PlatformerObject>();
                entity.addComponent<BattleComponent>();
                entity.addComponent(new BoxCollider(-10f, -15f, 20f, 35f));

                var patrolStartRight = bool.Parse(enemy.properties.ContainsKey("patrolStartRight")
                    ? enemy.properties["patrolStartRight"]
                    : "false");
                var instance = createEnemyInstance(enemy.type, patrolStartRight);
                var enemyComponent = entity.addComponent(instance);
                enemyComponent.sprite.renderLayer = ENEMIES_RENDER_LAYER;
                enemyComponent.playerCollider = findEntity("player").getComponent<BoxCollider>();
                enemyComponent.patrolTime = float.Parse(enemy.properties.ContainsKey("patrolTime")
                    ? enemy.properties["patrolTime"]
                    : "0");

                entity.transform.position = enemy.position + new Vector2(enemy.width, enemy.height) / 2;

                if (enemy.type == "EnemyBoss")
                {
                    entity.name = "boss";
                    bossEntity = entity;
                }
            }
        }

        private EnemyComponent createEnemyInstance(string enemyName, bool patrolStartRight)
        {
            var enemiesNamespace = typeof(BattleComponent).Namespace + ".Enemies";
            var type = Type.GetType(enemiesNamespace + "." + enemyName + "Component");
            if (type != null)
            {
                return Activator.CreateInstance(type, new object[] { patrolStartRight }) as EnemyComponent;
            }
            return null;
        }

        private void setupReactors()
        {
            _reactors = new List<Entity>();
            var reactorsGroup = _tiledMap.getObjectGroup("reactors");
            if (reactorsGroup == null) return;
            foreach (var core in reactorsGroup.objects)
            {
                var entity = createEntity(core.name);
                entity.transform.position = core.position + new Vector2(core.width, core.height) / 2.0f;
                entity.addComponent<ReactorComponent>();
                entity.addComponent(new BoxCollider(32, 32));
                _reactors.Add(entity);
            }
        }

        private void setupCoreDrops()
        {
            var coreDropsGroup = _tiledMap.getObjectGroup("coreDrops");
            if (coreDropsGroup == null) return;
            foreach (var coreDrop in coreDropsGroup.objects)
            {
                createCoreDrop(coreDrop.position + new Vector2(coreDrop.width, coreDrop.height) / 2.0f);
            }
        }

        public void createCoreDrop(Vector2 position)
        {
            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var texture = content.Load<Texture2D>(Content.Misc.coreDrop);
            var name = findEntitiesWithTag(COREDROPS).Count;
            var entity = createEntity($"coredrop:{name}");
            entity.addComponent(new Sprite(texture) { renderLayer = MISC_RENDER_LAYER });
            entity.addComponent(new BoxCollider(16, 16));
            entity.addComponent<CoreDropComponent>();
            entity.addComponent<PlatformerObject>();
            entity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));
            entity.transform.position = position;
        }

        private void setupNpcs()
        {
            var npcObjects = _tiledMap.getObjectGroup("npcs");
            if (npcObjects == null) return;

            var collisionLayer = _tiledMap.properties["collisionLayer"];
            var names = new Dictionary<string, int>();
            foreach (var npc in npcObjects.objects)
            {
                names[npc.name] = names.ContainsKey(npc.name) ? ++names[npc.name] : 0;

                var npcEntity = createEntity(string.Format("{0}:{1}", npc.name, names[npc.name]));
                var npcComponent = (NpcBase)Activator.CreateInstance(Type.GetType("AdvJam2017.NPCs." + npc.type), npc.name);
                npcComponent.setRenderLayer(MISC_RENDER_LAYER);
                npcComponent.ObjectRect = new Rectangle(0, 0, npc.width, npc.height);
                npcEntity.addComponent(npcComponent);
                npcEntity.addComponent<TextWindowComponent>();
                npcEntity.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>(collisionLayer)));

                if (!npcComponent.Invisible) {
                    npcEntity.position = npc.position + new Vector2(npc.width, npc.height) / 2;
                    npcEntity.addComponent<PlatformerObject>();
                }

                // Props
                if (npc.properties.ContainsKey("flipX") && npc.properties["flipX"] == "true")
                {
                    npcComponent.FlipX = true;
                }
                if (npc.properties.ContainsKey("autorun") && npc.properties["autorun"] == "true")
                {
                    getEntityProcessor<NpcInteractionSystem>().addAutorun(npcComponent);
                }
            }
        }

        private void setupParticles()
        {
            var particles = _tiledMap.getObjectGroup("particles");
            if (particles == null) return;

            var names = new Dictionary<string, int>();
            foreach (var particleObj in particles.objects)
            {
                names[particleObj.name] = names.ContainsKey(particleObj.name) ? ++names[particleObj.name] : 0;

                var particle = createEntity(string.Format("{0}:{1}", particleObj.name, names[particleObj.name]));
                var emitterConf = content.Load<ParticleEmitterConfig>("particles/" + particleObj.type);
                var emitter = particle.addComponent(new ParticleEmitter(emitterConf));
                emitter.renderLayer = PARTICLES_RENDER_LAYER;
                particle.position = particleObj.position + new Vector2(particleObj.width, particleObj.height) / 2;
            }
        }

        private void setupLadders()
        {
            var ladders = _tiledMap.getObjectGroup("ladders");
            if (ladders == null) return;

            var names = new Dictionary<string, int>();
            foreach (var ladderObj in ladders.objects)
            {
                names[ladderObj.name] = names.ContainsKey(ladderObj.name) ? ++names[ladderObj.name] : 0;

                var ladder = createEntity(string.Format("{0}:{1}", ladderObj.name, names[ladderObj.name]));
                ladder.addComponent(new LadderComponent(ladderObj));
            }
        }

        private NpcInteractionSystem _npcInteractionSystem;

        private void setupEntityProcessors()
        {
            var player = findEntity("player");
            var playerComponent = player.getComponent<PlayerComponent>();

            _npcInteractionSystem = new NpcInteractionSystem(playerComponent);

            camera.addComponent(new CameraShake());
            var mapSize = new Vector2(_tiledMap.width * _tiledMap.tileWidth, _tiledMap.height * _tiledMap.tileHeight);
            _camera = new CameraSystem(player)
            {
                mapLockEnabled = true,
                mapSize = mapSize,
                followLerp = 0.08f,
                deadzoneSize = new Vector2(20, 10)
            };
            addEntityProcessor(_camera);
            addEntityProcessor(_npcInteractionSystem);
            addEntityProcessor(new LadderSystem(new Matcher().all(typeof(LadderComponent)), playerComponent));
            addEntityProcessor(new BattleSystem());
            addEntityProcessor(new ShotsBattleSystem(player));
        }

        private void setupWater()
        {
            var environmentObjects = _tiledMap.getObjectGroup("environment");
            if (environmentObjects != null)
            {
                var waterObjects = environmentObjects.objects.Where(obj => obj.type == "water").ToList();
                for (var i = 0; i < waterObjects.Count; i++)
                {
                    var waterObj = waterObjects[i];
                    var entity = createEntity(string.Format("water:{0}", i));
                    var reflectionPlane = new WaterReflectionPlane(waterObj.width, waterObj.height) { renderLayer = WATER_RENDER_LAYER };
                    var material = reflectionPlane.getMaterial<WaterReflectionMaterial>();
                    material.effect.normalMap = content.Load<Texture2D>(Content.System.waterNormalMap);
                    material.effect.perspectiveCorrectionIntensity = 0.0f;
                    material.effect.normalMagnitude = 0.015f;
                    entity.addComponent(reflectionPlane);
                    entity.position = waterObj.position;
                }

                addRenderer(new RenderLayerRenderer(0, WATER_RENDER_LAYER));
            }
        }

        private Entity[] _hudEntities;
        private Vector2[] _hudPositions;
        private void setupHud()
        {
            _hudEntities = new Entity[4];
            _hudPositions = new Vector2[4];

            _hudEntities[0] = createEntity("playerHudBack");
            _hudEntities[0].addComponent(new Sprite(content.Load<Texture2D>(Content.Hud.player_hud)) { renderLayer = HUD_BACK_RENDER_LAYER })
                .setOriginNormalized(Vector2.Zero)
                .transform.localPosition = new Vector2(3, 5);
            _hudPositions[0] = new Vector2(6, 17);

            _hudEntities[1] = createEntity("playerHudFill");
            _hudEntities[1].addComponent(new Sprite(content.Load<Texture2D>(Content.Hud.player_hp)) { renderLayer = HUD_FILL_RENDER_LAYER })
                .setOriginNormalized(Vector2.Zero)
                .transform.localPosition = new Vector2(3, 5);
            _hudPositions[1] = new Vector2(6, 17);

            _hudEntities[2] = createEntity("bossHudBack");
            _hudEntities[2].addComponent(new Sprite(content.Load<Texture2D>(Content.Hud.boss_hud)) { renderLayer = HUD_BACK_RENDER_LAYER })
                .setOriginNormalized(Vector2.Zero)
                .transform.localPosition = new Vector2(144, 4);
            _hudPositions[2] = new Vector2(142, 17);

            _hudEntities[3] = createEntity("bossHudFill");
            _hudEntities[3].addComponent(new Sprite(content.Load<Texture2D>(Content.Hud.boss_hp)) { renderLayer = HUD_FILL_RENDER_LAYER })
                .setOriginNormalized(Vector2.Zero)
                .transform.localPosition = new Vector2(144, 4);
            _hudPositions[3] = new Vector2(142, 17);

            _playerHudFillSprite = findEntity("playerHudFill").getComponent<Sprite>();
            _bossHudFillSprite = findEntity("bossHudFill").getComponent<Sprite>();
        }

        private void setupPostProcessors()
        {
            Core.getGlobalManager<SystemManager>().cinematicLetterboxPostProcessor = addPostProcessor(new CinematicLetterboxPostProcessor(1));
            Core.getGlobalManager<SystemManager>().flashPostProcessor = addPostProcessor(new FlashPostProcessor(1));

            var bloom = addPostProcessor(new BloomPostProcessor(2));
            bloom.setBloomSettings(new BloomSettings(0.5f, 2, 0.9f, 1, 1, 1));

            var scanlines = addPostProcessor(new ScanlinesPostProcessor(1));
            scanlines.effect.attenuation = 0.04f;
            scanlines.effect.linesFactor = 1500f;
        }

        private void setupScripts()
        {
            _bossBattleScript = createEntity("bossScript")
                .addComponent(new BossBattleScript("boss battle script"));
        }

        public void startScreenShake(float magnitude, float duration)
        {
            _camera.startCameraShake(magnitude, duration);
        }
        
        public override void update()
        {
            base.update();

            _backgroundSprite.entity.position = _camera.camera.position;
            
            updateHud();

            updateBossActivation();

            // Update cinematic
            /*
            var cinematicAmount = Core.getGlobalManager<SystemManager>().cinematicAmount;
            if (cinematicAmount > 0)
            {
                _cinematicPostProcessor.enabled = true;
                _cinematicPostProcessor.letterboxSize = cinematicAmount;
            }
            else
            {
                _cinematicPostProcessor.enabled = false;
            }*/
        }

        private void updateBossActivation()
        {
            if (_bossBattleActive) return;
            if (Core.getGlobalManager<PlayerManager>().CoresCollected == 3)
            {
                _bossBattleActive = true;

                var player = findEntity("player");
                player.removeComponent<TiledMapMover>();
                var mover = player.addComponent(new TiledMapMover(_tiledMap.getLayer<TiledTileLayer>("bossCollision")));
                player.getComponent<PlatformerObject>().setMover(mover);

                _npcInteractionSystem.executeNpc(_bossBattleScript);
            }
        }

        public void blockPassage()
        {
            var layersToRender = _tiledMap.properties["defaultLayers"].Split(',').Select(s => s.Trim()).ToList();
            layersToRender.Add("bossBlock");
            tiledMapComponent.setLayersToRender(layersToRender.ToArray());
        }

        private void updateHud()
        {
            var camerapos = _camera.camera.position - virtualSize.ToVector2() / 2;
            for (var i = 0; i < 4; i++) _hudEntities[i].position = _hudPositions[i] + new Vector2((int)camerapos.X, (int)camerapos.Y);

            var player = findEntitiesWithTag(PLAYER)[0].getComponent<PlayerComponent>();
            var playerSubtexture = _playerHudFillSprite.subtexture;
            _playerHudFillSprite.setSubtexture(recreateSubtextureWithRate(playerSubtexture, player.hpRate()));
            _playerHudFillSprite.setOriginNormalized(Vector2.Zero);

            var boss = findEntity("boss");
            if (boss == null) return;
            var bossBattler = boss.getComponent<EnemyBossComponent>();
            var bossSubtexture = _playerHudFillSprite.subtexture;
            _bossHudFillSprite.setSubtexture(recreateSubtextureWithRate(bossSubtexture, bossBattler.hpRate()));
            _bossHudFillSprite.setOriginNormalized(Vector2.Zero);
            _hudEntities[2].enabled = bossBattler.isBattleActive;
            _hudEntities[3].enabled = bossBattler.isBattleActive;
        }

        private Subtexture recreateSubtextureWithRate(Subtexture subtexture, float rate)
        {
            var r = subtexture.sourceRect;
            r.Width = (int)(subtexture.texture2D.Width * rate);
            return new Subtexture(subtexture.texture2D, r);
        }
    }
}
