using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudumDare40.Managers;
using Microsoft.Xna.Framework;
using Nez;

namespace LudumDare40.Scenes
{
    class SceneTitle : Scene
    {
        //--------------------------------------------------
        // Menu

        private List<string> _options;
        private List<Entity> _optionsList;
        private int _index;

        //----------------------//------------------------//

        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = new Color(58, 61, 101);
            createMenu();
        }

        private void createMenu()
        {
            _optionsList = new List<Entity>();
            _options =  new List<string>()
            {
                "New Game",
                "Options",
                "Quit"
            };

            var screenWidth = virtualSize.X;
            var y = virtualSize.Y - 90;

            foreach (var option in _options)
            {
                var position = new Vector2(screenWidth / 2, y);
                var entity = createEntity($"option:{option}");
                var markupText = entity.addComponent(createMarkupText(option));
                entity.position = position - markupText.width / 2 * Vector2.UnitX;
                _optionsList.Add(entity);
                y += (int)markupText.height;
            }

            _optionsList[_index].getComponent<MarkupText>().setColor(Color.Red);
        }

        private MarkupText createMarkupText(string text)
        {
            var markupText = new MarkupText();
            markupText.setTextWidth(500);
            markupText.setText(wrapText(text));
            markupText.compile();
            markupText.setTextWidth(markupText.resultTextWidth);
            return markupText;
        }

        private string wrapText(string text)
        {
            var model = "<markuptext face='default' align='center'><p>{0}</p></markuptext>";
            return string.Format(model, text);
        }

        public override void update()
        {
            base.update();

            var input = Core.getGlobalManager<InputManager>();
            var lastIndex = _index;

            if (input.UpButton.isPressed)
            {
                _index = _index - 1 < 0 ? _options.Count - 1 : _index - 1;
            }
            if (input.DownButton.isPressed)
            {
                _index = _index + 1 >= _options.Count ? 0 : _index + 1;
            }

            if (lastIndex != _index)
            {
                _optionsList[lastIndex].getComponent<MarkupText>().setColor(Color.White);
                _optionsList[_index].getComponent<MarkupText>().setColor(Color.Red);
            }
        }
    }
}
