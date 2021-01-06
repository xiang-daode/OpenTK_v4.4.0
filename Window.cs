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
        private Dictionary<string, int> dLen = new Dictionary<string, int>();//存放长度
        private Dictionary<string, int> dPtr = new Dictionary<string, int>();//存放偏移
        
        //定义顶点(位置三维,纹理二维)readonly 
        private float[] _vertices=new float[0xffff*20];// =
        /*{
            // Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
        };*/

        //顶点之序号,取二个三角形拼成四边形: readonly
        private uint[] _indices=new uint[0xffff*6];// =
       /* {
            0, 1, 3,
            1, 2, 3
        };*/

        private int _elementBufferObject; //元素缓存对象
        private int _vertexBufferObject;//顶点缓存对象
        private int _vertexArrayObject;//顶点数组对象

        private Shader _shader;//渲染器

        private Matrix4 _view;//视图矩阵---“摄影机”,它表示窗口中的当前视口
        private Matrix4 _projection;//投影矩阵

        private Texture _texture1;//纹理-1
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

            //Sphere--顶点缓存处理:
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.DynamicDraw);//_vertices

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.DynamicDraw);//_indices
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            //。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。。

            //渲染器调用GPU二程序(顶点与片元):
            _shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            _shader.Use();

            //纹理-1:
            _texture1 = new Texture("Resources/container.png");
            _texture1.Use();
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
            int[] ak = { 0, 1, 1, 0 }; //水平增量
            int[] bk = { 0, 0, 1, 1 }; //垂直增量
            int ptr = 0;int vArrLen = 0;int iArrLen = 0;//指针偏移,顶点数目,索引数目
            
            #region  四边形:  
            //定义顶点(位置三维,纹理二维):
           float[] _verRect = new float[]
            {
                // Position         Texture coordinates
                 0.5f,  0.75f, 0.0f, 1.0f, 1.0f, // top right
                 0.5f, -0.75f, 0.0f, 1.0f, 0.0f, // bottom right  
                -0.5f, -0.75f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f,  0.75f, 0.0f, 0.0f, 1.0f  // top left
            };

            //顶点之序号:
             uint[]  _idxRect =new uint[]
               {
                 0, 1, 3,
                 1, 2, 3
               };
            _verRect.CopyTo(_vertices, 0);
            _idxRect.CopyTo(_indices, 0);
            vArrLen += _verRect.Length;  iArrLen += _idxRect.Length;
            dLen.Add("rect", _idxRect.Length); dPtr.Add("rect", ptr);
            ptr += (_idxRect.Length * 4);            
            #endregion

            #region  正方形: 
            //定义顶点(位置三维,纹理二维)
            float[] _verQuat = new float[]
             {
                // Position         Texture coordinates
                 0.2f,  0.2f, 0.0f, 1.0f, 1.0f, // top right
                 0.2f, -0.2f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.2f, -0.2f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.2f,  0.2f, 0.0f, 0.0f, 1.0f  // top left
             };

            //顶点之序号,取二个三角形拼成四边形: 
            uint dlt = (uint)(vArrLen / 5);
            uint[] _idxQuat = new uint[]
               {
                 dlt+0, dlt+1, dlt+3,
                 dlt+1, dlt+2, dlt+3
               };
            _verQuat.CopyTo(_vertices, vArrLen);
            _idxQuat.CopyTo(_indices, iArrLen);
            vArrLen += _verQuat.Length; iArrLen += _idxQuat.Length;
            dLen.Add("quat", _idxQuat.Length); dPtr.Add("quat", ptr);
            ptr += (_idxQuat.Length * 4);
            #endregion

            #region  菱形: 
            //定义顶点(位置三维,纹理二维)
            float[] _verSide4 = new float[]
             {
                // Position         Texture coordinates
                 0,  0, 0, 0.5f, 0.5f,
                 (float)(0.5*MathHelper.Cos(0*2*PI/4)),(float)(0.5*MathHelper.Sin(0*2*PI/4)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(0*2*PI/4)),(float)(0.5+0.5*MathHelper.Sin(0*2*PI/4)),
                 (float)(0.5*MathHelper.Cos(1*2*PI/4)),(float)(0.5*MathHelper.Sin(1*2*PI/4)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(1*2*PI/4)),(float)(0.5+0.5*MathHelper.Sin(1*2*PI/4)),
                 (float)(0.5*MathHelper.Cos(2*2*PI/4)),(float)(0.5*MathHelper.Sin(2*2*PI/4)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(2*2*PI/4)),(float)(0.5+0.5*MathHelper.Sin(2*2*PI/4)),
                 (float)(0.5*MathHelper.Cos(3*2*PI/4)),(float)(0.5*MathHelper.Sin(3*2*PI/4)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(3*2*PI/4)),(float)(0.5+0.5*MathHelper.Sin(3*2*PI/4))
             };

            //顶点之序号,取二个三角形拼成四边形: 
            dlt = (uint)(vArrLen / 5);
            uint[] _idxSide4 = new uint[]
               {
                 dlt+0, dlt+1, dlt+2,
                 dlt+0, dlt+2, dlt+3,
                 dlt+0, dlt+3, dlt+4,
                 dlt+0, dlt+4, dlt+1
               };
            _verSide4.CopyTo(_vertices, vArrLen);
            _idxSide4.CopyTo(_indices, iArrLen);
            vArrLen += _verSide4.Length; iArrLen += _idxSide4.Length;
            dLen.Add("side4", _idxSide4.Length); dPtr.Add("side4", ptr);
            ptr += (_idxSide4.Length * 4);
            #endregion

            #region  六边形: 
            //定义顶点(位置三维,纹理二维)
            float[] _verSide6 = new float[]
             {
                // Position         Texture coordinates
                 0,  0, 0, 0.5f, 0.5f, 
                 (float)(0.5*MathHelper.Cos(0*2*PI/6)),(float)(0.5*MathHelper.Sin(0*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(0*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(0*2*PI/6)),
                 (float)(0.5*MathHelper.Cos(1*2*PI/6)),(float)(0.5*MathHelper.Sin(1*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(1*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(1*2*PI/6)),
                 (float)(0.5*MathHelper.Cos(2*2*PI/6)),(float)(0.5*MathHelper.Sin(2*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(2*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(2*2*PI/6)),
                 (float)(0.5*MathHelper.Cos(3*2*PI/6)),(float)(0.5*MathHelper.Sin(3*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(3*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(3*2*PI/6)),
                 (float)(0.5*MathHelper.Cos(4*2*PI/6)),(float)(0.5*MathHelper.Sin(4*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(4*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(4*2*PI/6)),
                 (float)(0.5*MathHelper.Cos(5*2*PI/6)),(float)(0.5*MathHelper.Sin(5*2*PI/6)), 0.0f, (float)(0.5+0.5*MathHelper.Cos(5*2*PI/6)),(float)(0.5+0.5*MathHelper.Sin(5*2*PI/6)),
             };

            //顶点之序号,取二个三角形拼成四边形: 
            dlt = (uint)(vArrLen / 5);
            uint[] _idxSide6 = new uint[]
               {
                 dlt+0, dlt+1, dlt+2,
                 dlt+0, dlt+2, dlt+3,
                 dlt+0, dlt+3, dlt+4,
                 dlt+0, dlt+4, dlt+5,
                 dlt+0, dlt+5, dlt+6,
                 dlt+0, dlt+6, dlt+1
               };
            _verSide6.CopyTo(_vertices, vArrLen);
            _idxSide6.CopyTo(_indices, iArrLen);
            vArrLen += _verSide6.Length; iArrLen += _idxSide6.Length;
            dLen.Add("side6", _idxSide6.Length); dPtr.Add("side6", ptr);
            ptr += (_idxSide6.Length * 4);
            #endregion

            #region sphere---使用字典装载顶点数据与索引数据,数组大小自动计算,算法紧凑.
            //顶点赋值:
            Dictionary<P5D, int> p5d = new Dictionary<P5D, int>();
            int idx = 0;//顶点数目,每一顶点为五维数(x,y,z,u,v)
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
            float[]  _verSphere = new float[5 * idx];
            uint h = 0;
            foreach (var m in p5d)
            {
                _verSphere[5 * h + 0] = m.Key._x;
                _verSphere[5 * h + 1] = m.Key._y;
                _verSphere[5 * h + 2] = m.Key._z;
                _verSphere[5 * h + 3] = m.Key._u;
                _verSphere[5 * h + 4] = m.Key._v;
                h++;
            }
            //顶点序号赋值(6个序号来自四边形的四个顶点):
            uint[] _idxSphere= new uint[6 * idx / 4];
            dlt = (uint)(vArrLen / 5);
            for (int g = 0; g < idx / 4; g++)
            {
                _idxSphere[6 * g + 0 ] = (uint)(4 * g + 0+dlt);
                _idxSphere[6 * g + 1 ] = (uint)(4 * g + 1+ dlt);
                _idxSphere[6 * g + 2 ] = (uint)(4 * g + 3+ dlt);
                _idxSphere[6 * g + 3 ] = (uint)(4 * g + 1+ dlt);
                _idxSphere[6 * g + 4 ] = (uint)(4 * g + 2+ dlt);
                _idxSphere[6 * g + 5 ] = (uint)(4 * g + 3+ dlt);
            }
           _verSphere.CopyTo(_vertices, vArrLen);
           _idxSphere.CopyTo(_indices, iArrLen); 
            vArrLen += _verSphere.Length; iArrLen += _idxSphere.Length;
            dLen.Add("sphere", _idxSphere.Length); dPtr.Add("sphere",ptr);
            ptr +=(_idxSphere.Length * 4);
            #endregion

            #region bar---使用字典装载顶点数据与索引数据,数组大小自动计算,算法紧凑.
            //顶点赋值:
            Dictionary<P5D, int> p5d2 = new Dictionary<P5D, int>();
            int idx2 = 0;//顶点数目,每一顶点为五维数(x,y,z,u,v)
            for (int i = 1; i <= 60; i += 1)  //经线:0--360
            {
                for (int j = 1; j <= 30; j += 1) //纬线:-90--+90
                {
                    for (int s = 0; s < 4; s++)
                    {
                        float a = 2 * PI * (i - ak[s]) / 60f;//
                        float b = 2 * PI * (j - bk[s]) / 60f - PI / 2;//
                        float x = (float)(1.0f * MathHelper.Cos(a));//空间坐标x ,,,* MathHelper.Cos(b)
                        float y = (float)(1.0f * MathHelper.Sin(a));//空间坐标y,,,* MathHelper.Cos(b)
                        float z = b;// (float)(1.0f * MathHelper.Sin(b));//空间坐标z
                        float ta = (i - ak[s]) / 60f;//纹理坐标u
                        float tb = (j - bk[s]) / 30f;//纹理坐标v
                        p5d2.Add(new P5D(x, y, z, ta, tb), idx2);
                        idx2++;
                    }
                }
            }
            //顶点字典数据--->转数组 float[] _vertices
            float[] _verBar = new float[5 * idx2];
            int h2 = 0;
            foreach (var m in p5d2)
            {
                _verBar[5 * h2 + 0] = m.Key._x;
                _verBar[5 * h2 + 1] = m.Key._y;
                _verBar[5 * h2 + 2] = m.Key._z;
                _verBar[5 * h2 + 3] = m.Key._u;
                _verBar[5 * h2 + 4] = m.Key._v;
                h2++;
            }
            //顶点序号赋值(6个序号来自四边形的四个顶点):
            uint[] _idxBar= new uint[6 * idx2 / 4];
            dlt = (uint)(vArrLen / 5);
            for (int g = 0; g < idx2 / 4; g++)
            {
                _idxBar[6 * g + 0] = (uint)(4 * g + 0+ dlt) ;
                _idxBar[6 * g + 1 ] = (uint)(4 * g + 1+ dlt);
                _idxBar[6 * g + 2 ] = (uint)(4 * g + 3+ dlt);
                _idxBar[6 * g + 3 ] = (uint)(4 * g + 1+ dlt);
                _idxBar[6 * g + 4 ] = (uint)(4 * g + 2+ dlt);
                _idxBar[6 * g + 5 ] = (uint)(4 * g + 3+ dlt);
            }
            _verBar.CopyTo(_vertices, vArrLen);
            _idxBar.CopyTo(_indices, iArrLen);
            vArrLen += _verBar.Length; iArrLen += _idxBar.Length;
            dLen.Add("bar", _idxBar.Length); dPtr.Add("bar", ptr);
            ptr += (_idxBar.Length * 4);
            #endregion

        }

        //窗体渲染事件---用点,线,面方式绘制所有对象:
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);//清除位缓存

            #region  =========== 用点,线,面方式绘制所有对象 ============            
            dwRect();            
            dwQuat();           
            dwSphere();
            dwBar();
            dwSide4();
            dwSide6();
            #endregion ================================================


            SwapBuffers();//交换图像缓存
            t += 0.05f;//动画递增速率
            base.OnRenderFrame(e);//渲染帧
        }

        private void dwRect() {

            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(1.0f);
            //作旋转变换:
            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(t / 50) * 0.2f+1, (float)MathHelper.Sin(t / 50) * 0.2f, 0.0f);//作平移变换
           
            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-t));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(t / 2f));
            transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(t / 3f));         
            
            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            _texture1.Use();//使用二图片混合 
            //_texture1.Use(TextureUnit.Texture1); //使用单一图片
            //_texture2.Use(TextureUnit.Texture0);//使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            ////以点集绘制:
            //GL.PointSize(8);           
            GL.DrawElements(PrimitiveType.Triangles,dLen["rect"] , DrawElementsType.UnsignedInt, dPtr["rect"]);//_indices.Length
            int ttt = 0;
        }

        private void dwQuat()
        {

            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(1.0f);
            //作旋转变换:
            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(t / 30) * 0.2f-1, (float)MathHelper.Sin(t / 30) * 0.2f, 0.5f);//作平移变换

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(t));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / 3f));
            transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(t / 2f));

            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            //_texture1.Use();//使用二图片混合 
            _texture1.Use(TextureUnit.Texture0); //使用单一图片
            //_texture2.Use(TextureUnit.Texture1);//使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            //:
            //GL.PointSize(8); //以点集绘制             
            GL.DrawElements(PrimitiveType.Triangles, dLen["quat"], DrawElementsType.UnsignedInt, dPtr["quat"]);//_indices.Length
        }

        private void dwSphere()
        {
            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(0.4f);
            //作旋转变换:
            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(-t / 30) * 0.4f, (float)MathHelper.Sin(-t / 30) * 0.4f, -0.5f);//作平移变换

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-t/3));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / .5f));
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-t / .4f));

            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            //_texture1.Use();//使用二图片混合 
            //_texture1.Use(TextureUnit.Texture0); //使用单一图片
            _texture2.Use(TextureUnit.Texture1);//使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            //:
            GL.PointSize(8); //以点集绘制时适用
            //GL.LineWidth(4);//以线段集绘制时适用           
            GL.DrawElements(PrimitiveType.Points, dLen["sphere"], DrawElementsType.UnsignedInt, dPtr["sphere"]);//_indices.Length
        }

        private void dwBar()
        {
            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(0.4f);
            //作旋转变换:
            //transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(-t / 30) * 0.4f, (float)MathHelper.Sin(-t / 30) * 0.4f, -0.5f);//作平移变换

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            //transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-t/3));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / .5f));
            //transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(-t / .4f));

            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            //_texture1.Use();//使用二图片混合 
            //_texture1.Use(TextureUnit.Texture0); //使用单一图片
            //_texture2.Use(TextureUnit.Texture1);//使用单一图片
            _texture2.Use(TextureUnit.Texture0); //使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            //:
            //GL.PointSize(8); //以点集绘制时适用
            GL.LineWidth(4);//以线段集绘制时适用 
            GL.DrawElements(PrimitiveType.Lines, dLen["bar"], DrawElementsType.UnsignedInt, dPtr["bar"]);
        }

        private void dwSide4()
        {
            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(0.4f);
            //作旋转变换:
            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(-t / 30) * 0.4f, (float)MathHelper.Sin(-t / 30) * 0.4f, -0.75f);//作平移变换

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            //transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-t/3));
            transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / .5f));
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(t / .7f));

            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            //_texture1.Use();//使用二图片混合 
            //_texture1.Use(TextureUnit.Texture0); //使用单一图片
            //_texture2.Use(TextureUnit.Texture1);//使用单一图片
            _texture2.Use(TextureUnit.Texture0); //使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            //:
            //GL.PointSize(8); //以点集绘制时适用
            GL.LineWidth(4);//以线段集绘制时适用 
            GL.DrawElements(PrimitiveType.Triangles, dLen["side4"], DrawElementsType.UnsignedInt, dPtr["side4"]);
        }

        private void dwSide6()
        {
            Matrix4 transform = Matrix4.Identity;

            //作缩放变换:
            transform *= Matrix4.CreateScale(0.4f);
            //作旋转变换:
            transform *= Matrix4.CreateTranslation((float)MathHelper.Cos(-t / 30) * 0.4f, (float)MathHelper.Sin(-t / 30) * 0.4f, 0.75f);//作平移变换

            //依Z-Y-X顺序,绕坐标轴作旋转变换:
            //transform *= Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-t/3));
            //transform *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(-t / .5f));
            transform *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(t / .4f));

            transform *= _view;//再乘以:"眼睛,或相机"的视口矩阵
            transform *= _projection;//再乘以:投影矩阵(含有远小近大之透视效果)

            //纹理应用:
            //_texture1.Use();//使用二图片混合 
            //_texture1.Use(TextureUnit.Texture0); //使用单一图片
            //_texture2.Use(TextureUnit.Texture1);//使用单一图片
            _texture2.Use(TextureUnit.Texture0); //使用单一图片
            _shader.Use();

            //最终的变换矩阵传入给渲染器中的顶点着色器:
            _shader.SetMatrix4("transform", transform);

            //:
            //GL.PointSize(8); //以点集绘制时适用
            GL.LineWidth(4);//以线段集绘制时适用 
            GL.DrawElements(PrimitiveType.Triangles, dLen["side6"], DrawElementsType.UnsignedInt, dPtr["side6"]);
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

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            GL.DeleteProgram(_shader.Handle);
            GL.DeleteTexture(_texture1.Handle);
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