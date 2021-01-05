/*               OpenTK_v4.4.0 实验平台
-------- 运行环境------------------
 Win10_x64, VS2019
 AnyCPU ,AnyCPU
 .Net core 3.1  <TargetFramework>netcoreapp3.1</TargetFramework>
 也可用NuGet获取:    
<PackageReference Include="OpenTK" Version="4.4.0" />
<PackageReference Include="System.Drawing.Common" Version="5.0.0" />
         -----------来自官方例程,     修改与注释:  daode1212,2021-1-3
 
 
 
 
 
 
 
 */




using System.Collections;
using System.Collections.Generic;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;




namespace LearnOpenTK
{

    public class Window : GameWindow
    {
        private float t = 0;//动画速度控制
        private float PI = 3.14159265f;


        //定义顶点(位置三维,纹理二维)readonly 
        private float[] _vertices;// =
        /*{
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };*/

        //顶点之序号,取二个三角形拼成四边形: readonly
        private uint[] _indices;// =
       /* {
            0, 1, 3,
            1, 2, 3
        };*/

        Dictionary<string, float[]> Dp5d = new Dictionary<string, float[]>();
        Dictionary<string, uint[]> Didx = new Dictionary<string,uint[]>();


        private int _elementBufferObject0; //元素缓存对象
        private int _elementBufferObject1; //元素缓存对象
        private int _elementBufferObject2; //元素缓存对象
        private int _elementBufferObject3; //元素缓存对象
        private int _vertexBufferObject0;//顶点缓存对象
        private int _vertexBufferObject1;//顶点缓存对象
        private int _vertexBufferObject2;//顶点缓存对象
        private int _vertexBufferObject3;//顶点缓存对象

        private int _vertexArrayObject0;//顶点数组对象
        private int _vertexArrayObject1;//顶点数组对象
        private int _vertexArrayObject2;//顶点数组对象
        private int _vertexArrayObject3;//顶点数组对象



        private Shader _shader;//渲染器

        private Matrix4 _view;//视图矩阵---“摄影机”,它表示窗口中的当前视口
        private Matrix4 _projection;//投影矩阵

        private Texture _texture;//纹理-1
        private Texture _texture2;//纹理-2

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
         }

        //窗体加载,注册一些事件,数据初始化:
        protected override void OnLoad()
        {
            MouseMove += Window_MouseMove;
            MouseDown += Window_MouseDown;
            MouseUp += Window_MouseUp;
            MouseWheel += Window_MouseWheel;

            initData();//顶点,纹理数据生成

            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);//背景色
            GL.Enable(EnableCap.DepthTest);//按深度遮挡不应看见部分

            /*
            //顶点缓存处理:
            _vertexBufferObject0 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject0);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);//_vertices

            //元素缓存处理:
            _elementBufferObject0 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject0);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);//_indices
            */


            //****************************************************************

            //Quat--顶点缓存处理:
            _vertexBufferObject1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject1);
            GL.BufferData(BufferTarget.ArrayBuffer, Dp5d["quat"].Length * sizeof(float), Dp5d["quat"], BufferUsageHint.DynamicDraw);//_vertices

            //Quat--元素缓存处理:
            _elementBufferObject1 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject1);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Didx["quat"].Length * sizeof(uint), Didx["quat"], BufferUsageHint.DynamicDraw);//_indices
            _vertexArrayObject1 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexArrayObject1);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject1);
            //。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。


            //Sphere--顶点缓存处理:
            _vertexBufferObject2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject2);
            GL.BufferData(BufferTarget.ArrayBuffer, Dp5d["sphere"].Length * sizeof(float), Dp5d["sphere"], BufferUsageHint.DynamicDraw);//_vertices
                                                                                                                                  
            _elementBufferObject2 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject2);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Didx["sphere"].Length * sizeof(uint), Didx["sphere"], BufferUsageHint.DynamicDraw);//_indices
            _vertexArrayObject2 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexArrayObject2);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject2);
            //。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。

            //Bar--顶点缓存处理:
            _vertexBufferObject3 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject3);
            GL.BufferData(BufferTarget.ArrayBuffer, Dp5d["bar"].Length * sizeof(float), Dp5d["bar"], BufferUsageHint.DynamicDraw);//_vertices

            //Bar--元素缓存处理:
            _elementBufferObject3 = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject3);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Didx["bar"].Length * sizeof(uint), Didx["bar"], BufferUsageHint.DynamicDraw);//_indices

            //顶点数组对象绑定与元素数组绑定到GL内部缓存中:
            _vertexArrayObject3 = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexArrayObject3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject3);
            //。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。


            //渲染器调用GPU二程序(顶点与片元):
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            //纹理-1:
            _texture = new Texture("Resources/container.png");
            _texture.Use();
            //纹理-2:
            _texture2 = new Texture("Resources/awesomeface.png");
            _texture2.Use(TextureUnit.Texture1);

            _shader.SetInt("texture0", 0);//传入到GPU内部
            _shader.SetInt("texture1", 1);//传入到GPU内部



            //顶点位置数据类型传输:
            var vertexLocation = _shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            //顶点纹理数据类型传输:
            var texCoordLocation = _shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            //眼睛放在(0.0f, 0.0f, -4.0f)处:
            _view = Matrix4.CreateTranslation(0.0f, 0.0f, -4.0f);

            //对于投影矩阵，我们使用了一些参数:
            //视野--视角=45度。这决定了视口一次可以看到多少。45被认为是最“现实”的设置，但现在大多数视频游戏使用90
            //纵横比。这应该设置为:宽度/高度。
            //近剪裁=0.1。任何比该值更接近摄影机的顶点都将被剪裁。
            //远剪裁=100。任何比该值更远离摄影机的顶点都将被剪裁。
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100.0f);

            base.OnLoad();
        }

        //顶点,纹理数据生成:
        private void initData()
        {
            #region sphere---使用字典装载顶点数据与索引数据,数组大小自动计算,算法紧凑.

            //顶点赋值:
            Dictionary<P5D, int> p5d = new Dictionary<P5D, int>();
            int idx = 0;//顶点数目,每一顶点为五维数(x,y,z,u,v)
            int[] ak = { 0, 1, 1, 0 }; //水平增量
            int[] bk = { 0, 0, 1, 1 }; //垂直增量
            for (int i = 1; i <= 60; i += 1)  //经线:0--360
            {
                for (int j = 1; j <= 30; j += 1) //纬线:-90--+90
                {
                    for (int s = 0; s < 4; s++)
                    {
                        float a = 2 * PI * (i - ak[s]) / 60f;//
                        float b = 2 * PI * (j - bk[s]) / 60f - PI / 2;//
                        float x = (float)(1.0f * MathHelper.Cos(a) * MathHelper.Cos(b));//空间坐标x
                        float y = (float)(1.0f * MathHelper.Sin(a) * MathHelper.Cos(b));//空间坐标y
                        float z = (float)(1.0f * MathHelper.Sin(b));//空间坐标z
                        float ta = (i - ak[s]) / 60f;//纹理坐标u
                        float tb = (j - bk[s]) / 30f;//纹理坐标v
                        p5d.Add(new P5D(x, y, z, ta, tb), idx);
                        idx++;
                    }
                }
            }
            //顶点字典数据--->转数组 float[] _vertices
            _vertices = new float[5 * idx];
            int h = 0;
            foreach (var m in p5d)
            {
                _vertices[5 * h + 0] = m.Key._x;
                _vertices[5 * h + 1] = m.Key._y;
                _vertices[5 * h + 2] = m.Key._z;
                _vertices[5 * h + 3] = m.Key._u;
                _vertices[5 * h + 4] = m.Key._v;
                h++;
            }
            //顶点序号赋值(6个序号来自四边形的四个顶点):
            _indices = new uint[6 * idx / 4];
            for (int g = 0; g < idx / 4; g++)
            {
                _indices[6 * g + 0] = (uint)(4 * g + 0);
                _indices[6 * g + 1] = (uint)(4 * g + 1);
                _indices[6 * g + 2] = (uint)(4 * g + 3);
                _indices[6 * g + 3] = (uint)(4 * g + 1);
                _indices[6 * g + 4] = (uint)(4 * g + 2);
                _indices[6 * g + 5] = (uint)(4 * g + 3);
            }

            Dp5d.Add("sphere", _vertices); Didx.Add("sphere", _indices);
            #endregion

            #region  bar---直接计算的,要准确计算两类数组之元素的数目

            _vertices = new float[1800 * 20];
            _indices = new uint[1800 * 6];

            //顶点集赋值---逐一计算赋值:
            uint k = 0;
            for (int i = 0; i < 60; i += 1)
            {
                float a0 = 2 * PI * (i - 1) / 60f; float a1 = 2 * PI * i / 60f;
                for (int j = 0; j < 30; j += 1)
                {
                        float b0 =  (j - 1) / 30f -1 / 2f; float b1 =  j / 30f - 1 / 2f;

                        float x0 = (float)(0.5f * MathHelper.Cos(a0) );
                        float x1 = (float)(0.5f * MathHelper.Cos(a1) );
                        float x2 = (float)(0.5f * MathHelper.Cos(a1) );
                        float x3 = (float)(0.5f * MathHelper.Cos(a0) );

                        float y0 = (float)(0.5f * MathHelper.Sin(a0) );
                        float y1 = (float)(0.5f * MathHelper.Sin(a1) );
                        float y2 = (float)(0.5f * MathHelper.Sin(a1) );
                        float y3 = (float)(0.5f * MathHelper.Sin(a0) );

                        float z0 = (float)(1.0f*b0 );
                        float z1 = (float)(1.0f*b0 );
                        float z2 = (float)(1.0f*b1 );
                        float z3 = (float)(1.0f*b1 );

                        _vertices[k + 0] = x0; _vertices[k + 1] = y0; _vertices[k + 2] = z0; _vertices[k + 3] = i / 30f; _vertices[k + 4] = (j - 1) / 15f;
                        _vertices[k + 5] = x1; _vertices[k + 6] = y1; _vertices[k + 7] = z1; _vertices[k + 8] = (i - 1) / 30f; _vertices[k + 9] = (j - 1) / 15f;
                        _vertices[k + 10] = x2; _vertices[k + 11] = y2; _vertices[k + 12] = z2; _vertices[k + 13] = (i - 1) / 30f; _vertices[k + 14] = j / 15f;
                        _vertices[k + 15] = x3; _vertices[k + 16] = y3; _vertices[k + 17] = z3; _vertices[k + 18] = i / 30f; _vertices[k + 19] = j / 15f;

                        k += 20;
                }
            }

            //顶点索引号:
            uint ptr = 0;
            for (uint i = 0; i < 4 * k / 20; i += 4)
            {
                ptr = 6 * i / 4;
                _indices[ptr + 0] = i + 0; _indices[ptr + 1] = i + 1; _indices[ptr + 2] = i + 3;
                _indices[ptr + 3] = i + 1; _indices[ptr + 4] = i + 3; _indices[ptr + 5] = i + 2;
            }

            Dp5d.Add("bar", _vertices); Didx.Add("bar", _indices);
            #endregion

            #region quat---单一四边形的,如此赋值
            // 定义四个顶点,用于画四边形:
            _vertices = new float[20]
                {
                 // Position         Texture coordinates
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
                };

            //顶点之序号,取二个三角形拼成四边形:
            _indices = new uint[6]
          {
                0, 1, 3,
                1, 2, 3
          };
            Dp5d.Add("quat", _vertices); Didx.Add("quat", _indices);
            #endregion
        }





        //窗体渲染事件---用点,线,面方式绘制所有对象:
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);//清除位缓存

            #region  用点,线,面方式绘制所有对象：            
            dwQuat();
            //=============================================
            dwSphere();
            //=============================================
            dwBar();
            //=============================================            
            #endregion =======================


            SwapBuffers();//交换图像缓存
            t += 0.05f;//动画递增速率
            base.OnRenderFrame(e);//渲染帧
        }

        private void dwBar()
        {
            //变换矩阵先取单位化矩阵(即对角线元素全部等于1)
            var transform = Matrix4.Identity;

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(t));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(t / 2f));
            transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(t / 3f));

            //作缩放变换:
            transform *= Matrix4.CreateScale(1.1f);

            transform *= Matrix4.CreateTranslation(0.1f, 0.1f, 0.0f);//作平移变换
            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            _texture.Use();
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);           

            //以三角形为片元,绘制所有对象:
            GL.DrawElements(PrimitiveType.Triangles, Didx["bar"].Length, DrawElementsType.UnsignedInt, 0);//_indices.Length
        }


        private void dwQuat()
        {
            Matrix4 transform = Matrix4.Identity;

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(t));
            //transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / 2f));
            //transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-t / 3f));

            //作缩放变换:
            transform *= Matrix4.CreateScale(1.2f);

           //transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(-t /50f) * 1.3f, (float)MathHelper.Sin(-t / 50f) * 1.3f, 0.0f);//作平移变换
           transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
           transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            _texture.Use();
            _texture2.Use(TextureUnit.Texture1);
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);
            ////以连续线段绘制:
            //GL.LineWidth(4);
            GL.DrawElements(PrimitiveType.Triangles,Didx["quat"].Length , DrawElementsType.UnsignedInt, 0);//  _indices.Length
        }


        private void dwSphere() {

            Matrix4 transform = Matrix4.Identity;

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-t));
            //transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(t / 2f));
            //transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(t / 3f));

            //作缩放变换:
            transform *= Matrix4.CreateScale(0.1f);

            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(t / 50) * 1.2f, (float)MathHelper.Sin(t / 50) * 1.2f, 0.0f);//作平移变换
            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            _texture.Use();
            _texture2.Use(TextureUnit.Texture0);
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            ////以点集绘制:
            //GL.PointSize(8);           
            GL.DrawElements(PrimitiveType.Triangles, Didx["sphere"].Length, DrawElementsType.UnsignedInt, 0);
        }



        //鼠标滚轮事件:
        private void Window_MouseWheel(MouseWheelEventArgs obj)
        {
            //throw new System.NotImplementedException();
            this.Title = "Window_MouseWheel";
        }

        //鼠标按下事件:
        private void Window_MouseDown(MouseButtonEventArgs obj)
        {
            //throw new System.NotImplementedException();
            this.Title = "Window_MouseDown";
        }

        //鼠标移动事件:
        private void Window_MouseMove(MouseMoveEventArgs obj)
        {
            //throw new System.NotImplementedException();
            this.Title = obj.X + "," + obj.Y;
        }

        //鼠标弹起事件:
        private void Window_MouseUp(MouseButtonEventArgs obj)
        {
            //throw new System.NotImplementedException();
            this.Title = "Window_MouseUp";
        }


        //键盘事件与处理响应:
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var input = KeyboardState;
            
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            base.OnUpdateFrame(e);
        }

 
        //窗体大小变化事件:
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
        }

        //窗体卸载事件--清理内存:
        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject0);
            GL.DeleteVertexArray(_vertexArrayObject0);
            GL.DeleteBuffer(_vertexBufferObject1);
            GL.DeleteVertexArray(_vertexArrayObject1);
            GL.DeleteBuffer(_vertexBufferObject2);
            GL.DeleteVertexArray(_vertexArrayObject2);
            GL.DeleteBuffer(_vertexBufferObject3);
            GL.DeleteVertexArray(_vertexArrayObject3);

            GL.DeleteProgram(_shader.Handle);
            GL.DeleteTexture(_texture.Handle);
            GL.DeleteTexture(_texture2.Handle);

            base.OnUnload();
        }
    }

    //自定义P5D数据类型:
    class P5D
    {
          public float _x;public float _y;public float _z;
          public float _u;public float _v;
          public P5D(float x, float y, float z, float u, float v)
        {
            this._x = x;this._y = y;this._z = z;
            this._u = u;this._v = v;
        }
    }
}