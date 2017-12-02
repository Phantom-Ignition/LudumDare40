using Nez;

namespace LudumDare40.Scenes.SceneMapExtensions
{
    public interface ISceneMapExtensionable
    {
        Scene Scene { get; set; }
        void initialize();
        void update();
    }
}
