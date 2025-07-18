using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            MyClass2 c2 = new MyClass2();

        }
    }

    public class MyClass
    {
        public event EventHandler MyEvent;

        public void Rise()
        {
            if (MyEvent != null)
                MyEvent(this, EventArgs.Empty);
        }
    }

    public class MyClass2
    {
        public MyClass2()
        {
            MyClass cls = new MyClass();
            cls.MyEvent += action;
            cls.Rise();
        }

        private void action(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
