using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace MyFirstGame
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Menu : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private SpriteBatch spritebatch;
        private SpriteFont Font;

        
        public MenuItem[] PauseMenu;

        public Menu(Game game, int count) : base(game)
        {
            PauseMenu=new MenuItem[count];
            Initialize();
        }
        
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            spritebatch = new SpriteBatch(this.Game.GraphicsDevice);
            this.Font=((Game1)this.Game).Font;
            
            base.Initialize();
        }

        int SelectedIndex = 0;
        bool UpDown = false;
        bool DownDown = false;
        bool EnterDown = false;

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            if (Keyboard.GetState().IsKeyDown(Keys.Up)) UpDown = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Down)) DownDown = true;
            if (Keyboard.GetState().IsKeyDown(Keys.Enter)) EnterDown = true;

            if (Keyboard.GetState().IsKeyUp(Keys.Down) && DownDown)
            {
                MoveItem(1);
                DownDown = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && UpDown)
            {
                MoveItem(-1);
                UpDown = false;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.Enter) && EnterDown)
            {
                PauseMenu[SelectedIndex].Doit();
                EnterDown = false;
            }
            
            base.Update(gameTime);
        }

        private void MoveItem(int p)
        {
            PauseMenu[SelectedIndex].Selected = false;
            SelectedIndex += p;
            if (SelectedIndex < 0) SelectedIndex = PauseMenu.Length - 1; else if (SelectedIndex > PauseMenu.Length - 1) SelectedIndex = 0;
            PauseMenu[SelectedIndex].Selected = true;
        }
        public class MenuItem
        {
            
            public Action ItemFunction;
            public string Text="";
            private Color _color = Color.White;
            public Color color {    get { return _color; }  }
            
           public bool Selected {
                set
                {
                    if (value) _color = Color.Yellow; else _color = Color.White;                    
                }
            }

            public MenuItem(String text) 
            {
                Text = text;
            }
            public void Doit(){
                if (ItemFunction!=null) ItemFunction();
            }
        }
        public override void Draw(GameTime gameTime)
        {
            // TODO: Add your update code here
           

            float offset = this.Game.GraphicsDevice.Viewport.Height / 2.5f;
            const float step = 20.0f;
            foreach (MenuItem item in PauseMenu)
            {
                float center = (this.Game.GraphicsDevice.Viewport.Width) / 2;
                spritebatch.Begin();
                spritebatch.DrawString(Font, item.Text, new Vector2(center - Font.MeasureString(item.Text).X / 2, offset), item.color);
                spritebatch.End();

                offset += step;
            }           
            base.Draw(gameTime);
        }
    }
}