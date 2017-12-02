using LudumDare40.Components.Map;
using LudumDare40.Managers;
using Nez;
using Nez.Particles;

namespace LudumDare40.Scenes.SceneMapExtensions
{
    class FirstRaft : ISceneMapExtensionable
    {
        public Scene Scene { get; set; }

        private ParticleEmitter _rainEmitter;
        private ParticleEmitter _secondaryRainEmitter;

        private int _flashCount;
        private float _flashInterval;

        public void initialize()
        {
            var raft = Scene.createEntity("raft");
            var player = Scene.findEntity("player");
            var bag = Scene.findEntity("Bag:0");
            raft.addComponent(new RaftComponent(player, bag));

            _rainEmitter = Scene.findEntity("Rain:0").getComponent<ParticleEmitter>();
            _rainEmitter.enabled = false;

            _secondaryRainEmitter = Scene.findEntity("Rain:1").getComponent<ParticleEmitter>();
            _secondaryRainEmitter.enabled = false;
        }

        public void update()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            if (!sysManager.getSwitch("make_it_rain") || !sysManager.getSwitch("can_rain")) return;
            _rainEmitter.enabled = true;

            _flashInterval -= Time.deltaTime;
            if (_flashInterval <= 0.0f)
            {
                _flashCount++;
                _flashInterval = 1.5f + Random.nextFloat(3.0f);
                var ease = Random.nextInt(2);
                var duration = Random.nextFloat(1.0f);
                if (ease == 0)
                {
                    Core.startCoroutine(sysManager.flashPostProcessor.animate(duration));
                }
                else
                {
                    Core.startCoroutine(sysManager.flashPostProcessor.animate(duration, Nez.Tweens.EaseType.BounceIn));
                }
                if (sysManager.getSwitch("can_rain_higher"))
                {
                    _secondaryRainEmitter.enabled = true;
                }
            }
        }
    }
}
