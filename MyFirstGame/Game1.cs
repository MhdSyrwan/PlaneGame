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
using MyFirstGame.Physics;


namespace MyFirstGame
{

    public class Mobile : Thing
    {
        public Keys FireKey = Keys.LeftControl ;
        public Texture2D fire = Game1.fire;        
        public bool Bounding = false ;
        
        private int health = 100;
        private int shots = 0;

        // AutoPilot
        private bool AutoPilot = false;
        private Thing Enemy;  

        public int Health 
        {
            set { if (value >=0 ) health=value;  }
            get { return health; }
        }

       private float GetVectorsAngle(Vector2 a,Vector2 b){
           float Res=(float)Math.Acos(
               (Double)(Vector2.Dot(a, b) / (a.Length() * b.Length())   )
               ) ;
           if (b.X < 0 && b.Y<0)
               Res -= (float)Math.PI / 2;
           return Res;
           
       }

        public void EnableAutoPilot(Mobile Enemy)
        {
            this.Control = false;
            this.AutoPilot = true;
            this.Enemy = Enemy;
        }
        public void DisableAutoPilot()
        {
            this.AutoPilot = false;
            this.Enemy = null;
        }
        void Shot(GameTime gameTime)
        {
            if (this.shots == 5)
            {
                ThrowFire(gameTime);
                shots = 0;
            }
            else
                shots++;
        }
        public override void  Update(GameTime gameTime)
        {
            
            if (AutoPilot)
            {
                Vector2 MyDir = Thing.VectorByAngle(this.Rotation);
                Vector2 EnemyDir = Thing.VectorByAngle(Enemy.Rotation);
                Vector2 TargetLine = Enemy.Position - this.Position;
                Vector2 EnemyTargetLine = this.Position - Enemy.Position;
                //float TargetAngle = GetVectorsAngle(-Vector2.UnitY, TargetLine);
                int mode = (int)((Game1)this.Game).CurrentGameMode;
                 float TargetDomainR = 0.1f / mode;
                if ((Vector2.Normalize(EnemyTargetLine) - EnemyDir).Length() < TargetDomainR)
                    this.Acceleration = -Push * (Vector2.UnitX * (float)Math.Cos(Math.PI / 2 + this.Rotation) + Vector2.UnitY * (float)Math.Sin(Math.PI / 2 + this.Rotation));
                if ((Vector2.Normalize(TargetLine) - MyDir).Length() < TargetDomainR)
                    Shot(gameTime);
                else
                {
                    float op = 1;
                    if ((Vector2.Normalize(Vector2.Normalize(TargetLine) - MyDir).Y < Vector2.Normalize(Vector2.Normalize(TargetLine) - MyDir).X) && this.Position.X < Enemy.Position.X)
                        op = -1;
                    this.Rotation += 0.07f * op;
                }
                this.Velocity -= this.Velocity * 0.01f;
                

            }
            if (Bounding)
            {
                if (this.Position.X > BoundingRect.Right )
                    this.Position.X = BoundingRect.Left;
                if (this.Position.Y > BoundingRect.Bottom)
                    this.Position.Y = BoundingRect.Top;
                else
                if (this.Position.X < BoundingRect.Left)
                    this.Position.X = BoundingRect.Right;
                if (this.Position.Y < BoundingRect.Top)
                    this.Position.Y = BoundingRect.Bottom;
            }

            if (Control)
            {
                if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(FireKey))
                {
                    Shot(gameTime);
                }
                
            }
            base.Update(gameTime);
        }
        public Mobile(Game game ,Texture2D tex, Vector2 position ,float rotation,float scale )  :base(game,tex,1,position,rotation,scale)
        {
            this.Control = true;
        }

        public void ThrowFire(GameTime gameTime)
        {
            this.Velocity -= Velocity * 0.1f ;
            Vector2 Dir = Thing.VectorByAngle(this.Rotation);
            Thing ball = new Thing(this.Game,fire, 10, this.Position + Dir *50, 0.0f, 0.2f);
            
            ball.BoundingRect = this.BoundingRect;
            ball.AutoDestroy = true;
            ball.Velocity = Dir * 30 + Dir * this.Velocity.Length() ;
            
            ball.AntiPush = 0.0f;
            ball.DrawOrder = 0;
            ball.RelativeThings.AddRange(this.RelativeThings);
            ball.RelativeThings.Add(this);

            this.Game.Components.Add(ball);


            Game1.fireSound.Play(1,0,0.0f,false);
        }

    }
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static SoundEffect fireSound;
        SoundEffect hitSound;
        public GameMode CurrentGameMode = GameMode.Easy;

        public bool F1Pressed;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 800;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        // Textures ..
        Texture2D myTexture;
        Texture2D Enemy;
        public Texture2D ExplosionMap;
        public static Texture2D fire;
        public static Texture2D Mask;
        public static Texture2D BackGround;
        // Sounds ..
        SoundEffect ExplpsionSnd;

        // Things ..
        public static Mobile Plane;
        public static Mobile EnemyPlane;

        Menu menu;
        Menu StartMenu;
        public SpriteFont Font;
        private Rectangle BoundingRectangle;

        void Resume()
        {
            this.Paused = false;
        }
        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Thing.spritebatch = spriteBatch;

            BoundingRectangle = new Rectangle(0, 0, GraphicsDevice.Viewport.Width,GraphicsDevice.Viewport.Height);
            Thing.CollisionHappend += new Thing.CollisionEventHandler(Thing_CollisionHappend);

            Font = Content.Load<SpriteFont>("Font");

            fireSound       = Content.Load<SoundEffect>("Laser");
            hitSound        = Content.Load<SoundEffect>("hit");
            ExplpsionSnd    = Content.Load<SoundEffect>("explosion");

            ExplosionMap    = Content.Load<Texture2D>("explode");
            myTexture       = Content.Load<Texture2D>("blue");
            fire            = Content.Load<Texture2D>("bullet");
            Enemy           = Content.Load<Texture2D>("red");
            BackGround      = Content.Load<Texture2D>("stars");
            
           
            // Menu ..
            menu = new Menu(this,6);

            menu.PauseMenu[0] = new Menu.MenuItem("Resume");
            menu.PauseMenu[0].Selected = true;
            menu.PauseMenu[0].ItemFunction = Resume;

            menu.PauseMenu[1] = new Menu.MenuItem("Reset(MultiPlayer)");
            menu.PauseMenu[1].ItemFunction = RestartMultiPlayer;

            menu.PauseMenu[2] = new Menu.MenuItem("Reset(Easy)");
            menu.PauseMenu[2].ItemFunction = RestartEasy;

            menu.PauseMenu[3] = new Menu.MenuItem("Reset(Medium)");
            menu.PauseMenu[3].ItemFunction = RestartMedium;

            menu.PauseMenu[4] = new Menu.MenuItem("Reset(Hard)");
            menu.PauseMenu[4].ItemFunction = RestartHard;

            menu.PauseMenu[5] = new Menu.MenuItem("Exit");
            menu.PauseMenu[5].ItemFunction = Exit;

            this.Components.Add(menu);
            menu.DrawOrder = 999;
            menu.Visible = false;

            Reset(GameMode.Easy);

           graphics.ToggleFullScreen();
           this.Paused = true;
        }


        public void RestartMultiPlayer()
        {
            Reset(GameMode.Multiplayer);
        }
        public void RestartEasy()
        {
            Reset(GameMode.Easy);
        }
        public void RestartMedium()
        {
            Reset(GameMode.Medium);
        }
        public void RestartHard()
        {
            Reset(GameMode.Hard);
        }
        public enum GameMode
        {
            Multiplayer=0,
            Easy = 1,
            Medium = 2,
            Hard = 3
        }

        public void Reset(GameMode gameMode)
        {
            this.CurrentGameMode = gameMode;
            this.Window.Title = "Planes Game Muhammad Al-Syrwan ( MhdSyrwan.wordpress.com )";
            graphics.GraphicsDevice.Reset();
            EndTimeOut = -1;
            if (Plane != null || EnemyPlane != null)
            {
                this.Components.Remove(Plane);
                this.Components.Remove(EnemyPlane);
            }

            Plane = new Mobile(this, myTexture, Vector2.One * 100, 0.0f, 1);
            EnemyPlane = new Mobile(this, Enemy, Vector2.Zero, 0.0f, 1);

            Plane.FireKey = Keys.RightControl;
            EnemyPlane.FireKey = Keys.LeftControl;

            // Relatives and Collisions ..
            Plane.RelativeThings.Add(EnemyPlane);
            EnemyPlane.RelativeThings.Add(Plane);

            // Enemy Controls
            EnemyPlane.Control = true;
            EnemyPlane.Acccelerate = Keys.W;
            EnemyPlane.BackAccelerate = Keys.S;
            EnemyPlane.TurnLeft = Keys.A;
            EnemyPlane.TurnRight = Keys.D;

            Plane.Bounding = true;
            EnemyPlane.Bounding = true;

            Plane.BoundingRect = BoundingRectangle;
            EnemyPlane.BoundingRect = BoundingRectangle;

            this.Components.Add(Plane);
            this.Components.Add(EnemyPlane);

            Plane.Velocity = Vector2.Zero;
            EnemyPlane.Velocity = Vector2.Zero;
            Plane.Position = Vector2.One * 400;
            EnemyPlane.Position = Vector2.UnitX * GraphicsDevice.Viewport.Width / 2 + Vector2.One * 400;
            if (gameMode!=GameMode.Multiplayer) EnemyPlane.EnableAutoPilot(Plane);
            Plane.Health = 100;
            EnemyPlane.Health = 100;

            Plane.Rotation = 0;
            EnemyPlane.Rotation = 0;

            Winner = Winning.none;
            Paused = false;
        }

        void Thing_CollisionHappend(Thing obj1, Thing obj2)
        {
            hitSound.Play();
            obj2.Tint = Color.Red;
            if (obj1 is Mobile && obj2 is Mobile)
            {
                
                obj1.Velocity = obj2.Velocity - obj1.Velocity;
                obj2.Velocity = obj1.Velocity - obj2.Velocity;
                
                this.Tick();
                ((Mobile)obj1).Health-=2;
                ((Mobile)obj2).Health-=2;
            }
            else
            {
                if (obj2 is Mobile)
                {
                    ((Mobile)obj2).Health-=2;
                }
                else
                {
                    Vector2 position = obj2.Position;
                    obj1.Velocity += obj2.Velocity * 0.02f;
                    this.Components.Remove(obj2);
                    Animation exp = new Animation(this, spriteBatch, ExplosionMap, 0.5f);
                    this.Components.Add(exp);
                    

                }
                if (obj1 is Mobile)
                    ((Mobile)obj1).Health-=2;
                else
                {
                    Vector2 position = obj1.Position;
                    
                    obj2.Velocity += obj1.Velocity * 0.02f;
                    this.Components.Remove(obj1);
                    Animation exp = new Animation(this, spriteBatch, ExplosionMap, 0.5f);
                    exp.Position = position;
                    this.Components.Add(exp);
                    
                }
            }
            if (!(obj1 is Mobile) && !(obj2 is Mobile))
            {
                obj1.Dispose();
                obj2.Dispose();
            }


        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        enum Winning
        {
            none=0,
            PlayerOne=1,
            PlayerTwo=3,
            Computer=2,
        }
        Winning Winner = Winning.none;

        private bool paused = false;
        public bool Paused
        {
            set
            {
                paused = value;
                Plane.Control = !value;
                menu.Visible = value;
            }
            get { return paused; }
        }

        bool PauseKeyDown =false;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                PauseKeyDown=true;
            if (Keyboard.GetState().IsKeyUp(Keys.Escape) && PauseKeyDown)
            {
                Paused =!Paused;
                PauseKeyDown = false;
            }

            if (!Paused)
            {
                // TODO: Add your update logic here
                Mobile Destroid = null;
                if (Plane.Health == 0 && Winner==Winning.none) {
                    Destroid = Plane;
                    EnemyPlane.RelativeThings.Remove(Plane);
                }
                if (EnemyPlane.Health == 0 && Winner == Winning.none)
                {
                    Destroid = EnemyPlane;
                    Plane.RelativeThings.Remove(EnemyPlane);
                }
                if (Destroid != null)
                {
                    EnemyPlane.Velocity = Vector2.Zero;
                    Plane.Velocity      = Vector2.Zero;
                    EnemyPlane.DisableAutoPilot();
                    Destroid.Visible = false;
                    Animation exp = new Animation(this, spriteBatch, ExplosionMap, 2);
                    exp.Position = Destroid.Position;
                    this.Components.Remove(Destroid);

                    this.Components.Add(exp);
                    if (Destroid == Plane)
                        if (this.CurrentGameMode != GameMode.Multiplayer)
                            Winner = Winning.Computer;
                        else Winner = Winning.PlayerTwo;
                    else
                        Winner = Winning.PlayerOne;
                    ExplpsionSnd.Play();
                    EndGame();
                }
                
                base.Update(gameTime);
            }
            else menu.Update(gameTime);
            if (EndTimeOut != -1) EndGame();
        }

        int EndTimeOut = -1;
        void EndGame()
        {
            if (EndTimeOut == 20)
            {
                this.Paused = true;
            }
            else EndTimeOut++;
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            
            // TODO: Add your drawing code here
            // drawing background ...
            spriteBatch.Begin();
            spriteBatch.Draw(BackGround, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            spriteBatch.End();

            // raising draw event to draw other objects .

            base.Draw(gameTime);
            Rectangle srcrect = new Rectangle(0, 0, myTexture.Width, myTexture.Height);
            spriteBatch.Begin();
            spriteBatch.DrawString(Font, "Plane 1 :" + Plane.Health + "%", new Vector2(10, 10), Color.White);
            String s = "Plane 2 :" + EnemyPlane.Health + "%";
            spriteBatch.DrawString(Font,s , new Vector2(GraphicsDevice.Viewport.Width-Font.MeasureString(s).X-10, 10), Color.White);
            s = "Muhammad Al-Syrwan (MhdSyrwan.wordpress.com)";
            float center = graphics.GraphicsDevice.Viewport.Width / 2;
            spriteBatch.DrawString(Font, s, new Vector2(center - Font.MeasureString(s).X / 2, 30), Color.White);
            if (Winner != Winning.none)
            {
                //Paused = true;
                s = Winner + " Won !";
                spriteBatch.DrawString(Font, s, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Width / 2), Color.White);
                
                Plane.Control = false;
                EnemyPlane.Control = false;
            }
            else
            {

                
            }
            if (Paused)
            {
                
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.AppendLine("Help :");
                sb.AppendLine("Player One Controls :");
                sb.AppendLine("Accelerate       : " + Plane.Acccelerate);
                sb.AppendLine("BackAccelerate   : " + Plane.BackAccelerate);
                sb.AppendLine("Right            : " + Plane.TurnRight);
                sb.AppendLine("Left             : " + Plane.TurnLeft);
                sb.AppendLine("Fire             : " + Plane.FireKey);
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Player Two Controls :");
                sb.AppendLine("Accelerate       : " + EnemyPlane.Acccelerate);
                sb.AppendLine("BackAccelerate   : " + EnemyPlane.BackAccelerate);
                sb.AppendLine("Right            : " + EnemyPlane.TurnRight);
                sb.AppendLine("Left             : " + EnemyPlane.TurnLeft);
                sb.AppendLine("Fire             : " + EnemyPlane.FireKey);
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("Planes Game 2010");

                spriteBatch.DrawString(Font, sb, new Vector2(20, 100), Color.LimeGreen);
                
            }
            spriteBatch.End();
            if (Paused) menu.Visible = true ;
        }

        class Animation : DrawableGameComponent
        {
            public Point frameSize = new Point(64, 64);
            public Point currentFrame = new Point(0, 0);
            public Point sheetSize = new Point(4, 4);
            public Vector2 Position = Vector2.One * 15;
            SpriteBatch spriteBatch;
            float Scale= 1.0f;
            Texture2D texture;
            public bool loop = false;

            public Animation(Game game, SpriteBatch sb, Texture2D texture , float scale): base(game) {
                spriteBatch = sb;
                this.texture = texture;
                Scale = scale;
            }


            public override void Update(GameTime gameTime)
            {
                ++currentFrame.X;
                if (currentFrame.X >= sheetSize.X)
                {
                    currentFrame.X = 0;
                    ++currentFrame.Y;
                    if (currentFrame.Y >= sheetSize.Y)
                    {
                        if (loop) currentFrame.Y = 0;
                        else this.Game.Components.Remove(this);
                    }
                }
                base.Update(gameTime);
            }

            public override void Draw(GameTime gameTime)
            {
                spriteBatch.Begin(SpriteBlendMode.AlphaBlend,SpriteSortMode.FrontToBack, SaveStateMode.None);
                spriteBatch.Draw(texture, Position,
                    new Rectangle(currentFrame.X * frameSize.X,
                        currentFrame.Y * frameSize.Y,
                        frameSize.X,
                        frameSize.Y),
                    Color.White, 0,new Vector2(frameSize.X/2,frameSize.Y/2),Scale
                    , SpriteEffects.None, 0);
                
                spriteBatch.End();
                base.Draw(gameTime);
            }
        }
    } // game1
} // namespace
