using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1920, 1080), //定义窗体宽度与高度
                Title = "LearnOpenTK - Transformations",//定义窗体标题
            };

            using (var window = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run(); //启动窗体
            }
        }
    }
}
